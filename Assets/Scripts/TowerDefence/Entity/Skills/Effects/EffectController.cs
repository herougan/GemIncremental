using System.Collections.Generic;
using Util.Debug;
using TowerDefence.Context;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Effects.Types.Attack;
using TowerDefence.Entity.Skills.Effects.Types.Spawn;
using TowerDefence.Entity.Skills.Effects.Types.Stat;
using TowerDefence.Manager;
using Util.Game;

namespace TowerDefence.Entity.Skills.ActionHandler
{
	public static class EffectController
	{
		/// <summary>
		/// One IActionHandler instance per ActionType. Add a new type's handler here as it's built -
		/// GetEffectHandler/ApplyAction below dispatch on ActionType, so nothing else needs to change.
		/// </summary>
		public static Dictionary<ActionType, IActionHandler> ActionHandlers { get; } = new Dictionary<ActionType, IActionHandler>
		{
			{ ActionType.Stat, new StatActionHandler() },
			{ ActionType.Damage, new DamageActionHandler() },
			{ ActionType.Reflect, new ReflectActionHandler() },
			{ ActionType.Projectile, new ProjectileActionHandler() },
			{ ActionType.ApplyBuff, new ApplyBuffActionHandler() },
			{ ActionType.Revenge, new RevengeActionHandler() },
			{ ActionType.Spawn, new SpawnActionHandler() },
		};

		public static IActionHandler GetEffectHandler(ActionType actionType)
		{
			if (ActionHandlers.TryGetValue(actionType, out var handler))
			{
				return handler;
			}

			return null;
		}

		public static void ApplyAction(TriggerContext trigger, IEntity Entity, IEffect effect)
		{
			if (!ConditionsMet(effect.Conditions, Entity)) return;
			// Only gate on TargetConditions if the Effect actually declared some - a Target-less
			// trigger (e.g. OnPeriodic) with no TargetConditions must still be free to fire.
			if (effect.TargetConditions.Count > 0 && !ConditionsMet(effect.TargetConditions, trigger?.Target)) return;

			foreach (IAction action in effect.Actions)
			{
				ApplyAction(trigger, Entity, action);
			}
		}

		/// <summary>
		/// True if every condition in the list holds for entity - vacuously true for an empty/null list.
		/// A non-empty list checked against a null entity (e.g. TargetConditions with no Target present)
		/// fails closed: a condition about a target can't hold if there is no target. Actual evaluation
		/// is EntityUtil.Check - the one place conditions get evaluated, also used by
		/// Skill.ApplyPassive/UnapplyPassive.
		/// </summary>
		static bool ConditionsMet(List<ICondition> conditions, IEntity entity)
		{
			if (conditions == null || conditions.Count == 0) return true;
			if (entity == null) return false;
			return EntityUtil.Check(entity, conditions);
		}

		public static void ApplyAction(TriggerContext trigger, IEntity Entity, IAction action)
		{
			ActionHandlers.TryGetValue(action.ActionType, out var handler);
			if (handler != null)
			{
				// Instance?. , not .GameContext directly - GameManager.Instance is null in a plain-C#
				// context (a test, or anything before a scene creates one); every handler here ignores
				// GameContext today anyway, so passing null through is harmless, not a silent bug.
				handler.ApplyAction(GameManager.Instance?.GameContext, trigger, Entity, action);
			}
			else
			{
				LogManager.Instance.LogWarning($"No IActionHandler found for ActionType: {action.ActionType}");
			}
		}
	}
}