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
		/// Concisely stores information on what enemies will be fought.
		/// MonsterMatrix = Enemietrix[Biome][Stage]
		/// 	// Recall, World, Biome, Stage, Round, Wave
		/// 	Consists of a List of Chain Series - Randomly select one to be the round's chain series
		/// 		A Chain Series is a List of Chain(s) - The n-th chain describes when and what will be spawned
		/// 		It represents a different challenge each time.
		/// 		Chain(Monster to be summoned, Quantity, Time offset (s), Time)
		/// 		* Note * Round 0 always spawns ChainSeries 0, which is the easiest chain. The rest are not ranked in difficulty.
		/// Stronger chains are usually higher in the "n" 
		/// </summary>
		public static readonly Dictionary<StageType, List<SpawnChain>> ENEMIETRIX = new Dictionary<StageType, List<SpawnChain>>()
		{
			// Need to be a Monobehaviour

		};

		public static SpawnChain LoadWaveData(WaveData data)
		{
			return new SpawnChain();
			// {
			// 	monster = data.monster,
			// 	period = data.period,
			// 	quantity = data.quantity,
			// 	offset = data.offset,
			// 	tags = data.tags
			// };
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
}