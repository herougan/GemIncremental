using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using UnityEditor;
using Util.Maths;
using Random = UnityEngine.Random;

namespace TowerDefence.Entity.Monster
{
	[Serializable]
	public class Monster : Entity
	{
		#region Information

		MonsterPlan _plan;
		public new IEntityPlan Plan => _plan;

		#endregion Information

		#region Game
		#endregion Game

		#region Event
		#endregion Event

		#region Stats

		#endregion Stats

		#region Behaviour
		#endregion Behaviour
		#region Util

		#endregion Util

		#region Calculation

		#endregion Calculation

		#region Enum

		public enum Race
		{
			Slime,
			Beast,
			Undead,
			Elf,
			Ork,
			Dwarf,
			Human,
			Kobold,
			Gnome,
			Spirit,
			Robot,
			Mollusc,
		}
		public enum Type
		{
			None,
			Bloodo,
			Bubbling,
			Dewy,
			Drippie,
			Ferro,
			Gooey,
			Grimeo,
			Melta,
			Muddy,
			Oily,
			Plasma,
			Riverling,
			Rotto,
			Slimey,
			Splashy,
			Sticky,
			Sweetie,
			Toxa,
			// Beast
			Bat,
			DireWolf,
			Felid,
			Werewolf,
			// Spirit
			Incarnation,
			Ghost,
			Avatar,
			// Robot
			RobotWalker,
			// None,
			// // Forest(s)
			// Slime,
			// Bat,
			// // Cave(s)
			// Undead,
			Skeleton,
			Pumpkin,
			Ghoul,
			// Milkbool,
			// // Techno
			// Electromite,
			// Mollusc
			Octopus,
		}
		public enum Category
		{
			/* Basic */
			Common,
			Uncommon,

			/* Rare */
			Rare,
			SuperRare,
			Epic,

			/* Individual */
			Heroic,
			Legendary,
			Mythic,
		}

		#endregion Enum
	}
}