#if UNITY_EDITOR
using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Effects.Types.Attack;
using TowerDefence.Entity.Skills.Effects.Types.Spawn;
using TowerDefence.Entity.Tower;
using TowerDefence.Map;
using TowerDefence.Projectile;
using TowerDefence.Stages;
using TowerDefence.Stats;
using UnityEditor;
using UnityEngine;
using Util.Game;
using Util.Maths;

namespace TowerDefence.Editor
{
	/// <summary>
	/// Generates a small set of example Monster/Tower/Skill/Buff/Projectile .asset files (plus two demo
	/// prefabs) under Assets/Examples/ - MVP-prep content exercising the real pipeline end to end:
	/// periodic timers, OnAttack/OnHit triggers, real ActionType.Stat/Projectile/ApplyBuff effects, real
	/// TowerAttackController targeting, real ProjectileController flight and Damage.
	///
	/// BasicTower/BasicMonster (Assets/Examples/Prefabs/) are generic, reusable for any TowerPlan/
	/// MonsterPlan - drop one of each into a scene a few units apart, assign the matching Plan asset via
	/// a small spawn script (or call TowerController.Init/MonsterController.Init directly), hit Play.
	/// No real art - primitives stand in for sprites/models on purpose.
	///
	/// Re-run via Tools/TowerDefence/Generate Example Content - overwrites the same asset paths.
	/// </summary>
	public static class ExampleContentGenerator
	{
		const string ROOT = "Assets/Examples";

