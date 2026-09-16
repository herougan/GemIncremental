using System;
using System.Collections.Generic;
using TowerDefence.Entity.Skills;
using TowerDefence.Stats;
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

		public Tower(TowerPlan plan) : base(plan)
		{
			_plan = plan;
		}

		// ===== Lineage =====
		public List<Tower> Children { get; private set; }// Set on fusion
		public DateTime DateCreated { get; private set; }


		#endregion Preamble

		#region Stats

		// Tower-specific Stats
		// cooldown/turnSpeed/arcOfFire/reeling used to live here as raw fields - dead pre-StatMod cruft,
		// removed: Attack/Range/AttackSpeed/TurnSpeed/ArcOfFire are all StatType entries now (see
		// StatType.cs), read identically off any Entity via GetStat - the shared inheritance point is
		// Entity itself, not a Tower-specific field, so Monster gets the same mechanism for free. `cost`
		// is genuinely Tower-only (not a combat stat) but isn't read anywhere yet either - left as a
		// placeholder for whenever the Mulligan/shop layer needs a price.
		public double cost;

		#endregion Stats

		#region Upgrades

		// Upgrade Centre state - see Util.Game.TowerUpgradeUtil, which is the only thing that should
		// mutate either of these (never GetStat/RegisterStatMod or Skills.Add directly - that would
		// desync the purchase count from what's actually been bought/registered).
		//
		// How many times each StatType has been purchased on THIS live Tower instance - drives per-
		// purchase cost scaling (see TowerUpgradeUtil.GetStatCost). Runtime-only, not part of TowerPlan -
		// every Tower spawned from the same Plan starts back at 0 purchases, same as StatBlock itself
		// starting fresh from the Plan's sparse StatEntries.
		public readonly Dictionary<StatType, int> UpgradePurchases = new();

		// Which of TowerPlan.SkillUnlocks have actually been bought (as opposed to merely eligible -
		// see TowerUpgradeUtil.GetEligibleSkillUnlocks) - a purchased unlock's Skill also lives in the
		// normal Skills list once registered, but that alone can't distinguish "granted via InitSkills"
		// from "bought via the Upgrade Centre," which TowerUpgradeUtil needs to know to avoid a double-buy.
		public readonly HashSet<SkillPlan> PurchasedSkillUnlocks = new();

		#endregion Upgrades

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