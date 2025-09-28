using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Entity.Tower
{
	[System.Serializable]
	public class Tower : Entity
	{
		#region Preamble

		[SerializeField]
		// Config and Mutators
		public TowerPlan _plan;
		public new IEntityPlan Plan => _plan;

		// ===== Lineage =====
		public List<Tower> Children { get; private set; }// Set on fusion
		public DateTime DateCreated { get; private set; }


		#endregion Preamble

		#region Stats

		// Tower-specific Stats
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
		#endregion Enum
	}

	public enum TowerType
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

}