		[MenuItem("Tools/TowerDefence/Generate Example Content")]
		public static void Generate()
		{
			EnsureFolder(ROOT);
			EnsureFolder($"{ROOT}/Skills");
			EnsureFolder($"{ROOT}/Monsters");
			EnsureFolder($"{ROOT}/Towers");
			EnsureFolder($"{ROOT}/Projectiles");
			EnsureFolder($"{ROOT}/Maps");
			EnsureFolder($"{ROOT}/Prefabs");

			// ===== Prefabs =====

			GameObject projectilePrefab = GetOrCreatePrefab("BasicProjectile", go =>
			{
				go.AddComponent<ProjectileController>();
				AddPrimitiveVisual(go, PrimitiveType.Sphere, 0.3f);
			});

			GameObject towerPrefab = GetOrCreatePrefab("BasicTower", go =>
			{
				go.AddComponent<TowerController>(); // owns targeting/attack timing directly now, not a separate TowerAttackController
				AddPrimitiveVisual(go, PrimitiveType.Cube, 1f);
			});

			GameObject monsterPrefab = GetOrCreatePrefab("BasicMonster", go =>
			{
				go.AddComponent<MonsterController>();
				AddPrimitiveVisual(go, PrimitiveType.Capsule, 1f);
			});

			// Self-destructs (FxAutoDestroy) so ProjectileController's fire-and-forget Instantiate calls
			// (ReleaseEffect/TrailEffect/HitEffect - none of which track the resulting instance) don't
			// leak. No real art - a shrinking sphere stand-in, same as everything else here.
			GameObject fxPrefab = GetOrCreatePrefab("FxPrefab", go =>
			{
				go.AddComponent<Effects.FxAutoDestroy>().Lifetime = 0.5f;
				AddPrimitiveVisual(go, PrimitiveType.Sphere, 0.4f);
			});

			// ===== Projectiles =====

			// Magic Fire - Ruby's shot. Slower and hits harder; ComputeWillHit weighs this against the
			// target's own Speed stat, so this is the "hits like a truck but easier to dodge" option.
			// Element/Tags live on the ProjectileAction below, not here - a ProjectilePlan is just the
			// reusable physics/visual definition (see ProjectilePlan's own doc comment), so the same
			// Firebolt shot shape could in principle be reused by a differently-elemented skill later.
			ProjectilePlan firebolt = CreateProjectile("Firebolt", projectilePrefab,
				speed: 6f, size: 0.5f, timeToDie: 3f, damage: 12, hitEffect: fxPrefab);

			// Armor Piercing Ice, straight from the design discussion's own example - Sapphire's shot.
			// Untagged Magic -> physical by definition (see Tag.cs), fast and accurate over raw power.
			ProjectilePlan iceShard = CreateProjectile("IceShard", projectilePrefab,
				speed: 14f, size: 0.4f, timeToDie: 2f, damage: 6, hitEffect: fxPrefab);

			// ===== Skills =====

			// On attacking, shreds 3 Defence off whatever it hit - a lingering corrosion effect.
			SkillPlan corrosiveBite = CreateSkill("CorrosiveBite", "Corrosive Bite", forMonster: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Defence, Value = -3 } }));

			// Every 5s, +1 Attack to itself (periodic trigger has no Target, so Stat actions fall back
			// to the caster - see StatActionHandler). Gets scarier the longer the fight runs.
			SkillPlan growingMenace = CreateSkill("GrowingMenace", "Growing Menace", forMonster: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnPeriodic, 5f) },
					actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Attack, Value = 1 } }));

			// On attacking, shreds 2 Defence off whatever it hit.
			SkillPlan armorShatter = CreateSkill("ArmorShatter", "Armor Shatter", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Defence, Value = -2 } }));

			// The actual attack, for both Towers below - TowerAttackController finds this by walking
			// InitSkills for a ProjectileAction directly (see TowerAttackController.FindAttackAction),
			// so the Trigger here is inert for firing purposes but kept as OnAttack for documentation/
			// consistency with everything else.
			SkillPlan firebreath = CreateSkill("Firebreath", "Firebreath", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = firebolt, Element = ElementType.Fire, Tags = new List<Tag> { Tag.Magic } } }));

			// Untagged -> Physical by definition (see Tag.cs) - IceShard mitigates against Defence, not
			// MagicResist, once it lands (see Entity.ApplyDamage).
			SkillPlan frostShot = CreateSkill("FrostShot", "Frost Shot", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = iceShard, Element = ElementType.Ice, Tags = new List<Tag> { Tag.ArmorPiercing } } }));

			// A BuffPlan, not a SkillPlan - attached via InitBuffs, applied once at Spawn (Entity.Spawn ->
			// Buff.Create -> ApplyBuff). +1 Attack every 3s while active. A real range-based Aura now
			// (IsAura, not just a self-buff): every other Tower within 6 units gets its own periodic
			// +1 Attack tick too, kept for 2s after leaving Range (AuraLingerDuration) instead of losing
			// it the instant they step out - see AuraPropagationService/Aura.Propagate.
			BuffPlan whetstoneAura = CreateBuff("WhetstoneAura", "Whetstone Aura", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnPeriodic, 3f) },
					actions: new List<IAction> { new Action { Type = ActionType.Stat, Stat = StatType.Attack, Value = 1 } }));
			whetstoneAura.IsAura = true;
			whetstoneAura.AuraRange = 6f;
			whetstoneAura.AuraAffectOthers = true;
			whetstoneAura.AuraLingerDuration = 2f;
			// No TargetConditions restricting this to Towers-only (it'll buff a nearby Monster too, which
			// is thematically wrong) - RaceCondition exists but its constructors are still empty stubs
			// (never stored a type, and ConditionType.Race isn't handled in EntityUtil.Check either), so
			// there's no clean "Towers only" condition to reach for yet without building that out too.

			// ===== Monsters =====

			// SpawnAction.SpawnSelf, not a fixed Monster reference - "splits into 2 half-strength copies
			// of whatever just died," which is the same SkillPlan asset regardless of species (a Slug
			// splitting works identically - just add this to its InitSkills too, no separate SkillPlan
			// needed per monster). CounterCondition gates it on CounterType.SplitDepth so it stops after
			// 2 generations rather than splitting forever: Slimey (depth 0) splits into two depth-1
			// copies, each of THOSE splits again into depth-2 copies, and depth-2 copies fail the
			// condition (2 < 2 is false) and just die normally.
			SkillPlan splitOnDeath = CreateSkill("SplitOnDeath", "Split On Death", forMonster: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnDeath) },
					conditions: new List<ICondition> { new CounterCondition(CounterType.SplitDepth, 2, MathOperation.Lesser) },
					actions: new List<IAction> { new SpawnAction { SpawnSelf = true, Quantity = 2, ScalePerDepth = 0.5 } }));

			// Speed/DodgeChance matter now - ProjectileController.ComputeWillHit reads a target's Speed
			// (relative to the shot's own) and DodgeChance directly.
			MonsterPlan slimey = CreateMonster("Slimey", MonsterType.Muddy, Monster.Race.Slime, startingHealth: 30,
				skills: new List<SkillPlan> { corrosiveBite, splitOnDeath }, speed: 2f, dodgeChance: 0, prefab: monsterPrefab, reward: 5);

			MonsterPlan direWolf = CreateMonster("DireWolf", MonsterType.DireWolf, Monster.Race.Beast, startingHealth: 50,
				skills: new List<SkillPlan> { growingMenace }, speed: 6f, dodgeChance: 10, prefab: monsterPrefab, reward: 15);

			// ===== Towers =====

			// Range/AttackSpeed are what TowerAttackController actually reads to decide who's in range
			// and how often it fires - without these a Tower has 0 range and never targets anything.
			TowerPlan ruby = CreateTower("Ruby", TowerType.Ruby,
				skills: new List<SkillPlan> { firebreath, armorShatter },
				buffs: new List<BuffPlan> { whetstoneAura },
				range: 8f, attackSpeed: 0.6f, targetting: Tower.Targetting.proximity);
			// Demo Skill Unlock (Util.Game.TowerUpgradeUtil) - after 3 total stat purchases on a given
			// Ruby, Frost Shot becomes purchasable (separately, for its own Gold cost) on that Tower.
			ruby.SkillUnlocks.Add(new TowerPlan.SkillUnlock { Threshold = 3, Skill = frostShot, Cost = 50 });

			// BasicMonster/BasicTower now "just work" dropped straight into a scene with no spawner script -
			// MonsterController/TowerController.Start builds a default Monster/Tower from these if Init was
			// never called explicitly. Mutating the already-saved prefab assets directly (GetOrCreatePrefab
			// returns the real asset, not a scratch copy) rather than re-running GetOrCreatePrefab, since
			// slimey/ruby didn't exist yet back when the prefabs were first created above.
			monsterPrefab.GetComponent<MonsterController>().DefaultPlan = slimey;
			towerPrefab.GetComponent<TowerController>().DefaultPlan = ruby;
			EditorUtility.SetDirty(monsterPrefab);
			EditorUtility.SetDirty(towerPrefab);

			TowerPlan sapphire = CreateTower("Sapphire", TowerType.Sapphire,
				skills: new List<SkillPlan> { frostShot },
				buffs: new List<BuffPlan>(),
				range: 10f, attackSpeed: 1.2f, targetting: Tower.Targetting.health);

			// Three more basics (TowerType already has a full gem roster from an earlier prototype - see
			// Tower.cs's TowerType enum - reused here rather than inventing new names) so there are 5
			// basics total to build fusion recipes out of.
			ProjectilePlan thornSpike = CreateProjectile("ThornSpike", projectilePrefab, speed: 8f, size: 0.45f, timeToDie: 2.5f, damage: 9, hitEffect: fxPrefab);
			SkillPlan thornVolley = CreateSkill("ThornVolley", "Thorn Volley", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = thornSpike, Element = ElementType.Nature } }));
			TowerPlan emerald = CreateTower("Emerald", TowerType.Emerald,
				skills: new List<SkillPlan> { thornVolley },
				buffs: new List<BuffPlan>(),
				range: 7f, attackSpeed: 0.8f, targetting: Tower.Targetting.percentageHealth);

			// Fast/weak but hits often - Electric, tagged Magic so it mitigates against MagicResist.
			ProjectilePlan sparkBolt = CreateProjectile("SparkBolt", projectilePrefab, speed: 20f, size: 0.3f, timeToDie: 1.5f, damage: 4, hitEffect: fxPrefab);
			SkillPlan staticDischarge = CreateSkill("StaticDischarge", "Static Discharge", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = sparkBolt, Element = ElementType.Electric, Tags = new List<Tag> { Tag.Magic } } }));
			TowerPlan topaz = CreateTower("Topaz", TowerType.Topaz,
				skills: new List<SkillPlan> { staticDischarge },
				buffs: new List<BuffPlan>(),
				range: 9f, attackSpeed: 2.0f, targetting: Tower.Targetting.debuffed);

			// Slow but hits like a truck - Dark/Magic, mitigates against MagicResist same as Ruby's Firebolt.
			ProjectilePlan voidBolt = CreateProjectile("VoidBolt", projectilePrefab, speed: 4f, size: 0.6f, timeToDie: 4f, damage: 18, hitEffect: fxPrefab);
			SkillPlan voidRay = CreateSkill("VoidRay", "Void Ray", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = voidBolt, Element = ElementType.Dark, Tags = new List<Tag> { Tag.Magic } } }));
			TowerPlan amethyst = CreateTower("Amethyst", TowerType.Amethyst,
				skills: new List<SkillPlan> { voidRay },
				buffs: new List<BuffPlan>(),
				range: 6f, attackSpeed: 0.4f, targetting: Tower.Targetting.health, attack: 25);

			// ===== Intermediate Towers (fusion results - 3 of the 5 basics each, to exercise
			// Util.Game.FusionUtil/the Mulligan panel's "extra pick" button end to end) =====

			ProjectilePlan prismBurst = CreateProjectile("PrismBurst", projectilePrefab, speed: 10f, size: 0.55f, timeToDie: 3f, damage: 20, hitEffect: fxPrefab);
			SkillPlan prismLance = CreateSkill("PrismLance", "Prism Lance", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = prismBurst, Element = ElementType.Light, Tags = new List<Tag> { Tag.Magic } } }));
			// Ruby + Sapphire + Emerald - fire/ice/nature converging into light.
			CreateTower("Opal", TowerType.Opal,
				skills: new List<SkillPlan> { prismLance },
				buffs: new List<BuffPlan>(),
				range: 10f, attackSpeed: 1.0f, targetting: Tower.Targetting.proximity, attack: 30,
				ingredients: new List<TowerPlan> { ruby, sapphire, emerald });

			ProjectilePlan chaosBolt = CreateProjectile("ChaosBolt", projectilePrefab, speed: 12f, size: 0.55f, timeToDie: 3f, damage: 24, hitEffect: fxPrefab);
			SkillPlan chaosBurst = CreateSkill("ChaosBurst", "Chaos Burst", forTower: true,
				effect: new Effect(
					triggers: new List<ITrigger> { new Trigger(TriggerType.OnAttack) },
					actions: new List<IAction> { new ProjectileAction { Plan = chaosBolt, Element = ElementType.Dark, Tags = new List<Tag> { Tag.Magic } } }));
			// Topaz + Amethyst + Ruby - electric/dark/fire converging into chaos.
			CreateTower("Garnet", TowerType.Garnet,
				skills: new List<SkillPlan> { chaosBurst },
				buffs: new List<BuffPlan>(),
				range: 11f, attackSpeed: 0.9f, targetting: Tower.Targetting.health, attack: 30,
				ingredients: new List<TowerPlan> { topaz, amethyst, ruby });

			// ===== Waves (first Stage - 20 Rounds of 20 Waves) =====

			// Written straight into WaveUtil.ENEMIETRIX, not a saved asset - it's a plain static
			// Dictionary (the "designer-authored in-code path" per its own doc comment), so this only
			// populates it for as long as this Editor session's domain stays loaded (re-run this command
			// after any domain reload). WorldProgress.Biome defaults to StageType.Forest (enum value 0),
			// so GameManager.StartNextWave() reads this with no other setup once a WorldProgress exists.
			// Formulaic pacing (quantity climbs with wave number, DireWolf joins from Round 3 on) - not
			// hand-tuned design, just enough real content to actually playtest wave progression against
			// the static TestMap with the two example Monsters above.
			WaveUtil.ENEMIETRIX[StageType.Forest] = new List<BiomeStage> { GenerateFirstStage(slimey, direWolf) };

			// ===== Map =====

			// One shared tile visual (a flat cube) for every cell - MapGenerator.TileColors (not a
			// separate prefab per TileType) is what actually distinguishes Wall/Floor/Buildable/etc., set
			// on the MapContainer prefab below. No real art, same as everything else here.
			GameObject tilePrefab = GetOrCreateTilePrefab("TilePrefab", PrimitiveType.Cube, new Vector3(1f, 0.2f, 1f));

			// A legitimate first test map: a width-1 Floor corridor serpentines Source (1,1) to
			// Destination (bottom-right) across full-row/connector passes, each pass's leftover width
			// filled with Buildable plots on either side - PathUtil can't walk through those (see its own
			// doc comment), so towers dropped on them actually flank the path rather than sharing it.
			// Generated (GenerateSerpentineMapRaw), not hand-typed - 16x16 is large enough that hand-typed
			// ASCII risks a silent misalignment nothing would catch until PathUtil fails to find a route.
			CreateMap("TestMap", GenerateSerpentineMapRaw(16, 16));

			GetOrCreatePrefab("MapContainer", go =>
			{
				MapGenerator generator = go.AddComponent<MapGenerator>();
				generator.TileSize = 1f;
				generator.TilePrefab = tilePrefab;
				generator.TileColors = new List<TileColorEntry>
				{
					new TileColorEntry { Type = TileType.Floor, Color = new Color(0.55f, 0.35f, 0.15f) },       // brown
					new TileColorEntry { Type = TileType.Wall, Color = new Color(0.2f, 0.2f, 0.2f) },           // dark grey
					new TileColorEntry { Type = TileType.Buildable, Color = new Color(0.6f, 0.75f, 0.4f) },     // pale green
					new TileColorEntry { Type = TileType.Source, Color = new Color(0.2f, 0.4f, 0.9f) },         // blue
					new TileColorEntry { Type = TileType.Destination, Color = new Color(0.9f, 0.75f, 0.1f) },   // gold
				};
			});

			// No "TestHarness" prefab built here on purpose - GetOrCreatePrefab's rapid scratch-
			// GameObject create/DestroyImmediate churn (this method already does it 8 times above) can
			// desync the Scene Hierarchy window's TreeView mid-repaint (ArgumentOutOfRangeException from
			// GameObjectTreeViewDataSource - a Unity Editor UI bug, not an asset-generation failure, but
			// not worth courting for one more prefab). Build the harness by hand instead - see the
			// Tools/TowerDefence menu's own log line below for exactly which components/fields.

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Debug.Log($"Example content generated under {ROOT}/ (prefabs under {ROOT}/Prefabs/). WaveUtil.ENEMIETRIX[Forest] has a full 20x20 first Stage (GameManager.StartNextWave()). " +
				"BasicMonster/BasicTower now have a DefaultPlan (Slimey/Ruby) and self-Init if dropped into a scene with no spawner script. " +
				"To play with the full pipeline: make an empty GameObject, Add Component TickManager/EntityManager/GameManager/EntityWaveManager/HudDisplay/MvpDemoSpawner, " +
				"then on MvpDemoSpawner assign TowerPrefab=BasicTower/TowerPlan=Ruby, MonsterPrefab=BasicMonster/MonsterPlan=Slimey, MapContainer=MapContainer/MapPlan=TestMap, AvailableTowers=[Ruby,Sapphire].");
		}

		// ===== Builders =====

		static SkillPlan CreateSkill(string assetName, string displayName, Effect effect, bool forMonster = false, bool forTower = false)
		{
			var plan = ScriptableObject.CreateInstance<SkillPlan>();
			plan.Name = displayName;
			plan.ForMonster = forMonster;
			plan.ForTower = forTower;
			plan.IsPositive = false;
			plan.Effects.Add(effect);
			Save(plan, $"{ROOT}/Skills/{assetName}.asset");
			return plan;
		}

		static BuffPlan CreateBuff(string assetName, string displayName, Effect effect, bool forMonster = false, bool forTower = false)
		{
			var plan = ScriptableObject.CreateInstance<BuffPlan>();
			plan.Name = displayName;
			plan.ForMonster = forMonster;
			plan.ForTower = forTower;
			plan.IsPositive = true;
			plan.Effects.Add(effect);
			Save(plan, $"{ROOT}/Skills/{assetName}.asset");
			return plan;
		}

		static ProjectilePlan CreateProjectile(string assetName, GameObject prefab, float speed, float size, float timeToDie, ddouble damage, bool homing = false, GameObject hitEffect = null)
		{
			var plan = ScriptableObject.CreateInstance<ProjectilePlan>();
			plan.Speed = speed;
			plan.Size = size;
			plan.TimeToDie = timeToDie;
			plan.Damage = damage;
			plan.Homing = homing;
			plan.ProjectilePrefab = prefab;
			plan.HitEffect = hitEffect;
			Save(plan, $"{ROOT}/Projectiles/{assetName}.asset");
			return plan;
		}

		static MonsterPlan CreateMonster(string assetName, MonsterType type, Monster.Race race, double startingHealth, List<SkillPlan> skills, float speed, float dodgeChance, GameObject prefab = null, double reward = 1)
		{
			var plan = ScriptableObject.CreateInstance<MonsterPlan>();
			plan.Name = assetName;
			plan.Type = type;
			plan.Race = race;
			plan.StartingHealth = startingHealth;
			plan.Prefab = prefab; // See MonsterPlan.Prefab - lets SpawnActionHandler spawn one with no scene reference of its own.
			plan.StatEntries.Add(new StatEntry { Type = StatType.Attack, Value = 10 });
			plan.StatEntries.Add(new StatEntry { Type = StatType.Defence, Value = 5 });
			plan.StatEntries.Add(new StatEntry { Type = StatType.Speed, Value = speed });
			plan.StatEntries.Add(new StatEntry { Type = StatType.DodgeChance, Value = dodgeChance });
			plan.StatEntries.Add(new StatEntry { Type = StatType.Reward, Value = reward }); // Gold dropped on death - see GameManager.HandleEntityEvent.
			plan.InitSkills.AddRange(skills);
			Save(plan, $"{ROOT}/Monsters/{assetName}.asset");
			return plan;
		}

		static TowerPlan CreateTower(string assetName, TowerType type, List<SkillPlan> skills, List<BuffPlan> buffs, float range, float attackSpeed, Tower.Targetting targetting, double attack = 15, List<TowerPlan> ingredients = null)
		{
			var plan = ScriptableObject.CreateInstance<TowerPlan>();
			plan.Name = assetName;
			plan.type = type;
			plan.StartingHealth = 20;
			plan.StatEntries.Add(new StatEntry { Type = StatType.Attack, Value = attack });
			plan.StatEntries.Add(new StatEntry { Type = StatType.Range, Value = range });
			plan.StatEntries.Add(new StatEntry { Type = StatType.AttackSpeed, Value = attackSpeed });
			plan.Targetting = targetting;
			plan.InitSkills.AddRange(skills);
			plan.InitBuffs.AddRange(buffs);
			// See TowerPlan.ingredients/Util.Game.FusionUtil - a fusion recipe, not a starting loadout.
			if (ingredients != null) plan.ingredients.AddRange(ingredients);
			Save(plan, $"{ROOT}/Towers/{assetName}.asset");
			return plan;
		}

		static MapPlan CreateMap(string assetName, string rawData)
		{
			var plan = ScriptableObject.CreateInstance<MapPlan>();
			plan.Name = assetName;
			plan.RawData = rawData;
			Save(plan, $"{ROOT}/Maps/{assetName}.asset");
			return plan;
		}

		/// <summary>
		/// Builds MapGenerator's RawData format (see its Legend) for a width-1 serpentine Source(1,1) ->
		/// Destination corridor: a Wall border, an interior of alternating rows - every even interior row
		/// is entirely Floor (a full-width horizontal pass, still only 1 cell *tall*, so it's no less a
		/// width-1 corridor than a single connector column is), every odd interior row is Buildable with
		/// exactly one Floor connector column, alternating which side each time. The two always connect
		/// (an odd row's single connector column is, by construction, also Floor in both neighbouring even
		/// rows, which span the full width) - reachability doesn't depend on which side alternates where,
		/// that's purely cosmetic zigzag. Destination lands whichever cell the final interior row's own
		/// connector (or, if height leaves the last interior row as a full row, its far end) resolves to.
		/// </summary>
		static string GenerateSerpentineMapRaw(int width, int height)
		{
			char[][] rows = new char[height][];
			rows[0] = Repeat('#', width);
			rows[height - 1] = Repeat('#', width);

			bool connectorOnRight = true;
			int lastInteriorRow = height - 2;
			for (int y = 1; y <= lastInteriorRow; y++)
			{
				char[] row = new char[width];
				row[0] = '#';
				row[width - 1] = '#';
				bool isFullRow = (y - 1) % 2 == 0;

				if (isFullRow)
				{
					for (int x = 1; x < width - 1; x++) row[x] = '.';
					if (y == 1) row[1] = 'S';
					if (y == lastInteriorRow) row[width - 2] = 'D';
				}
				else
				{
					for (int x = 1; x < width - 1; x++) row[x] = 'B';
					int connectorX = connectorOnRight ? width - 2 : 1;
					row[connectorX] = y == lastInteriorRow ? 'D' : '.';
					connectorOnRight = !connectorOnRight;
				}
				rows[y] = row;
			}

			string[] lines = new string[height];
			for (int y = 0; y < height; y++) lines[y] = new string(rows[y]);
			return string.Join("\n", lines);
		}

		static char[] Repeat(char c, int count)
		{
			char[] chars = new char[count];
			for (int i = 0; i < count; i++) chars[i] = c;
			return chars;
		}

		/// <summary>
		/// 20 Rounds of 20 Waves (WorldProgress.WavesPerRound/RoundsPerStage), formulaic: Slimey's
		/// quantity climbs 1 every 8 waves, DireWolf joins from Round 3 onward and climbs slower - a
		/// placeholder pacing curve, not tuned difficulty. Every SpawnChain shares the same two Monsters
		/// (no new content authored here), so this only proves wave progression/scaling actually runs
		/// end to end, not that it's fun yet.
		/// </summary>
		static BiomeStage GenerateFirstStage(MonsterPlan slimey, MonsterPlan direWolf)
		{
			BiomeStage stage = new BiomeStage();
			for (int round = 0; round < WorldProgress.RoundsPerStage; round++)
			{
				WaveRound waveRound = new WaveRound();
				for (int wave = 0; wave < WorldProgress.WavesPerRound; wave++)
				{
					int waveNumber = round * WorldProgress.WavesPerRound + wave; // 0..399
					Wave w = new Wave();
					w.Chains.Add(new SpawnChain { monster = slimey, quantity = 3 + waveNumber / 8, period = 1.5f, offset = 0f });
					if (round >= 2) w.Chains.Add(new SpawnChain { monster = direWolf, quantity = 1 + waveNumber / 20, period = 3f, offset = 1f });
					waveRound.Waves.Add(w);
				}
				stage.Rounds.Add(waveRound);
			}
			return stage;
		}

		// ===== Util =====

		static void Save(Object asset, string path)
		{
			var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
			if (existing != null) AssetDatabase.DeleteAsset(path);
			AssetDatabase.CreateAsset(asset, path);
		}

		/// <summary>Builds a temporary GameObject, lets configure add whatever components/children it needs, saves it as a prefab asset, and cleans up the scratch instance - EnsureFolder + Save's delete-then-create pattern aren't enough on their own for prefabs (SaveAsPrefabAsset needs a live GameObject to save from).</summary>
		static GameObject GetOrCreatePrefab(string assetName, System.Action<GameObject> configure)
		{
			string path = $"{ROOT}/Prefabs/{assetName}.prefab";
			GameObject scratch = new GameObject(assetName);
			try
			{
				configure(scratch);
				if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);
				return PrefabUtility.SaveAsPrefabAsset(scratch, path);
			}
			finally
			{
				Object.DestroyImmediate(scratch);
			}
		}

		/// <summary>A standalone tile marker prefab - just a scaled primitive, no controller component (unlike GetOrCreatePrefab's Tower/Monster/Projectile uses, tiles don't need one yet). Collider stripped for the same reason as AddPrimitiveVisual.</summary>
		static GameObject GetOrCreateTilePrefab(string assetName, PrimitiveType primitive, Vector3 scale)
		{
			return GetOrCreatePrefab(assetName, go =>
			{
				GameObject visual = GameObject.CreatePrimitive(primitive);
				visual.name = "Visual";
				visual.transform.SetParent(go.transform);
				visual.transform.localScale = scale;
				Object.DestroyImmediate(visual.GetComponent<Collider>());
			});
		}

		static void AddPrimitiveVisual(GameObject parent, PrimitiveType primitive, float scale)
		{
			GameObject visual = GameObject.CreatePrimitive(primitive);
			visual.name = "Visual";
			visual.transform.SetParent(parent.transform);
			visual.transform.localScale = Vector3.one * scale;
			// No physics/collision system exists yet (see TargetIng/range - all done via
			// EntityManager.GetEntitiesInRange, plain distance checks) - the primitive's own Collider
			// would just be dead weight here.
			Object.DestroyImmediate(visual.GetComponent<Collider>());
		}

		static void EnsureFolder(string path)
		{
			if (AssetDatabase.IsValidFolder(path)) return;
			string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
			string name = System.IO.Path.GetFileName(path);
			EnsureFolder(parent);
			AssetDatabase.CreateFolder(parent, name);
		}
	}
}
#endif
