using System;
using System.Collections.Generic;
using TowerDefence.Entity.Items;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using UnityEngine;
using Util.Maths;
using Util.Serialisation;

namespace TowerDefence.Entity.Tower
{
	[System.Serializable]
	public class Tower : Entity
	{
		#region Preamble

		[SerializeField]
		// Config and Mutators
		public TowerPlan Plan;

		// ===== Lineage =====
		public List<Tower> Children { get; private set; }// Set on fusion
		public DateTime DateCreated { get; private set; }


		#endregion Preamble

		#region Stats
		//
		public double cooldown;
		public double cost;

		//
		public float turnSpeed;
		public float arcOfFire;
		public float reeling;

		// Cooldown Cost Decrease


		#endregion Stats

		#region Game State


		#endregion Game State

		#region Cumulative State

		public int kills = 0;
		public int time = 0;
		public int mvp_attacker = 0;
		public int mvp_status = 0; // Meta Stats TODO

		#endregion Cumulative State

		#region Meta
		public DateTime datePurchased;
		public DateTime lastDatePurchased;
		public DateTime dateFused;
		public DateTime dateMetamorphised;

		#endregion Meta

		#region Enum
		// ===== Enum =====
		public enum Targetting
		{
			health,
			percentageHealth,
			debuffed,
			distance,
			proximity,
		}
		public enum BulletCurve
		{
			linear,
			slerp,
			ease,
		}
		public enum Type
		{
			None,
			Amethyst,
			Emerald,
			Diamond,
			Ruby,
			Opal,
			Sapphire,
			Peridot,
			Topaz,
			Lapis,
			Alexandrite,
			Agate,
			Spinel,
			Beryl,
			Morganite,
			Onyx,
			Garnet,
			Citrine,
			Aquamarine,
			Mozanite,
			Moonstone,
			Tanzanite,
			Jasper,
			Jade,
			Tourmaline,
			Pyrite,
			Oricahclum,
			Socerorium,
			Philosophine,
			Seraphium,
			DarkCrystallum,
			Glass,
			Urite,
			SeaGlass,
			Amberina,
		}
		public enum Colour
		{


		}
		public enum Rank
		{

		}
		public enum Tag
		{
			Turret,
			Trap,

		}

		#endregion Enum
	}
}