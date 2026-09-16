using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Tower;
using TowerDefence.Stats;
using Util.Maths;
// Aliased, not a plain `using Player;` - Player is a top-level namespace containing a class of the same
// name, so an unqualified `Player` here always resolves to the namespace (CS0118) - same issue
// GameManager.cs's own PlayerModel alias exists for.
using PlayerModel = Player.Player;

namespace Util.Game
{
	/// <summary>
	/// Upgrade Centre calculation - "spend Gold on individual stats" (per-purchase cost scaling, a
	/// permanent StatMod each time) plus "unlock skills at thresholds - bought again separately" (see
	/// TowerPlan.SkillUnlock). Player.Gold is passed in as a parameter, not reached via a singleton -
	/// same "pure calculation given what's handed to it" spirit as EntityUtil, just Tower/Upgrade-shaped.
	/// Ranking a Tower up a Tier (TowerPlan.NextRank) is a deliberately separate mechanic, not built here -
	/// see the design discussion; nothing in this file touches rank/NextRank.
	/// </summary>
	public static class TowerUpgradeUtil
	{
		/// <summary>STUB tuning table - not balanced. (baseCost, costPerPurchase, valuePerPurchase) per upgradable StatType; a StatType missing here just isn't upgradable through the Upgrade Centre.</summary>
		public static readonly Dictionary<StatType, (ddouble baseCost, ddouble costPerPurchase, ddouble valuePerPurchase)> UpgradableStats = new()
		{
			[StatType.Attack] = (10, 5, 1),
			[StatType.Range] = (15, 8, 0.5),
			[StatType.AttackSpeed] = (20, 10, 0.1),
		};

		public static bool IsUpgradable(StatType stat) => UpgradableStats.ContainsKey(stat);

		/// <summary>Linear cost scaling - baseCost + costPerPurchase * (times already bought). 0 if `stat` isn't in UpgradableStats.</summary>
		public static ddouble GetStatCost(Tower tower, StatType stat)
		{
			if (!UpgradableStats.TryGetValue(stat, out var config)) return default(ddouble);
			int purchases = tower.UpgradePurchases.TryGetValue(stat, out int n) ? n : 0;
			return config.baseCost + config.costPerPurchase * (double)purchases;
		}

		/// <summary>Deducts Gold and registers a permanent +valuePerPurchase StatMod on `stat` - false (no charge, no mod) if `stat` isn't upgradable or the Player can't afford GetStatCost.</summary>
		public static bool PurchaseStat(Tower tower, StatType stat, PlayerModel player)
		{
			if (!UpgradableStats.TryGetValue(stat, out var config)) return false;
			if (!player.Gold.Subtract(GetStatCost(tower, stat))) return false;

			tower.RegisterStatMod(new StatMod(config.valuePerPurchase, stat, MathOperation.Add, new UpgradeSource(tower)));
			tower.UpgradePurchases[stat] = (tower.UpgradePurchases.TryGetValue(stat, out int n) ? n : 0) + 1;
			return true;
		}

		/// <summary>Sum of every stat's purchase count on this Tower - what TowerPlan.SkillUnlock.Threshold is measured against (any purchase counts toward it, not a specific stat).</summary>
		public static int GetTotalPurchases(Tower tower)
		{
			int total = 0;
			foreach (int purchases in tower.UpgradePurchases.Values) total += purchases;
			return total;
		}

		/// <summary>Every SkillUnlock this Tower has hit the purchase-threshold for but hasn't bought yet - what the Upgrade Centre UI should offer as purchasable (not already-owned, not still locked).</summary>
		public static List<TowerPlan.SkillUnlock> GetEligibleSkillUnlocks(Tower tower)
		{
			List<TowerPlan.SkillUnlock> eligible = new();
			if (tower.Plan is not TowerPlan plan) return eligible;

			int total = GetTotalPurchases(tower);
			foreach (TowerPlan.SkillUnlock unlock in plan.SkillUnlocks)
			{
				if (total < unlock.Threshold) continue;
				if (tower.PurchasedSkillUnlocks.Contains(unlock.Skill)) continue;
				eligible.Add(unlock);
			}
			return eligible;
		}

		/// <summary>Deducts Gold and actually grants the Skill (Entity.RegisterSkill, same path any other Skill goes live through) - false (no charge) if already purchased or the Player can't afford unlock.Cost. Doesn't check the threshold itself - GetEligibleSkillUnlocks already gates what should be offered; this trusts its caller.</summary>
		public static bool PurchaseSkill(Tower tower, TowerPlan.SkillUnlock unlock, PlayerModel player)
		{
			if (tower.PurchasedSkillUnlocks.Contains(unlock.Skill)) return false;
			if (!player.Gold.Subtract(unlock.Cost)) return false;

			Skill skill = new Skill(unlock.Skill, tower);
			tower.Skills.Add(skill);
			tower.RegisterSkill(skill);
			tower.PurchasedSkillUnlocks.Add(unlock.Skill);
			return true;
		}

		/// <summary>Attribution for Upgrade-Centre-bought StatMods ("who granted this permanent mod") - same spirit as WorldProgress/Player's own dummy Entity Casters, just pointing at the Tower itself rather than a separate dummy, since an upgrade is intrinsic to that one Tower, not granted by an external caster.</summary>
		class UpgradeSource : ISource
		{
			public IEntity Caster { get; }
			public UpgradeSource(IEntity caster) => Caster = caster;
		}
	}
}
