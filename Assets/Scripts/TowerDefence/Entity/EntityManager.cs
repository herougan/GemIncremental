using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Maths;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;
using Util.Events;
using TowerDefence.Entity.Skills.ActionHandler;
using TowerDefence.Context;
using TowerDefence.Stats;
using TowerDefence.Entity.Monster; // for MonsterPlan
using Player; // for IEnhancementSystem - safe as a plain using here (no bare `Player` identifier referenced in this file, only the type)
// A separate, differently-named alias for the Monster class itself: EntityManager lives in the
// parent namespace TowerDefence.Entity, which also has a *nested* namespace called Monster;
// enclosing-namespace lookup finds that nested namespace before any using-directive (even a
// `using Monster = ...` alias) gets considered, so an unqualified `Monster` here always resolves to
// the namespace (CS0118) no matter how it's imported - the alias needs its own name to sidestep that.
using MonsterEntity = TowerDefence.Entity.Monster.Monster;


namespace TowerDefence.Entity
{
	[ExecuteInEditMode]
	public class EntityManager : MonoBehaviour
	{
		#region Preamble
		// #pragma warning disable UDR0001 // Domain Reload Analyzer
		public static EntityManager Instance;
		// public static PoolManager PoolManager;
		// public List<BiomeData> BiomeSpawnData;

		public Dictionary<int, LayerMask> masks;
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}

