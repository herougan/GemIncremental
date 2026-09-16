using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Stages;
using UnityEngine;

namespace Util.Game
{
	public static class WaveUtil
	{
		#region Spawning
		/// <summary>
		/// ENEMIETRIX[Biome][Stage][Round][Wave] = the SpawnChains for that exact wave - Biome/Stage/
		/// Round/Wave is the full progression hierarchy (see WorldProgress): 20 Waves make a Round, 20
		/// Rounds make a Stage, 20 Stages make a Biome (a fixed size per level, not enforced by the type
		/// itself - EntityWaveManager.LoadFromMatrix bounds-checks against however many are actually
		/// authored here rather than assuming exactly 20 everywhere).
		/// STUB - empty for every Biome; no wave content authored yet.
		/// </summary>
		public static readonly Dictionary<StageType, List<BiomeStage>> ENEMIETRIX = new Dictionary<StageType, List<BiomeStage>>()
		{
		};

		public static SpawnChain LoadWaveData(WaveData data)
		{
			return new SpawnChain
			{
				monster = data.monster,
				period = data.period,
				quantity = data.quantity,
				offset = data.offset,
				tags = data.tags,
			};
		}

		#endregion Spawning

		#region Mapping
		public static readonly Dictionary<MonsterType, Monster.Race> TYPE_TO_RACE = new Dictionary<MonsterType, Monster.Race>()
		{
			[MonsterType.Muddy] = Monster.Race.Slime,
			[MonsterType.Oily] = Monster.Race.Slime,
			[MonsterType.Plasma] = Monster.Race.Slime,
			[MonsterType.Riverling] = Monster.Race.Slime,
			[MonsterType.Rotto] = Monster.Race.Slime,
			[MonsterType.Slimey] = Monster.Race.Slime,
			[MonsterType.Sweetie] = Monster.Race.Slime,
			[MonsterType.Splashy] = Monster.Race.Slime,
			[MonsterType.Sticky] = Monster.Race.Slime,
			[MonsterType.Toxa] = Monster.Race.Slime,
			//
			[MonsterType.Bat] = Monster.Race.Beast,
			[MonsterType.DireWolf] = Monster.Race.Beast,
			[MonsterType.Felid] = Monster.Race.Beast,
			[MonsterType.Werewolf] = Monster.Race.Beast,
			//
			[MonsterType.Incarnation] = Monster.Race.Spirit,
			[MonsterType.Ghost] = Monster.Race.Spirit,
			[MonsterType.Avatar] = Monster.Race.Spirit,
			//
			[MonsterType.RobotWalker] = Monster.Race.Robot,
			//
			[MonsterType.Skeleton] = Monster.Race.Undead,
			[MonsterType.Pumpkin] = Monster.Race.Undead,
			[MonsterType.Ghoul] = Monster.Race.Undead,
			// Milkbool,
			// // Techno
			// Electromite,
			// Mollusc
			[MonsterType.Octopus] = Monster.Race.Mollusc,
			//
		};

		#endregion Mapping
	}

	[Serializable]
	public class SpawnChain
	{
		[Header("Monster")]
		public MonsterPlan monster;

		[Header("Spawn Settings")]
		public float period;
		public int quantity;
		public float offset;
		public List<Tag> tags;
	}

	// ===== Biome > Stage > Round > Wave hierarchy - see ENEMIETRIX's own doc comment =====

	/// <summary>A single wave - what actually gets passed to EntityWaveManager.SpawnWave.</summary>
	[Serializable]
	public class Wave
	{
		public List<SpawnChain> Chains = new();
	}

	/// <summary>A round: 20 waves, per the design ("20 waves makes up a round").</summary>
	[Serializable]
	public class WaveRound
	{
		public List<Wave> Waves = new();
	}

	/// <summary>A stage within a Biome: 20 rounds ("20 rounds makes up a stage").</summary>
	[Serializable]
	public class BiomeStage
	{
		public List<WaveRound> Rounds = new();
	}
}