using System.Collections.Generic;
using System.Linq;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Skills;
using UnityEngine;
using Util.Game;

namespace TowerDefence.Stages
{
	/// <summary>
	/// The spawner - decides when and which Monsters get spawned, and is the one place that answers "how
	/// many monsters are left" (see MonstersRemainingToSpawn/MonstersAlive below). EntityManager.
	/// SpawnMonster is the mechanism it calls into (builds one Monster and runs its lifecycle) but
	/// EntityManager itself doesn't decide timing/quantity/waves - that's this class.
	///
	/// A wave is a List&lt;SpawnChain&gt; (Monster/quantity/period/offset/tags - see Util.Game.WaveUtil),
	/// loaded either from the in-code matrix (WaveUtil.ENEMIETRIX, keyed by WorldProgress - the
	/// designer-authored path) or from WaveData ScriptableObject assets via WaveUtil.LoadWaveData (the
	/// data-file path - the one to build on for player-made content later). SpawnWave doesn't care which
	/// produced the list, since both paths converge on the same SpawnChain shape.
	/// </summary>
	public class EntityWaveManager : MonoBehaviour
	{
		public static EntityWaveManager Instance { get; private set; }

		void Awake()
		{
			if (Instance == null) Instance = this;
			else if (Instance != this) Destroy(this);
		}

		// Subscribe(type, ...) twice, not OnAnyEvent - this only ever cared about two TriggerTypes, so it
		// no longer runs on every other event in the game (stat changes, attacks, buff ticks, ...) just
		// to immediately discard almost all of them.
		void OnEnable()
		{
			Util.Events.EntityEventBus.Subscribe(TriggerType.OnDeath, OnMonsterLeftTheField);
			Util.Events.EntityEventBus.Subscribe(TriggerType.OnReached, OnMonsterLeftTheField);
		}

		void OnDisable()
		{
			Util.Events.EntityEventBus.Unsubscribe(TriggerType.OnDeath, OnMonsterLeftTheField);
			Util.Events.EntityEventBus.Unsubscribe(TriggerType.OnReached, OnMonsterLeftTheField);
		}

		// Current wave's bookkeeping - SpawnChains/MonsterCount are the existing RoundContext fields,
		// reused rather than duplicated. RoundContext used to have a Resolve(string) stub for loading a
		// whole round from a serialized blob - deleted (it was an empty no-op nothing called); the
		// per-wave file-loading path is EntityWaveManager.LoadFromFile below instead.
		public RoundContext CurrentRound { get; private set; } = new RoundContext();

		/// <summary>How many of this wave's Monsters haven't been spawned yet - what RoundManager waits on before it's even possible for the wave to be "cleared."</summary>
		public int MonstersRemainingToSpawn => pending.Sum(p => p.Remaining);

		/// <summary>How many currently-spawned Monsters from this wave are still alive - decremented via the same EntityEventBus everything else listens to, not a per-monster callback this class has to wire by hand.</summary>
		public int MonstersAlive { get; private set; }

		void OnMonsterLeftTheField(TriggerContext ctx)
		{
			// OnDeath (killed) and OnReached (walked to the Destination - see MonsterController.
			// ReachedDestination) are both "no longer active on the field," so both decrement - this one
			// handler is subscribed to both TriggerTypes above rather than checking ctx.TriggerType itself.
			if (ctx.Entity is Monster) MonstersAlive--;
		}

		/// <summary>
		/// Drills into WaveUtil.ENEMIETRIX[Biome][Stage][Round][Wave] using WorldProgress's own indices,
		/// bounds-checking at every level rather than assuming exactly 20 entries always exist (nothing's
		/// authored yet - see ENEMIETRIX's own doc comment) - returns an empty wave instead of throwing
		/// if progression has gone further than the authored content covers.
		/// </summary>
		public List<SpawnChain> LoadFromMatrix(WorldProgress world)
		{
			if (!WaveUtil.ENEMIETRIX.TryGetValue(world.Biome, out var stages)) return new List<SpawnChain>();
			if (world.Stage < 0 || world.Stage >= stages.Count) return new List<SpawnChain>();

			List<WaveRound> rounds = stages[world.Stage].Rounds;
			if (world.Round < 0 || world.Round >= rounds.Count) return new List<SpawnChain>();

			List<Wave> waves = rounds[world.Round].Waves;
			if (world.Wave < 0 || world.Wave >= waves.Count) return new List<SpawnChain>();

			return waves[world.Wave].Chains;
		}

		public List<SpawnChain> LoadFromFile(List<WaveData> waveDataAssets)
		{
			List<SpawnChain> chains = new();
			foreach (WaveData data in waveDataAssets) chains.Add(WaveUtil.LoadWaveData(data));
			return chains;
		}

		class PendingSpawn
		{
			public SpawnChain Chain;
			public WorldProgress World;
			public float TimeUntilNext;
			public int Remaining;
		}
		readonly List<PendingSpawn> pending = new();

		/// <summary>
		/// Schedules every SpawnChain in the wave: Chain.quantity spawns of Chain.monster, the first
		/// Chain.offset seconds from now, Chain.period seconds apart after that. Actual spawning happens
		/// in Update() via EntityManager.SpawnMonster(plan, world), so each one is scaled to the current
		/// WorldProgress on the way in - see that overload.
		/// </summary>
		public void SpawnWave(List<SpawnChain> wave, WorldProgress world)
		{
			CurrentRound.SpawnChains = wave;
			CurrentRound.MonsterCount = 0;
			foreach (SpawnChain chain in wave)
			{
				CurrentRound.MonsterCount += chain.quantity;
				pending.Add(new PendingSpawn { Chain = chain, World = world, TimeUntilNext = chain.offset, Remaining = chain.quantity });
			}
		}

		void Update()
		{
			for (int i = pending.Count - 1; i >= 0; i--)
			{
				PendingSpawn spawn = pending[i];
				spawn.TimeUntilNext -= Time.deltaTime;
				if (spawn.TimeUntilNext > 0) continue;

				Monster monster = EntityManager.SpawnMonster(spawn.Chain.monster, spawn.World);
				EntityManager.ApplyEnhancements(monster); // Foundry etc. - see EntityManager's Enhancement Systems region
				MonstersAlive++;
				if (spawn.Chain.tags != null)
				{
					foreach (Tag tag in spawn.Chain.tags) monster.AddTag(tag);
				}

				spawn.Remaining--;
				spawn.TimeUntilNext = spawn.Chain.period;

				if (spawn.Remaining <= 0) pending.RemoveAt(i);
			}
		}
	}
}