			masks = new Dictionary<int, LayerMask>()
			{
				[0] = LayerMask.GetMask("Default"),
				[1] = LayerMask.GetMask("TransparentFX"),
				[2] = LayerMask.GetMask("Ignore Raycast"),
				[3] = LayerMask.GetMask("Ignore Raycast"),
				[4] = LayerMask.GetMask("Water"),
				[5] = LayerMask.GetMask("UI"),
				[6] = LayerMask.GetMask("UI"),
				[7] = LayerMask.GetMask("UI"),
				[8] = LayerMask.GetMask("Monster"),
				[9] = LayerMask.GetMask("Tower"),
			};
		}

		void Init()
		{
			// if (PoolManager == null)
			// 	PoolManager = new PoolManager();
		}

		#endregion Preamble

		#region Position Registry

		// The one place a plain-C# IEntity gets linked to a real world Transform - Entity itself
		// deliberately carries no position (see CLAUDE.md's Entity/EntityController split). Populated by
		// EntityController.Initiate/OnDestroy; read by anything that needs a live position for an
		// IEntity it's holding, e.g. ProjectileActionHandler wiring up a homing shot's target.
		static readonly Dictionary<IEntity, Transform> entityTransforms = new();

		public static void RegisterTransform(IEntity entity, Transform transform) => entityTransforms[entity] = transform;
		public static void UnregisterTransform(IEntity entity) => entityTransforms.Remove(entity);
		public static Transform GetTransform(IEntity entity) =>
			entityTransforms.TryGetValue(entity, out var t) ? t : null;

		/// <summary>Every currently-registered Entity (Monster or Tower) - a snapshot List, not a live view, so TickManager (or anything else) can safely iterate it even if ticking one Entity causes another to die and unregister mid-loop.</summary>
		public static List<IEntity> GetAllEntities() => entityTransforms.Keys.ToList();

		/// <summary>
		/// Every registered Entity within range of position - O(every registered Entity), not real
		/// spatial partitioning (a quadtree/grid) - fine while entity counts are small, worth revisiting
		/// once TowerAttackController is actually scanning this every frame for every Tower against
		/// every Monster on screen.
		/// </summary>
		public static List<IEntity> GetEntitiesInRange(Vector3 position, float range)
		{
			List<IEntity> result = new();
			foreach (var kv in entityTransforms)
			{
				if (kv.Value == null) continue;
				if (Vector3.Distance(kv.Value.position, position) <= range) result.Add(kv.Key);
			}
			return result;
		}

		#endregion Position Registry

		#region Enhancement Systems

		// "EntityManager would have a way to collect the totality of all ApplyModTos, and cache the
		// value... upon some change, the systems become dirty, and EntityManager detects this and
		// recalculates" - per the design discussion. Every registered IEnhancementSystem's GetMods() is
		// flattened into one cached List<IStatMod> here, refreshed only when MarkEnhancementsDirty has
		// been called since the last read - not per system, per spawn. A system mutating its own data
		// (Foundry.AddPermaMod today) is responsible for calling MarkEnhancementsDirty itself; there's no
		// way for EntityManager to detect that on its own without every system exposing a change event,
		// which felt like more machinery than this needs yet - simplest thing that could plausibly work.
		static readonly List<IEnhancementSystem> enhancementSystems = new();
		static List<IStatMod> cachedEnhancementMods = new();
		static bool enhancementsDirty = true; // starts dirty - nothing's cached yet

		public static void RegisterEnhancementSystem(IEnhancementSystem system)
		{
			enhancementSystems.Add(system);
			MarkEnhancementsDirty();
		}

		public static void MarkEnhancementsDirty() => enhancementsDirty = true;

		/// <summary>
		/// Registers every currently-cached enhancement mod onto entity - call once per spawn (see
		/// EntityWaveManager.Update, right after EntityManager.SpawnMonster). Recomputes the cache first
		/// if dirty; otherwise this is just replaying an already-flattened list, not re-querying every
		/// system.
		/// </summary>
		public static void ApplyEnhancements(IEntity entity)
		{
			if (enhancementsDirty)
			{
				cachedEnhancementMods = enhancementSystems.SelectMany(s => s.GetMods()).ToList();
				enhancementsDirty = false;
			}
			foreach (IStatMod mod in cachedEnhancementMods)
			{
				entity.RegisterStatMod(mod);
			}
		}

		#endregion Enhancement Systems

		#region Wave Spawn

		public List<GameObject> monsterObjects = new List<GameObject>();
		public List<GameObject> towerObjects = new List<GameObject>();

		/// <summary>
		/// Builds a Monster from a Plan and runs it through the same lifecycle any spawned entity needs:
		/// fresh per-instance Stat/Element/Resource state, InitBuffs applied, InitSkills learned (each
		/// one's Triggers wired into WrappedActions on this Monster's events - see Skill.RegisterCallbacks),
		/// then OnSpawn fires. Pure C# - no GameObject/prefab/scene required - so this is safe to call
		/// from an Editor script or a plain test, not just at runtime.
		///
		/// Deliberately just `new Monster(plan); monster.Spawn();`, not RegisterEntityCallbacks() below:
		/// RegisterEntityCallbacks -> RegisterInitSkills iterates entity.Plan.InitSkills (a
		/// List&lt;SkillPlan&gt;) as if it already held ISkill instances, which throws InvalidCastException
		/// the moment a Plan actually has a skill on it - SkillPlan doesn't implement ISkill, only Skill
		/// does. That path is currently dead/broken and needs its own fix (or removal now that
		/// Entity.Spawn/Skill.RegisterCallbacks cover the same ground correctly) - tracked separately,
		/// not fixed here to keep this change scoped to spawning.
		/// </summary>
		public static MonsterEntity SpawnMonster(MonsterPlan plan)
		{
			MonsterEntity monster = new MonsterEntity(plan);
			monster.Spawn();
			return monster;
		}

		/// <summary>
		/// Same as SpawnMonster(plan), but scales the result to the given WorldProgress afterward -
		/// what EntityWaveManager calls so monsters get tougher as Stage/Round climb. The curve below
		/// (a flat % per Stage/Round on Health/Attack/Defence) is a placeholder purely to prove
		/// WorldProgress actually reaches spawn time - not a tuned difficulty curve.
		/// </summary>
		public static MonsterEntity SpawnMonster(MonsterPlan plan, TowerDefence.Stages.WorldProgress world)
		{
			MonsterEntity monster = SpawnMonster(plan);
			ScaleToWorld(monster, world);
			return monster;
		}

		/// <summary>
		/// Permanent, one-time scaling from total progression - "this stays," unlike a Skill/Buff's own
		/// Passives (temporary, applied/unapplied as they're granted/removed - see Skill.ApplyPassive).
		/// Scales both Base and Value by the same multiplier: Base, so anything computing a *fresh*
		/// Passive contribution later (a skill granted after this point) starts from the scaled number;
		/// Value, so whatever's already applied (InitSkills' Passives, folded in during Spawn() before
		/// this runs) scales proportionally along with everything else, rather than needing to be
		/// unapplied and reapplied to pick up the new Base.
		/// </summary>
		static void ScaleToWorld(MonsterEntity monster, TowerDefence.Stages.WorldProgress world)
		{
			if (world == null) return;

			ddouble multiplier = ComputeWaveMultiplier(world);
			ScaleStat(monster, StatType.Health, multiplier);
			ScaleStat(monster, StatType.Attack, multiplier);
			ScaleStat(monster, StatType.Defence, multiplier);
			// Gold dropped on death (see GameManager.HandleEntityEvent) scales the same way its combat
			// stats do - a Monster worth more to kill later is also worth more to have killed.
			ScaleStat(monster, StatType.Reward, multiplier);
		}

		/// <summary>Permanently multiplies both Base and Value of one stat - see ScaleToWorld's own doc comment for why both. Internal, not private: reused by SpawnActionHandler for "each split generation is weaker" scaling (ActionType.Spawn/SpawnAction.ScalePerDepth) - same operation, different caller, not worth two copies.</summary>
		internal static void ScaleStat(MonsterEntity monster, StatType type, ddouble multiplier)
		{
			monster.StatBlock.SetBase(type, monster.StatBlock.GetBase(type) * multiplier);
			monster.StatBlock.SetStat(type, monster.StatBlock.GetStat(type) * multiplier);
		}

		/// <summary>
		/// exp(1 + totalWaveNumber/100) - a small exponent by design (per the design discussion), not a
		/// tuned curve. totalWaveNumber = World*(BiomeCount*20^3) + Biome*20^3 + Stage*20^2 + Round*20 +
		/// Wave, using WorldProgress's own 0-indexed fields directly (the design discussion's own worked
		/// example used 1-indexed human-facing numbers - "biome 2" etc. - hence the "-1"s there; this only
		/// needs the same shape, not the same indexing, since WorldProgress already starts every field at
		/// 0). The World term is an outer multiplier on the whole Biome cycle, so looping back to World 1
		/// (a "New Game+" pass) is a strict difficulty step up from finishing every Biome at World 0, even
		/// though ENEMIETRIX itself doesn't key on World - see WorldProgress's own doc comment.
		/// Uses MathsLib.Exp, not System.Math.Exp - the latter overflows to double.PositiveInfinity once
		/// the exponent passes ~709 (totalWaveNumber ~70,800 with this formula), which stats should never
		/// hit since everything stat-related is ddouble specifically to avoid that ceiling.
		/// </summary>
		static ddouble ComputeWaveMultiplier(TowerDefence.Stages.WorldProgress world)
		{
			long biomeCount = System.Enum.GetValues(typeof(TowerDefence.Stages.StageType)).Length;
			long totalWaveNumber =
				(long)world.World * biomeCount * 20L * 20L * 20L +
				(long)world.Biome * 20L * 20L * 20L +
				(long)world.Stage * 20L * 20L +
				(long)world.Round * 20L +
				world.Wave;

			double exponent = 1.0 + totalWaveNumber / 100.0;
			return MathsLib.Exp(exponent);
		}

		public void KillAll()
		{
			// MonsterQueue.Clear();
			// LogManager.Instance.Log("All monsters cleared");

			// foreach (GameObject monsterObject in monsterObjects)
			// {
			// 	Destroy(monsterObject);
			// }
			// monsterObjects.Clear();
		}

		/* ===== Enemietrix ===== */
		/// <summary>
		/// Concisely stores information on what enemies will be fought.
		/// MonsterMatrix = Enemietrix[Biome][Stage]
		/// 	Consists of a List of SpawnChain Series - Randomly select one to be the round's chain series
		/// 		A SpawnChain Series is a List of SpawnChain(s) - The n-th chain describes when and what will be spawned
		/// 		It represents a different challenge each time.
		/// 		SpawnChain(Monster to be summoned, Quantity, Time offset (s), Time)
		/// 		* Note * Round 0 always spawns ChainSeries 0, which is the easiest chain. The rest are not ranked in difficulty.
		/// 
		/// Stronger chains are usually higher in the "n" 
		/// </summary>
		// public Dictionary<int, List<StageType>> stageTypeList;
		// public List<Monster> MonsterQueue = new List<Monster>();

		// Defines enemies faced // {Core Function}
		/// <summary>
		/// Huge function that generates the monsters depending on Round, Stage, and Biome
		/// </summary>
		/// <param name="world"></param>
		// public void GenerateMonsterQueue(WorldProgress world, int previous = -1)
		// {
		// 	MonsterQueue.Clear();
		// 	// Access Enemietrix
		// 	List<List<SpawnChain>> monsterMatrix = new List<List<SpawnChain>>(); // = MonsterUtil.ENEMIETRIX[world.Biome][world.Stage];
		// 	foreach (StageData stageData in BiomeSpawnData[world.Biome].Stages)
		// 	{
		// 		if (stageData.Type != world.StageType) continue;
		// 		foreach (WaveData waveData in stageData.Waves)
		// 		{
		// 			monsterMatrix.Add(waveData.SpawnChains);
		// 		}
		// 	}

		// 	// Don't replay the same thing again
		// 	int r = Random.Range(0, monsterMatrix.Count);
		// 	if (previous > 0 && r == previous)
		// 	{
		// 		--r;
		// 	}
		// 	else if (previous == 0 && r == 0) { ++r; }
		// 	List<SpawnChain> monsterChains = monsterMatrix[world.Round == 0 ? 0 : r];  // r];

		// 	// Add chains
		// 	foreach (SpawnChain chain in monsterChains)
		// 	{
		// 		// MonsterQueue.Add(world.ScaleMonster(world, new Monster(ResourceAllocater.Instance.monsterDict[chain.type])));
		// 		AddChain(world, MonsterQueue, chain);
		// 	}
		// 	// Remove last one to make Stage 0 easier but serve as a more difficult Stage n
		// 	if (world.Round == 0) MonsterQueue.RemoveAt(MonsterQueue.Count - 1);

		// 	// Sort by time
		// 	MonsterQueue.Sort((m1, m2) =>
		// 		 {
		// 			 if (m1.spawnTime > m2.spawnTime) return 1;
		// 			 if (m1.spawnTime == m2.spawnTime) return 0;
		// 			 return -1;
		// 		 }
		// 	);
		// 	LogManager.Instance.Log($"{MonsterQueue.Count} monsters queued, for {world.Stage}-{world.Round} ({world.StageType})");
		// }
		// // Add SpawnChain helper function
		// public void AddChain(WorldProgress world, List<Monster> MonsterQueue, SpawnChain chain)
		// {
		// 	for (int i = 0; i < chain.quantity; ++i)
		// 	{
		// 		Monster newt = new Monster(ResourceAllocater.Instance.MonsterDict[chain.monster.Type]);
		// 		world.Scale(newt);
		// 		newt.spawnTime = chain.period * i + chain.offset;
		// 		newt.plan.Tags.AddRange(chain.tags);
		// 		MonsterQueue.Add(newt);
		// 	}
		// }

		// public void QueueMonster(Monster monster, int time)
		// {
		// 	monster.spawnTime = time;
		// 	MonsterQueue.Add(monster);
		// }

		public void IssueMoveCommand(List<Vector3> path)
		{

		}

		public bool TrySpawn(double roundTime, Vector3 source, List<Vector3> path, float distance)
		{
			// // Try to spawn the first Monster
			// if (MonsterQueue.Count > 0 && roundTime > MonsterQueue[0].spawnTime)
			// {
			// 	LogManager.Instance.Log($"{roundTime:F2}: {MonsterQueue[0].plan.name} spawned");
			// 	Monster monster = MonsterQueue[0];
			// 	Spawn(monster, source, path, distance);
			// 	MonsterQueue.RemoveAt(0);
			// 	// Play sound
			// 	// TowerDefenceMaster.Instance.PlaySound(ResourceAllocater.Instance.playerHurtSound);
			// 	return true;
			// }
			return false;
		}

		private float periodicRescaleTime = 10.0f;
		private CountdownTimer RescaleTimer;
		public void PeriodicRescale()
		{
			// if (RescaleTimer == null)
			// {
			// 	RescaleTimer = new CountdownTimer(periodicRescaleTime);
			// 	RescaleTimer.TimerComplete += (() =>
			// 	{
			// 		foreach (Monster monster in MonsterQueue)
			// 		{
			// 			// Rescale monsters
			// 			monster.Scale();
			// 		}
			// 		LogManager.Instance.Log($"Monsters rescaled at {periodicRescaleTime:F2}s");
			// 	});
			// }
			// else
			// {
			// 	RescaleTimer.Reset();
			// }
			// RescaleTimer.Start();
		}

		#endregion Wave Spawn

		#region Events

		// Overall
		// public event Action OnAllyMonstersSpawned = delegate { };
		// public event Action OnAllyMonstersSlayed = delegate { };

		// Player
		// public event Action OnPlayerDeath = delegate { };

		// Tower
		// public event Action<Tower> OnTowerBuiltEvent = delegate { };
		// public event Action<Tower> OnTowerDestroyed = delegate { };
		// public event Action<Tower> OnTowerUpgraded = delegate { };
		// public event Action<Tower> OnTowerAttacked = delegate { };
		// public event Action<Tower> OnTowerPeriodicSkill = delegate { };
		// public event Action<Tower> OnTowerOnUltimaTrigger = delegate {};

		// Monsters

		// public event Action<Monster> OnMonsterSpawned = delegate { };
		// public event Action<Monster> OnMonsterReached = delegate { };
		// public event Action<Monster, Tower> OnMonsterHit = delegate { };
		// public event Action<Monster> OnMonsterDied = delegate { };
		// public event Action<Monster> OnMonsterOnDeathEffect = delegate { };

		// During Game
		// public void MonsterReached(Monster monster)
		// {
		// 	// Logic
		// 	GameObject reachedObject = monsterObjects.Find((m) => m.GetComponent<MonsterController>().monster == monster);
		// 	Destroy(reachedObject);

		// 	// Invoke event
		// 	OnMonsterReached.Invoke(monster);
		// }

		// public void MonsterHit(Monster monster, Tower tower)
		// {
		// 	// On hit effects activate
		// 	// monster.health -= tower.attack;
		// 	OnMonsterHit.Invoke(monster, tower);
		// }

		// public void MonsterDied(Monster monster, Tower tower)
		// {
		// 	// Logic
		// 	GameObject reachedObject = monsterObjects.Find((m) => m.GetComponent<MonsterController>().monster == monster);
		// 	Destroy(reachedObject);

		// 	OnMonsterDied.Invoke(monster);
		// }

		static void RegisterEntityCallbacks(IEntity entity)
		{
			entity.RegisterCallbacks();
			RegisterInitSkills(entity);
		}

		static void RegisterInitSkills(IEntity entity)
		{
			foreach (ISkill skill in entity.Plan.InitSkills)
			{
				RegisterSkill(entity, skill);
			}
		}

		static void RegisterSkill(IEntity entity, ISkill skill)
		{
			foreach (IEffect effect in skill.Plan.Effects)
			{
				RegisterEffect(entity, effect, skill);
			}
		}

		static void RegisterEffect(IEntity entity, IEffect effect, ISkill skill)
		{
			foreach (ITrigger trigger in effect.Triggers)
			{
				RegisterTrigger(entity, trigger, effect, skill);
			}
		}

		static void RegisterTrigger(IEntity entity, ITrigger trigger, IEffect effect, ISkill skill)
		{
			WrappedAction wrapped;
			if (trigger.Type == TriggerType.OnPeriodic)
			{
				// One dedicated timer per periodic Effect - see WrappedAction for why this can't share
				// a dictionary slot the way every other trigger type does.
				CountdownTimer timer = new CountdownTimer(trigger.Parameter, true);
				entity.AddTimer(timer);
				wrapped = new WrappedAction(entity, timer, ctx => EffectController.ApplyAction(ctx, entity, effect), skill);
			}
			else if (Trigger.IsGameTrigger(trigger.Type))
			{
				// Game-level triggers (OnGameStart, OnWaveStart, ...) aren't an Entity event at all -
				// routing these through GameManager's own event surface is still unbuilt.
				return;
			}
			else
			{
				wrapped = new WrappedAction(entity, trigger.Type, ctx => EffectController.ApplyAction(ctx, entity, effect), skill);
			}
			entity.WrappedActions.Add(wrapped);
		}

		static void DeregisterSkill(IEntity entity, ISkill skill)
		{
			// ToList() - DeregisterWrappedAction removes from entity.WrappedActions, so iterating that
			// list directly here would throw (collection modified during enumeration).
			foreach (WrappedAction wrapped in entity.WrappedActions.Where(w => w.Ref == skill).ToList())
			{
				DeregisterWrappedAction(entity, wrapped);
			}
		}

		static void DeregisterWrappedAction(IEntity entity, WrappedAction wrapped)
		{
			wrapped.Detach();
			entity.WrappedActions.Remove(wrapped);
		}

		#endregion Events

		#region Event Bookkeeping

		// EntityManager no longer *is* the bus - see Util.Events.EntityEventBus, which Entity.RaiseEvent
		// publishes to directly now. This is just one more subscriber to it, opted in via the static
		// constructor below (so it's listening the moment anything touches EntityManager, without
		// needing a live Instance/scene) purely to keep its own "how many of TriggerType X have fired,
		// what were the last few" bookkeeping - a EntityManager-specific convenience, not something the
		// bus itself needs to know about or provide for free to every subscriber.
		static EntityManager()
		{
			Util.Events.EntityEventBus.OnAnyEvent += RecordEvent;
		}

		const int RECENT_EVENT_CAPACITY = 10;
		static readonly Dictionary<TriggerType, Queue<TriggerContext>> recentEvents = new();
		static readonly Dictionary<TriggerType, int> eventCounts = new();

		static void RecordEvent(TriggerContext ctx)
		{
			if (!recentEvents.TryGetValue(ctx.TriggerType, out var queue))
			{
				queue = new Queue<TriggerContext>();
				recentEvents[ctx.TriggerType] = queue;
			}
			queue.Enqueue(ctx);
			if (queue.Count > RECENT_EVENT_CAPACITY) queue.Dequeue();

			eventCounts[ctx.TriggerType] = eventCounts.GetValueOrDefault(ctx.TriggerType) + 1;
		}

		public static IEnumerable<TriggerContext> GetRecentEvents(TriggerType type) =>
			recentEvents.TryGetValue(type, out var queue) ? queue : Array.Empty<TriggerContext>();

		public static int GetEventCount(TriggerType type) => eventCounts.GetValueOrDefault(type);

		#endregion Event Bookkeeping

		// #region Game Space
		// readonly GameObject monsterContainer;
		// readonly GameObject towerContainer;
		// readonly GameObject effectContainer;
		// readonly GameObject monsterPrefab;
		// readonly GameObject towerPrefab;

		// void Spawn(Monster monster, Vector3 spawn, List<Vector3> path, float distance)
		// {
		// 	GameObject monsterObject = Instantiate(monsterPrefab, spawn, Quaternion.identity, monsterContainer.transform);
		// 	monsterObject.name = monster.plan.name;
		// 	monsterObject.GetComponent<MonsterController>().Init(monster);
		// 	monsterObject.GetComponent<MonsterController>().MoveCommand(path, distance);
		// 	monsterObjects.Add(monsterObject);
		// }

		// public void Build(Player player, Tower.Type towerType, (int, int) coord, Vector3 spawn)
		// {
		// 	Build(player, new Tower(ResourceAllocater.Instance.TowerDict[Tower.Type.Ruby]), coord);
		// }

		// public void Build(Player player, Tower tower, (int, int) coord)
		// {
		// 	// Build
		// 	tower.coord = coord;
		// 	GameObject towerObject = Instantiate(towerPrefab, MapManager.Instance.GetTilePosition(tower.coord), Quaternion.identity, towerContainer.transform);
		// 	towerObject.GetComponent<TowerController>().Init(player.ScaleTower(tower));

		// 	// Register into players
		// 	player.towers.Add(tower);
		// 	// Add entities
		// 	towerObjects.Add(towerObject);
		// }

		// public void BuildFromPlayer(Player player)
		// {
		// 	foreach (Tower tower in player.towers)
		// 	{
		// 		Build(player, tower, tower.coord);
		// 	}
		// }

		// public bool IsBuilt((int, int) coord)
		// {
		// 	foreach (GameObject towerObject in towerObjects)
		// 	{
		// 		if (towerObject.GetComponent<TowerController>().tower.coord == coord)
		// 		{
		// 			return true;
		// 		}
		// 	}

		// 	return false;
		// }

		// public Tower GetTower((int, int) coord)
		// {
		// 	foreach (GameObject towerObject in towerObjects)
		// 	{
		// 		if (towerObject.GetComponent<TowerController>().tower.coord == coord)
		// 		{
		// 			return towerObject.GetComponent<TowerController>().tower;
		// 		}
		// 	}

		// 	return null;
		// }

		// public GameObject GetTowerObject((int, int) coord)
		// {
		// 	foreach (GameObject towerObject in towerObjects)
		// 	{
		// 		if (towerObject.GetComponent<TowerController>().tower.coord == coord)
		// 		{
		// 			return towerObject;
		// 		}
		// 	}

		// 	return null;
		// }

		// public void Display(VfxType VfxType, Vector3 spawn, float time = 10.0f)
		// {
		// 	GameObject effect = Instantiate(ResourceAllocater.Instance.EffectDict[VfxType], spawn, Quaternion.identity, effectContainer.transform);
		// 	effect.name = $"{VfxType} effect";
		// 	Destroy(effect, time);
		// }

		// #endregion Game Space
	}

}