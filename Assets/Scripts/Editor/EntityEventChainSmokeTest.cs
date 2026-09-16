#if UNITY_EDITOR
using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Tower;
using TowerDefence.Stats;
using UnityEditor;
using UnityEngine;
using Util.Maths;

namespace TowerDefence.Editor
{
	/// <summary>
	/// Runs the Entity event-routing chain end to end with no scene, no Play Mode, no GameObjects: build
	/// a Tower + Monster purely in memory (ScriptableObject.CreateInstance, never saved to disk), spawn
	/// the Monster via EntityManager.SpawnMonster, subscribe one listener to Util.Events.EntityEventBus
	/// (standing in for GameManager.HandleEntityEvent - the exact subscription a live GameManager makes
	/// in Start()), then exercise:
	///
	///  A. OnAttack/OnHit - Tower attacks Monster, Monster hits back; ArmorShatter (OnAttack, no
	///     condition) and Thorns (OnHit) both fire through the real Skill/Effect/Trigger/
	///     EffectController chain, while Overwhelm (OnAttack, gated by a StatCondition that's false)
	///     proves Conditions actually block an Effect's Actions, not just get carried along unused.
	///  B. OnPeriodic - GrowingMenace ticks the Monster's own Attack up over time; proves periodic
	///     timers actually start counting down (AddTimer used to leave them at started=false forever).
	///  C. OnEnteredAttackRange - Ambush fires when the Monster is told the Tower entered its attack
	///     range; there's no real range/collision system yet, so this is called by hand (Entity.
	///     EnterAttackRange), same as Attack/GotHit.
	///  D. Damage pipeline - DamageCalculator walking an explicit Player root: Player's global 2x
	///     stacked with Compendium's Fire-vs-Water 1.5x matchup, on a Monster carrying a Water
	///     ElementEntry. Passes an explicit root list rather than going through the real
	///     GameManager.Instance singleton, since this test never creates one - see DamageCalculator.
	///  E. Shield/Nullifier absorb order + Mirror - runs the intended real call order (ApplyDamage
	///     first, then Attack/GotHit carrying that same Damage) and checks DamageNullifier drains
	///     before Shield, Shield before Health, Damage.FinalValue reflects what actually got through,
	///     and Mirror's ActionType.Reflect sends the right % of that back at the attacker.
	///
	/// Re-run via Tools/TowerDefence/Run Event Chain Smoke Test.
	/// </summary>
	public static class EntityEventChainSmokeTest
	{
		[MenuItem("Tools/TowerDefence/Run Event Chain Smoke Test")]
		public static void Run()
		{
			var observed = new List<TriggerContext>();
			void GlobalListener(TriggerContext ctx) => observed.Add(ctx);

			// One subscription covers every entity's every event - this is exactly what GameManager
			// does in Start() (see GameManager.HandleEntityEvent), just without a live GameManager
			// instance since this test never creates a scene/GameObject.
			Util.Events.EntityEventBus.OnAnyEvent += GlobalListener;
			try
			{
				bool pass = true;
				pass &= RunAttackAndHit(observed);
				pass &= RunPeriodic();
				pass &= RunRange(observed);
				pass &= RunDamagePipeline();
				pass &= RunShieldNullifierAndMirror();

				Debug.Log(pass
					? "[EntityEventChainSmokeTest] ALL SECTIONS PASS"
					: "[EntityEventChainSmokeTest] AT LEAST ONE SECTION FAILED - see log above");
			}
			finally
			{
				// Must unsubscribe - the bus is static, so a leaked closure here would keep firing (and
				// keep this test's Monster/Tower alive) on every future event forever.
				Util.Events.EntityEventBus.OnAnyEvent -= GlobalListener;
			}
		}

		// ===== A: OnAttack / OnHit / Conditions =====

