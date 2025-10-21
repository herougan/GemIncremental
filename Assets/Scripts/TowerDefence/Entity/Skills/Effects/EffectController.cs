using System.Collections.Generic;
using Util.Debug;
using TowerDefence.Context;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Manager;

namespace TowerDefence.Entity.Skills.ActionHandler
{
	public static class EffectController
	{
		public static Dictionary<ActionType, IActionHandler> ActionHandlers { get; }

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
			foreach (Action action in effect.Actions)
			{
				ApplyAction(trigger, Entity, action);
			}
		}

		public static void ApplyAction(TriggerContext trigger, IEntity Entity, IAction action)
		{
			ActionHandlers.TryGetValue(action.ActionType, out var handler);
			if (handler != null)
			{
				handler.ApplyAction(GameManager.Instance.GameContext, trigger, Entity, action);
			}
			else
			{
				LogManager.Instance.LogWarning($"No IActionHandler found for ActionType: {action.ActionType}");
			}
		}
	}
}