using System.Collections.Generic;
using System.Linq;
using Util.Debug;
using JetBrains.Annotations;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Effects.Types.Attack;
using TowerDefence.Stats;
using Util.Maths;

namespace Util.Game
{
	/// <summary>
	/// Pure calculation given entity/plan/stat data - no game-knowledge (GameManager/WorldProgress/
	/// EntityManager singletons) reaches in here, only what's passed as parameters. Straight,
	/// entity-agnostic math (ddouble ops, comparisons, PrettyPrint) lives in MathsLib instead - this is
	/// specifically the entity/skill-shaped calculations layered on top of that.
	/// </summary>
	public static class EntityUtil
	{
		public static bool Check(IEntity entity, IEnumerable<ICondition> conditions)
		{
			foreach (ICondition condition in conditions)
			{
				if (!Check(entity, condition))
					return false;
			}
			return true;
		}

		public static bool Check(IEntity Entity, ICondition condition)
		{
			switch (condition.ConditionType)
			{
				case ConditionType.Stat:
					// Load Condition
					var statCondition = condition as StatCondition;
					if (statCondition == null)
					{
						LogManager.Instance.LogWarning($"StatCondition {condition} is null or failed to cast in EntityUtil.Check");
						return false;
					}

					// Fetch value
					ddouble value = statCondition.CheckCurrent
						? Entity.StatBlock.GetCurrent(statCondition.StatType)
						: Entity.StatBlock.GetStat(statCondition.StatType);

					// Compare
					return MathsLib.Compare(value, statCondition.Value, statCondition.Comparative);

				case ConditionType.Kinematics:
					// Load Condition
					var kinCondition = condition as KinematicsCondition;
					if (kinCondition == null) return false;

					// Fetch value
					var kinValue = Entity.Kinematics.Get(kinCondition.Param);

					// Compare
					return MathsLib.Compare(kinValue, kinCondition.Value, kinCondition.Comparative);

				case ConditionType.Tag:
					var tagCondition = condition as TagCondition;
					return tagCondition != null && Entity.HasTag(tagCondition.Tag);

				case ConditionType.Counter:
					var counterCondition = condition as CounterCondition;
					if (counterCondition == null) return false;
					return MathsLib.Compare(Entity.GetCounter(counterCondition.CounterType), counterCondition.RequiredCount, counterCondition.Comparative);

				// Everything else (Mileage/Resource/Token/Status/EntityInventory/Behaviour/Meta/Race) is
				// still a data-only stub - its fields were never wired to real Entity state.
				// Permissive rather than failing closed, so an Effect carrying one of those doesn't
				// silently stop firing entirely just because the condition type isn't implemented yet.
				// Implement properly here as each is fleshed out - see Stat/Kinematics/Tag above for the
				// pattern to follow.
				default:
					return true;
			}
		}

		/// <summary>
		/// Applies every mod to baseValue, Operation-bucketed rather than insertion-ordered: every Add
		/// mod first, then every Multiply, then every Exponent - so the result only depends on which
		/// bucket each mod is in, never on what order they were registered/deregistered in (which
		/// matters, since sources register/deregister independently of each other - see Entity's
		/// StatType -> List&lt;IStatMod&gt; registry). Any other MathOperation is skipped - Geq/Leq/Abs*/etc.
		/// don't mean anything as a stat modifier, only as a comparison (see MathsLib.Compare).
		/// </summary>
		public static ddouble ApplyStatMods(ddouble baseValue, IEnumerable<IStatMod> mods)
		{
			ddouble value = baseValue;
			List<IStatMod> modList = mods.ToList();

			foreach (IStatMod mod in modList.Where(m => m.Operation == MathOperation.Add)) value += mod.Value;
			foreach (IStatMod mod in modList.Where(m => m.Operation == MathOperation.Multiply)) value *= mod.Value;
			foreach (IStatMod mod in modList.Where(m => m.Operation == MathOperation.Exponent)) value = value ^ mod.Value;

			return value;
		}

		/// <summary>
		/// Diminishing-returns defence mitigation: incoming * K/(K + defence) - defence=0 is no reduction,
		/// higher defence asymptotically approaches (never reaches) 100% reduction. K is a placeholder
		/// tuning constant (not a balanced number - STUB, revisit once real stat budgets exist), kept as a
		/// parameter rather than hardcoded so a future per-damage-type or per-difficulty K isn't a rewrite.
		/// Which stat (Defence vs MagicResist) to pass in is the caller's call - see Entity.ApplyDamage,
		/// which picks by Tag.Magic.
		/// </summary>
		public static ddouble Mitigate(ddouble incoming, ddouble defence, ddouble mitigationConstant)
		{
			if ((double)incoming <= 0 || (double)defence <= 0) return incoming;
			return incoming * (mitigationConstant / (mitigationConstant + defence));
		}

		/// <summary>
		/// Rough DPS estimate for a SkillPlan - a first approximation, not the real number a player would
		/// see per hit: no crit chance, no multi-target, none of DamageCalculator's modifier tree (Player/
		/// Compendium/Game multipliers) factored in. Good enough to compare skills' relative power or
		/// drive a "DPS" UI number later; not good enough to be the actual combat-log math.
		/// </summary>
		public static ddouble GetOverallDPS(SkillPlan plan)
		{
			ddouble total = 0;
			foreach (IEffect effect in plan.Effects)
			{
				float timing = GetTiming(effect, plan);
				if (timing <= 0) continue;

				total += GetEffectDamage(effect) / timing;
			}
			return total;
		}

		/// <summary>Seconds between activations - an OnPeriodic Trigger's own Parameter if this Effect has one, otherwise the Plan's Cooldown (an OnAttack/OnHit-triggered Effect fires roughly once per attack).</summary>
		static float GetTiming(IEffect effect, SkillPlan plan)
		{
			Trigger periodic = effect.Triggers.OfType<Trigger>().FirstOrDefault(t => t.Type == TriggerType.OnPeriodic);
			return periodic != null ? periodic.Parameter : plan.Cooldown;
		}

		static ddouble GetEffectDamage(IEffect effect)
		{
			ddouble damage = 0;
			foreach (IAction action in effect.Actions)
			{
				if (action.ActionType == ActionType.Damage || action.ActionType == ActionType.Reflect)
				{
					damage += action.Value;
				}
				else if (action is ProjectileAction projectile && projectile.Plan != null)
				{
					damage += projectile.Plan.Damage;
				}
			}
			return damage;
		}
	}
}