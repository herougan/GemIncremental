
using System;
using System.Collections.Generic;
using Util.Debug;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Effects.Types.Stat;
using TowerDefence.Entity.Tower;
using TowerDefence.Stats;
using UnityEngine;
using Util.Maths;
using Random = UnityEngine.Random;
using TowerDefence.Entity.Behaviour;

namespace TowerDefence.Library
{
	/// <summary>
	/// This static class provides functionality for handling buffs, debuffs, 
	/// and skill-related logic in the game. This includes applying, stacking, 
	/// and scaling buffs, as well as handling skill triggers, conditions, and effects.
	/// </summary>
	public static class SkillsLib
	{
		#region Data

		public static Dictionary<StatusType, BuffPlan> StatusPlans = new Dictionary<StatusType, BuffPlan>()
			{
				{ StatusType.Stun, Resources.Load<BuffPlan>("Status/Stun") },
				{ StatusType.Poison, Resources.Load<BuffPlan>("Status/Poison") },
				{ StatusType.Freeze, Resources.Load<BuffPlan>("Status/Freeze") },
				{ StatusType.Slow, Resources.Load<BuffPlan>("Status/Slow") },
				{ StatusType.Blind, Resources.Load<BuffPlan>("Status/Blind") },
				{ StatusType.Knockback, Resources.Load<BuffPlan>("Status/Knockback") },
				{ StatusType.Burn, Resources.Load<BuffPlan>("Status/Burn") },
				{ StatusType.Paralyse, Resources.Load<BuffPlan>("Status/Paralyse") },
				{ StatusType.Confuse, Resources.Load<BuffPlan>("Status/Confuse") },
				{ StatusType.Silence, Resources.Load<BuffPlan>("Status/Silence") },
				{ StatusType.Weak, Resources.Load<BuffPlan>("Status/Weak") },
				{ StatusType.Charm, Resources.Load<BuffPlan>("Status/Charm") },
				{ StatusType.Curse, Resources.Load<BuffPlan>("Status/Curse") },
			};

		public static Dictionary<StatusType, BuffStackType> StatusStackTypes = new Dictionary<StatusType, BuffStackType>()
			{
				{ StatusType.Stun, DelibilitatingTypeStacking},
				{ StatusType.Poison, PoisonTypeStacking},
				{ StatusType.Freeze, FireTypeStacking },
				{ StatusType.Slow, FireTypeStacking},
				{ StatusType.Blind, DelibilitatingTypeStacking},
				{ StatusType.Knockback, FireTypeStacking},
				{ StatusType.Burn, FireTypeStacking},
				{ StatusType.Paralyse, DelibilitatingTypeStacking},
				{ StatusType.Confuse, DelibilitatingTypeStacking},
				{ StatusType.Silence, DelibilitatingTypeStacking},
				{ StatusType.Weak, DelibilitatingTypeStacking},
				{ StatusType.Charm, DelibilitatingTypeStacking},
				{ StatusType.Curse, DelibilitatingTypeStacking},
			};

		// reference BuffStackTypes
		private static BuffStackType FireTypeStacking = new BuffStackType(MathOperation.Max, MathOperation.Max, MathOperation.Add);
		private static BuffStackType PoisonTypeStacking = new BuffStackType(MathOperation.Add, MathOperation.Max, MathOperation.Max);
		private static BuffStackType DelibilitatingTypeStacking = new BuffStackType(MathOperation.Max, MathOperation.Max, MathOperation.Max);

		#endregion Data

		#region Apply Effect
		public static readonly Dictionary<ActionType, IActionHandler> ActionHandlers = new Dictionary<ActionType, IActionHandler>()
			{
				{ ActionType.Stat, new StatActionHandler() },
			};
		public static void ApplyEffect(IEffect effect, IEntity source, TriggerContext triggerContext)
		{
			foreach (IAction action in effect.Actions)
			{
				ApplyEffect(action, source, triggerContext);
			}
		}

		public static void ApplyEffect(IAction action, IEntity source, TriggerContext triggerContext)
		{
			if (ActionHandlers.TryGetValue(action.ActionType, out IActionHandler handler))
			{
				// handler.ApplyAction(in GameManager.Instance.GameContext, in triggerContext, action);
			}
			else
			{
				LogManager.Instance.LogError($"No action handler found for action type: {action.ActionType}");
			}
		}

		#endregion Apply Effect

		#region Scale 

		public static void Recalculate(GameContext context, ISkill skill)
		{
			// TriggerContext triggerContext = context.TriggerContext;
		}

		#endregion Scale

		#region IBuff/Debuff Application
		public static readonly Dictionary<StatusType, double> StatusBuffValue = new Dictionary<StatusType, double>()
		{

		};
		public static readonly Dictionary<StatusType, double> StatusBuffScale = new Dictionary<StatusType, double>()
		{

		};
		public static readonly Dictionary<StatType, double> StatBuffValue = new Dictionary<StatType, double>()
		{

		};
		public static readonly Dictionary<StatType, double> StatBuffScale = new Dictionary<StatType, double>()
		{

		};

		// Stacking
		public static void StackBuff(ref IEntity entity, IBuff buff, IBuff duff)
		{

		}

		public static void StackRanks(ref IEntity entity, IBuff buff, int rank)
		{

		}

		public static void ApplyBuff(IEntity entity, IBuff buff)
		{
			entity.ApplyBuff(buff);
		}

		public static void ApplyStatus(IEntity entity, StatusType statusType, ddouble value = default(ddouble))
		{
			throw new NotImplementedException();
		}

