using System;
using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Tower;
using TowerDefence.Stages;
using TowerDefence.Stats;
using UnityEngine;
using Util.Game;
using Util.Maths;
// Aliased, not a plain `using Player;` - Player is a *top-level* namespace containing a class of the
// same name (`namespace Player { public class Player }`), so an unqualified `Player` anywhere in the
// project always resolves to the namespace, not the class (global-namespace member lookup wins over
// using-directives, same CS0118 issue EntityManager.SpawnMonster hit with TowerDefence.Entity.Monster).
using PlayerModel = Player.Player;

namespace TowerDefence.Manager
{
	public class GameManager : MonoBehaviour, IDamageModifier
	{
		#region Preamble
		public static GameManager Instance { get; private set; }

		// Plain C# class, constructed here rather than self-instantiating - see Player.cs for why it
		// isn't a MonoBehaviour/singleton itself.
		public PlayerModel Player { get; private set; }

		// Same reasoning as Player - plain C# progression state, not a MonoBehaviour, owned here.
		public WorldProgress WorldProgress { get; private set; }

		// RoundManager is its own MonoBehaviour singleton (like EntityManager/EntityWaveManager - it
		// needs its own Update()), not something GameManager constructs; this is just the convenient
		// "GameManager has a RoundManager" access point the design discussion asked for. Fully qualified
		// on the right-hand side - a property named the same as its own type shadows the type name for
		// unqualified lookup, so a bare `RoundManager.Instance` here would recurse into this property's
		// own getter instead of reaching the static Instance.
		public RoundManager RoundManager => TowerDefence.Manager.RoundManager.Instance;

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

			bool firstInit = Player == null;
			Player ??= new PlayerModel();
			WorldProgress ??= new WorldProgress();

			// Foundry is the first IEnhancementSystem - future ones (Achievements, Compendium, Forge,
			// Collection, Research Centre, ...) register here the same way as they're built.
			if (firstInit) EntityManager.RegisterEnhancementSystem(Player.Foundry);
		}

		/// <summary>
		/// Demonstrates the intended call shape - "GameManager calls EntityWaveManager.SpawnWave()" -
		/// not a real wave-start trigger (nothing decides yet when a wave *should* start: player
		/// readiness, a UI button, a timer are all still open). Loads from the matrix by default; swap
		/// to EntityWaveManager.LoadFromFile(waveDataAssets) for the data-file path. Advances
		/// WorldProgress to the next Wave (rolling into the next Round/Stage as needed) after spawning -
		/// so the wave that just got requested is still the one WorldProgress pointed at when it did.
		/// </summary>
		public void StartNextWave()
		{
			List<SpawnChain> wave = EntityWaveManager.Instance.LoadFromMatrix(WorldProgress);
			EntityWaveManager.Instance.SpawnWave(wave, WorldProgress);
			WorldProgress.AdvanceWave();
		}

		// Wiring (subscribing to other systems) happens in Start(), not Awake(): Awake is for a
		// MonoBehaviour to set up its own state (here, claiming Instance) - reaching out to other
		// singletons belongs in Start, once every Awake in the scene is guaranteed to have already run.
		// EntityEventBus.OnAnyEvent is static so there's no Instance-null race to dodge here either way,
		// but keeping the convention consistent matters more as more managers subscribe.
		void Start()
		{
			Util.Events.EntityEventBus.OnAnyEvent += HandleEntityEvent;
		}

		void OnDestroy()
		{
			Util.Events.EntityEventBus.OnAnyEvent -= HandleEntityEvent;
		}

		public GameContext GameContext { get; private set; }

		#endregion Preamble

		#region Events
		public event Action<TriggerContext> OnMonsterDeath = delegate { };
		public event Action<TriggerContext> OnTowerDeath = delegate { };
		public event Action<TriggerContext> OnMonsterReached = delegate { };
		public event Action<TriggerContext> OnMonsterHit = delegate { };
		public event Action<TriggerContext> OnNumberOfMonstersChanged = delegate { };

		#endregion Events

		#region Event Routing

		/// <summary>
		/// Turns "every event, from every entity, with no idea who's listening" (see
		/// EntityEventBus.OnAnyEvent) into GameManager's own named, semantic events.
		/// Nothing upstream of this (Entity, Skill, EffectController, WrappedAction) references
		/// GameManager at all - a monster's OnHit trigger doesn't know or care that GameManager exists,
		/// it just calls RaiseEvent. GameManager decided "OnHit matters to me" by subscribing here, and
		/// it - not the triggerer - is what decides what happens next: log it, count it, re-broadcast it.
		///
		/// Downstream systems that want to react to a threshold (e.g. a future MapManager wanting
		/// "every N monsters hit, trigger something") should subscribe to GameManager's OnMonsterHit and
		/// keep their own counter there, rather than this switch growing a special case per consumer.
		/// </summary>
		void HandleEntityEvent(TriggerContext ctx)
		{
			// Unconditional, same as EntityManager's own eventCounts - proves GameManager "knows and
			// counts" every event through EM's broadcast alone, not just the TriggerTypes named below.
			eventCounts[ctx.TriggerType] = eventCounts.GetValueOrDefault(ctx.TriggerType) + 1;

			switch (ctx.TriggerType)
			{
				case TriggerType.OnHit when ctx.Entity is Monster:
					OnMonsterHit.Invoke(ctx);
					break;
				case TriggerType.OnDeath when ctx.Entity is Monster:
					// Gold drop - StatType.Reward, already scaled by WorldProgress alongside Health/
					// Attack/Defence (see EntityManager.ScaleToWorld), so a killed Monster is worth more
					// the further progression has gone, same as it is tougher to kill.
					Player.Gold.Add(ctx.Entity.GetStat(StatType.Reward));
					OnMonsterDeath.Invoke(ctx);
					break;
				case TriggerType.OnDeath when ctx.Entity is Tower:
					OnTowerDeath.Invoke(ctx);
					break;
				case TriggerType.OnReached when ctx.Entity is Monster:
					OnMonsterReached.Invoke(ctx);
					break;
			}
		}

		readonly Dictionary<TriggerType, int> eventCounts = new();
		public int GetEventCount(TriggerType type) => eventCounts.GetValueOrDefault(type);

		#endregion Event Routing

		#region IDamageModifier

		// The "Game" root DamageCalculator walks alongside Player/Map/Boss (see DamageModifier.cs) -
		// no game-wide damage modifiers exist yet (difficulty scaling, event buffs, ...); this is the
		// extension point for them once they do.
		public ddouble GetMultiplier(Damage damage) => 1;
		public IEnumerable<IDamageModifier> Children => Array.Empty<IDamageModifier>();

		#endregion IDamageModifier
	}
}
