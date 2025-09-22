
using System;
using System.Collections.Generic;
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

namespace TowerDefence.Library
{
	/// <summary>
	/// This static class provides functionality for handling buffs, debuffs, 
	/// and skill-related logic in the game. This includes applying, stacking, 
	/// and scaling buffs, as well as handling skill triggers, conditions, and effects.
	/// </summary>
	public static class SkillsLib
	{
		#region Apply Effect
		public static readonly Dictionary<ActionType, IEffectHandler> EffectHandlers = new Dictionary<ActionType, IEffectHandler>()
		{
			{ ActionType.Stat, new StatEffectHandler() },
		};
		public static void ApplyEffect(IEffect effect, IEntity source, IEntity target)
		{
			// foreach (Skills.Action action in effect.Actions)
			// {

			// }


			// if (EffectHandlers.TryGetValue(effect.Type, out IEffectHandler handler))
			// {
			// 	handler.ApplyEffect(new GameContext(source, target), effect);
			// }
			// else
			// {
			// 	Debug.LogWarning($"No effect handler found for action type: {effect.Type}");
			// }
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
			entity.AddBuff(buff); // TODO - should only be an attempt - trigger OnBuffApplied?

			// Check if buff already exists
			foreach (IBuff _buff in entity.Buffs) // ????
			{
				// Stack Buff
				if (_buff.BuffType == BuffType.StatBuff)
				{
					StackBuff(ref entity, _buff, buff);
				}
				if (_buff.BuffType == BuffType.StatusBuff)
				{
					StackBuff(ref entity, _buff, buff);
				}
				if (_buff.BuffType == BuffType.Aura)
				{
					StackBuff(ref entity, _buff, buff);
				}
				if (_buff.BuffType == BuffType.Saga)
				{
					StackBuff(ref entity, _buff, buff);
				}
			}

			// else, Apply Buff
			if (buff is StatBuff)
			{
				entity.AddBuff(buff);
				// 	switch (buff as StatBuff)
				// 	{
				// 		case StatType.Armour:
				// 			entity.armourDefence *= (float)(buffValue[buff.type] * ScaleBuff(buff, buff.rank));
				// 			return;
				// 		case StatType.Type.Health:
				// 			entity.health *= (float)(buffValue[buff.type] * ScaleBuff(buff, buff.rank));
				// 			return;
				// 		case StatType.Type.Mana:
				// 			entity.mana *= (float)(buffValue[buff.type] * ScaleBuff(buff, buff.rank));
				// 			return;
				// 		case StatType.Type.Energy:
				// 			entity.energy *= (float)(buffValue[buff.type] * ScaleBuff(buff, buff.rank));
				// 			return;
				// 	}
				// }
				// if (buff is StatusBuff)
				// {
				// 	switch (buff as StatusBuff)
				// 	{
				// 		case StatusBuff.Type.Blinded:
				// 			entity.accuracy = 0;
				// 			return;
				// 	}
				// }
			}
		}

		public static double ScaleBuff(IBuff buff, int rank)
		{
			if (buff is StatBuff)
			{
				if (StatBuffScale.ContainsKey((buff as StatBuff).Type) == false) return 1; // Default
				return Math.Pow(StatBuffScale[(buff as StatBuff).Type], rank);
			}
			if (buff is StatusBuff)
			{
				if (StatusBuffScale.ContainsKey((buff as StatusBuff).Type) == false) return 1; // Default
				return Math.Pow(StatusBuffScale[(buff as StatusBuff).Type], rank);
			}
			return 1; // Default
		}

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
					return MathsLib.Compare(entity.GetMileage(mileageCond.MileageType), mileageCond.Mileage, mileageCond.Comparative);
				case ResourceCondition resourceCond:
					return MathsLib.Compare(entity.GetResource(resourceCond.ResourceType), resourceCond.RequiredAmount, resourceCond.Comparative);
				case CounterCondition counterCond:
					return MathsLib.Compare(entity.GetCounter(counterCond.CounterType), counterCond.RequiredCount, counterCond.Comparative);
				case EntityInventoryCondition entityInventoryCond:
					if (entityInventoryCond.RequireNoItems && entity.GetInventory(entityInventoryCond.ItemType) > 0)
						return false; // Entity should not have the item
					if (entityInventoryCond.RequireHoldingItems && entity.GetInventory(entityInventoryCond.ItemType) == 0)
						return false; // Entity should have the item
					if (entityInventoryCond.RequiredCount < 0)
						return true; // No specific count required, just check if the item exists
					return MathsLib.Compare(entity.GetInventory(entityInventoryCond.ItemType), entityInventoryCond.RequiredCount, entityInventoryCond.Comparative);
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
			return behaviourType == entity.Behaviour;
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
					return entity.GetMileage(mileageCond.MileageType);
				case ResourceCondition resourceCond:
					return entity.GetResource(resourceCond.ResourceType);
				case CounterCondition counterCond:
					return entity.GetCounter(counterCond.CounterType);
				case EntityInventoryCondition entityInventoryCond:
					return entity.GetInventory(entityInventoryCond.ItemType);
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