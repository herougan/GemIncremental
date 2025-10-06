using System;
using System.Collections.Generic;
using UnityEngine;
using Util.Maths;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;
using Util.Events;
using TowerDefence.Entity.Skills.ActionHandler;


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

		#region Wave Spawn

		public List<GameObject> monsterObjects = new List<GameObject>();
		public List<GameObject> towerObjects = new List<GameObject>();

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
		// public event Action OnAllMonstersSpawned = delegate { };
		// public event Action OnAllMonstersSlayed = delegate { };

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
			// TODO
			// Get event
			entity.GetEvent(trigger.TriggerType);
			Action<object[]> triggeringEvent;
			if (trigger.Type == TriggerType.OnPeriodic)
			{
				CountdownTimer timer = entity.CreateTimer(trigger.Parameter);
				triggeringEvent = timer.OnRing + (entity);
			}
			else if (trigger.Type == TriggerType.OnValueChanged)
			{
				//
				triggeringEvent = entity.GetEvent(trigger.Type);
				// Insert the entity as the first argument in args
				WrappedAction wrapped = DelegateAdapter.Wrap(
					triggeringEvent,
					(object[] args) =>
					{
						var newArgs = new object[args.Length + 1];
						newArgs[0] = entity;
						Array.Copy(args, 0, newArgs, 1, args.Length);
						EffectController.ApplyEffect(newArgs, entity, effect);
					},
					skill
				);
			}
			else triggeringEvent = entity.GetEvent(trigger.Type);

			WrappedAction wrapped = DelegateAdapter.Wrap(triggeringEvent, (object[] args) => EffectController.ApplyEffect(args, entity, effect), skill);
			triggeringEvent += wrapped.Invoke;
		}

		static void DeregisterSkill(IEntity entity, ISkill skill)
		{
			foreach (WrappedAction wrapped in entity.WrappedActions)
			{
				if (wrapped.Ref == skill)
				{
					DeregisterWrappedAction(entity, wrapped);
				}
			}
		}

		static void DeregisterWrappedAction(IEntity entity, WrappedAction wrapped)
		{
			wrapped.Trigger -= wrapped.Invoke;
			entity.WrappedActions.Remove(wrapped);
		}

		#endregion Events

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