		static bool RunAttackAndHit(List<TriggerContext> observed)
		{
			// ArmorShatter: on attacking, shreds 2 Defence off whatever the Tower hit. No Condition -
			// always fires.
			var armorShatter = ScriptableObject.CreateInstance<SkillPlan>();
			armorShatter.Name = "ArmorShatter";
			armorShatter.ForTower = true;
			armorShatter.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
				actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Defence, Value = -2 } }));

			// Overwhelm: on attacking, would shred 5 Attack off the target - but gated on the Tower's
			// own Attack being >= 999, which it never is here. Must NOT fire - proves Conditions gate
			// Actions rather than being inert data.
			var overwhelm = ScriptableObject.CreateInstance<SkillPlan>();
			overwhelm.Name = "Overwhelm";
			overwhelm.ForTower = true;
			overwhelm.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
				actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Attack, Value = -5 } },
				conditions: new List<ICondition> { new StatCondition(StatType.Attack, 999, MathOperation.Geq) }));

			// Thorns: on being hit, shreds 1 Defence off whatever hit the Monster.
			var thorns = ScriptableObject.CreateInstance<SkillPlan>();
			thorns.Name = "Thorns";
			thorns.ForMonster = true;
			thorns.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnHit) },
				actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Defence, Value = -1 } }));

			var towerPlan = ScriptableObject.CreateInstance<TowerPlan>();
			towerPlan.Name = "SmokeTestTower";
			towerPlan.StartingHealth = 20;
			towerPlan.StatEntries.Add(new StatEntry { Type = StatType.Defence, Value = 10 });
			towerPlan.StatEntries.Add(new StatEntry { Type = StatType.Attack, Value = 10 }); // well under Overwhelm's >= 999 gate
			towerPlan.InitSkills.Add(armorShatter);
			towerPlan.InitSkills.Add(overwhelm);

			var monsterPlan = ScriptableObject.CreateInstance<MonsterPlan>();
			monsterPlan.Name = "SmokeTestMonster";
			monsterPlan.StartingHealth = 30;
			monsterPlan.StatEntries.Add(new StatEntry { Type = StatType.Defence, Value = 5 });
			monsterPlan.InitSkills.Add(thorns);

			// SpawnMonster is the only "already exists" entry point asked for; there's no Tower
			// equivalent yet, so the Tower is spawned the same way SpawnMonster does it internally.
			Tower tower = new Tower(towerPlan);
			tower.Spawn();
			Monster monster = EntityManager.SpawnMonster(monsterPlan);

			ddouble towerAttackBefore = tower.GetStat(StatType.Attack);
			ddouble towerDefenceBefore = tower.GetStat(StatType.Defence);
			ddouble monsterDefenceBefore = monster.GetStat(StatType.Defence);

			// Nothing in the codebase calls both halves of a hit automatically yet (no attack-
			// resolution loop wired up - see CLAUDE.md's notes on EntityManager's wave-spawn gap). A
			// real combat tick will eventually call these together; this test does it by hand.
			tower.Attack(monster);
			monster.GotHit(tower);

			bool sawAttack = observed.Exists(c => c.TriggerType == TriggerType.OnAttack && c.Entity == tower && c.Target == monster);
			bool sawHit = observed.Exists(c => c.TriggerType == TriggerType.OnHit && c.Entity == monster && c.Target == tower);
			bool armorShatterFired = monster.GetStat(StatType.Defence) == monsterDefenceBefore - 2;
			bool thornsFired = tower.GetStat(StatType.Defence) == towerDefenceBefore - 1;
			bool overwhelmBlocked = tower.GetStat(StatType.Attack) == towerAttackBefore; // unchanged - Condition should have blocked it

			bool pass = sawAttack && sawHit && armorShatterFired && thornsFired && overwhelmBlocked;
			Debug.Log(
				$"[A: Attack/Hit/Conditions] OnAttack seen={sawAttack} OnHit seen={sawHit} " +
				$"ArmorShatter fired={armorShatterFired} Thorns fired={thornsFired} " +
				$"Overwhelm correctly blocked={overwhelmBlocked} -> {(pass ? "PASS" : "FAIL")}");
			return pass;
		}

		// ===== B: Periodic =====

		static bool RunPeriodic()
		{
			// GrowingMenace: every 2s, +1 Attack to itself (periodic trigger has no Target, so Stat
			// actions fall back to the caster - see StatActionHandler).
			var growingMenace = ScriptableObject.CreateInstance<SkillPlan>();
			growingMenace.Name = "GrowingMenace";
			growingMenace.ForMonster = true;
			growingMenace.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnPeriodic, 2f) },
				actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Attack, Value = 1 } }));

			var monsterPlan = ScriptableObject.CreateInstance<MonsterPlan>();
			monsterPlan.Name = "SmokeTestPeriodicMonster";
			monsterPlan.StartingHealth = 30;
			monsterPlan.StatEntries.Add(new StatEntry { Type = StatType.Attack, Value = 10 });
			monsterPlan.InitSkills.Add(growingMenace);

			Monster monster = EntityManager.SpawnMonster(monsterPlan);
			ddouble attackBefore = monster.GetStat(StatType.Attack);
			int activationsBefore = EntityManager.GetEventCount(TriggerType.OnSkillActivate);

			// Simulate 10 real seconds in 1s steps - well over the 2s period, generous enough to not be
			// brittle about CountdownTimer's exact fire-on-tick-after-reaching-zero timing.
			for (int i = 0; i < 10; i++) monster.Tick(1f);

			ddouble attackAfter = monster.GetStat(StatType.Attack);
			int activationsAfter = EntityManager.GetEventCount(TriggerType.OnSkillActivate);

			bool ticked = activationsAfter > activationsBefore;
			bool attackGrew = attackAfter > attackBefore;
			bool pass = ticked && attackGrew;
			Debug.Log(
				$"[B: Periodic] OnSkillActivate count {activationsBefore}->{activationsAfter} | " +
				$"Attack {attackBefore}->{attackAfter} -> {(pass ? "PASS" : "FAIL")}");
			return pass;
		}

		// ===== C: Range =====

		static bool RunRange(List<TriggerContext> observed)
		{
			// Ambush: the moment something enters this Monster's attack range, shred 1 Defence off it.
			var ambush = ScriptableObject.CreateInstance<SkillPlan>();
			ambush.Name = "Ambush";
			ambush.ForMonster = true;
			ambush.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnEnteredAttackRange) },
				actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Defence, Value = -1 } }));

			var monsterPlan = ScriptableObject.CreateInstance<MonsterPlan>();
			monsterPlan.Name = "SmokeTestAmbushMonster";
			monsterPlan.StartingHealth = 30;
			monsterPlan.InitSkills.Add(ambush);

			var towerPlan = ScriptableObject.CreateInstance<TowerPlan>();
			towerPlan.Name = "SmokeTestRangeTower";
			towerPlan.StartingHealth = 20;
			towerPlan.StatEntries.Add(new StatEntry { Type = StatType.Defence, Value = 8 });

			Monster monster = EntityManager.SpawnMonster(monsterPlan);
			Tower tower = new Tower(towerPlan);
			tower.Spawn();

			ddouble towerDefenceBefore = tower.GetStat(StatType.Defence);

			// No range/collision system exists yet to call this automatically - see Entity.
			// EnterAttackRange.
			monster.EnterAttackRange(tower);

			bool sawRange = observed.Exists(c => c.TriggerType == TriggerType.OnEnteredAttackRange && c.Entity == monster && c.Target == tower);
			bool ambushFired = tower.GetStat(StatType.Defence) == towerDefenceBefore - 1;
			bool pass = sawRange && ambushFired;
			Debug.Log($"[C: Range] OnEnteredAttackRange seen={sawRange} Ambush fired={ambushFired} -> {(pass ? "PASS" : "FAIL")}");
			return pass;
		}

		// ===== D: Damage pipeline =====

		static bool RunDamagePipeline()
		{
			var monsterPlan = ScriptableObject.CreateInstance<MonsterPlan>();
			monsterPlan.Name = "SmokeTestWaterMonster";
			monsterPlan.StartingHealth = 100;
			monsterPlan.ElementEntries.Add(new ElementEntry { Type = ElementType.Water });

			var towerPlan = ScriptableObject.CreateInstance<TowerPlan>();
			towerPlan.Name = "SmokeTestDamageTower";
			towerPlan.StartingHealth = 20;

			Monster monster = EntityManager.SpawnMonster(monsterPlan);
			Tower tower = new Tower(towerPlan);
			tower.Spawn();

			// Explicit root, not GameManager.Instance.Player - this test never creates a GameManager/
			// scene. In a real match, GameManager.Instance.Player would be this same node automatically
			// (see DamageCalculator.GetRoots), no call-site changes needed.
			var player = new Player.Player { GlobalDamageMultiplier = 2 };

			var damage = new Damage(StatType.Health, 10, tower);
			damage.SetTarget(monster);
			damage.SetElement(ElementType.Fire);

			ddouble result = DamageCalculator.Calculate(damage, new IDamageModifier[] { player });
			// 10 base * 2 (Player.GlobalDamageMultiplier) * 1.5 (Compendium's default Fire-vs-Water entry)
			ddouble expected = 30;
			bool pass = result == expected;
			Debug.Log($"[D: Damage pipeline] 10 base * 2 (Player) * 1.5 (Compendium Fire-vs-Water) = {result} (expected {expected}) -> {(pass ? "PASS" : "FAIL")}");
			return pass;
		}

		// ===== E: Shield/Nullifier absorb order + Mirror reflect, via the intended real call order =====

		static bool RunShieldNullifierAndMirror()
		{
			// Mirror: on being hit, reflects a % (its own Reflect stat) of what actually got through
			// back at whoever hit it.
			var mirror = ScriptableObject.CreateInstance<SkillPlan>();
			mirror.Name = "Mirror";
			mirror.ForMonster = true;
			mirror.Effects.Add(new Effect(
				triggers: new List<ITrigger> { new Trigger(TriggerType.OnHit) },
				actions: new List<IAction> { new Action { Type = ActionType.Reflect } }));

			var monsterPlan = ScriptableObject.CreateInstance<MonsterPlan>();
			monsterPlan.Name = "SmokeTestMirrorMonster";
			monsterPlan.StartingHealth = 100;
			monsterPlan.StatEntries.Add(new StatEntry { Type = StatType.DamageNullifier, Value = 3 });
			monsterPlan.StatEntries.Add(new StatEntry { Type = StatType.Shield, Value = 5 });
			monsterPlan.StatEntries.Add(new StatEntry { Type = StatType.Reflect, Value = 0.5 });
			monsterPlan.InitSkills.Add(mirror);

			var towerPlan = ScriptableObject.CreateInstance<TowerPlan>();
			towerPlan.Name = "SmokeTestMirrorTower";
			towerPlan.StartingHealth = 50; // no Shield/Nullifier - takes the reflect straight to Health

			Monster monster = EntityManager.SpawnMonster(monsterPlan);
			Tower tower = new Tower(towerPlan);
			tower.Spawn();

			ddouble monsterHealthBefore = monster.StatBlock.GetCurrent(StatType.Health);
			ddouble towerHealthBefore = tower.StatBlock.GetCurrent(StatType.Health);

			// The intended real order per the design discussion: ApplyDamage happens first (a
			// projectile touching its target, before any Skill/Effect system gets involved), then
			// Attack/GotHit fire carrying that same Damage object so anything reacting to the hit
			// (Mirror included) can see what actually happened.
			var damage = new Damage(StatType.Health, 20, tower); // School defaults to Physical -> DamageNullifier, not SpellNullifier
			damage.SetTarget(monster);
			monster.ApplyDamage(damage);
			tower.Attack(monster, damage);
			monster.GotHit(tower, damage);

			// 20 base -> 3 absorbed by DamageNullifier -> 17 -> 5 absorbed by Shield -> 12 hits Health.
			bool nullifierDrained = monster.StatBlock.GetCurrent(StatType.DamageNullifier) == 0;
			bool shieldDrained = monster.StatBlock.GetCurrent(StatType.Shield) == 0;
			bool finalValueCorrect = damage.FinalValue == 12;
			bool monsterHealthCorrect = monster.StatBlock.GetCurrent(StatType.Health) == monsterHealthBefore - 12;
			// Mirror reflects 50% of FinalValue (12) = 6, straight to Health since the Tower has no
			// Shield/Nullifier of its own.
			bool mirrorReflected = tower.StatBlock.GetCurrent(StatType.Health) == towerHealthBefore - 6;

			bool pass = nullifierDrained && shieldDrained && finalValueCorrect && monsterHealthCorrect && mirrorReflected;
			Debug.Log(
				$"[E: Shield/Nullifier/Mirror] Nullifier drained={nullifierDrained} Shield drained={shieldDrained} " +
				$"FinalValue={damage.FinalValue} (expected 12) Monster Health correct={monsterHealthCorrect} " +
				$"Mirror reflected 6 to Tower={mirrorReflected} -> {(pass ? "PASS" : "FAIL")}");
			return pass;
		}
	}
}
#endif