		public static void RecalculateBuff(ref IEntity entity, IBuff buff)
		{
			throw new NotImplementedException();
		}

		public static void RecalculateStatus(ref IEntity entity, StatusType statusType)
		{
			throw new NotImplementedException();
		}

		// public static double ScaleBuff(IBuff buff, int rank)
		// {
		// 	if (buff is StatBuff)
		// 	{
		// 		if (StatBuffScale.ContainsKey((buff as StatBuff).Type) == false) return 1; // Default
		// 		return Math.Pow(StatBuffScale[(buff as StatBuff).Type], rank);
		// 	}
		// 	if (buff is StatusBuff)
		// 	{
		// 		if (StatusBuffScale.ContainsKey((buff as StatusBuff).Type) == false) return 1; // Default
		// 		return Math.Pow(StatusBuffScale[(buff as StatusBuff).Type], rank);
		// 	}
		// 	return 1; // Default
		// }

		public static void UnapplyBuff(ref Tower tower, IBuff buff)
		{

		}

		// Per second (/tick checks)
		public static void AffectMonster(Monster monster, IBuff buff)
		{

		}

		// On Apply
		public static bool CheckCondition(IEffect effect, IEntity entity)
		{
			if (effect.Conditions == null || effect.Conditions.Count == 0) return true; // No conditions, always true
			foreach (ICondition cond in effect.Conditions)
			{
				if (!CheckCondition(cond, entity)) return false; // If any condition fails, return false
			}
			return true; // All conditions passed
		}

		public static bool CheckCondition(ICondition cond, IEntity entity)
		{
			switch (cond)
			{
				case StatCondition statCond:
					return MathsLib.Compare(GetStatValue(entity, statCond), statCond.Value, statCond.Comparative);
				case KinematicsCondition kinCond:
					return MathsLib.Compare(entity.GetKinematics(kinCond.Param), kinCond.Value, kinCond.Comparative);
				case MileageCondition mileageCond:
					return MathsLib.Compare(entity.GetMileage(mileageCond.MileageType).Value, mileageCond.Mileage, mileageCond.Comparative);
				case ResourceCondition resourceCond:
					return MathsLib.Compare(entity.GetResource(resourceCond.ResourceType).Value, resourceCond.RequiredAmount, resourceCond.Comparative);
				case TokenCondition tokenCond:
					return MathsLib.Compare(entity.GetToken(tokenCond.TokenType).Number, tokenCond.RequiredCount, tokenCond.Comparative);
				// case EntityInventoryCondition entityInventoryCond:
				// 	if (entityInventoryCond.RequireNoItems && entity.GetInventory(entityInventoryCond.ItemType) > 0)
				// 		return false; // Entity should not have the item
				// 	if (entityInventoryCond.RequireHoldingItems && entity.GetInventory(entityInventoryCond.ItemType) == 0)
				// 		return false; // Entity should have the item
				// 	if (entityInventoryCond.RequiredCount < 0)
				// 		return true; // No specific count required, just check if the item exists
				// 	return MathsLib.Compare(entity.GetInventory(entityInventoryCond.ItemType), entityInventoryCond.RequiredCount, entityInventoryCond.Comparative);
				case EntityBehaviourCondition entityBehaviourCond:
					return CheckBehaviour(entityBehaviourCond.BehaviourType, entity);
				case StatusCondition statusCond:
					return entity.ContainStatus(statusCond.StatusType);
				case TagCondition tagCond:
					return entity.Tags.Contains(tagCond.Tag); // Check if the entity has the specified tag
				case MetaCondition metaCond:
					return entity.CheckMeta(metaCond.MetaType, metaCond.Text); // Check if the entity matches the meta condition
				default:
					throw new ArgumentException($"Unknown condition type: {cond.GetType()}");
			}
		}

		public static bool CheckBehaviour(EntityBehaviourType behaviourType, IEntity entity)
		{
			return behaviourType == entity.Behaviour.Type;
		}

		#endregion IBuff/Debuff Application

		#region Entity Util

		public static ddouble GetStatValue(IEntity entity, ICondition condition)
		{
			switch (condition)
			{
				case StatCondition statCond:
					return entity.GetStat(statCond.StatType).Value;
				case KinematicsCondition kinCond:
					return GetKinematics(entity, kinCond);
				case MileageCondition mileageCond:
					return entity.GetMileage(mileageCond.MileageType).Value;
				case ResourceCondition resourceCond:
					return entity.GetResource(resourceCond.ResourceType).Value;
				case TokenCondition tokenCond:
					return entity.GetToken(tokenCond.TokenType).Number;
				// case EntityInventoryCondition entityInventoryCond:
				// 	return entity.GetInventory(entityInventoryCond.ItemType);
				default:
					throw new ArgumentException($"Unknown condition type: {condition.GetType()}");
			}
		}

		public static float GetKinematics(IEntity entity, KinematicsCondition condition)
		{
			return entity.GetKinematics(condition.Param);
		}

		#endregion Entity Util

		#region Attack Calculation

		public static double CalculateCritModifier(double critChance, double critDamage)
		{
			if (critChance > 1) critChance = 1;
			if (critDamage < 1) critDamage = 1;
			return (1 - critChance) + (critChance * critDamage);
		}

		public static int CalculateCritCount(double critChance)
		{
			int crit = 0;
			while (Random.Range(0, 1) < critChance) ++crit;
			return crit;
		}


		#endregion Attack Calculation
	}

}