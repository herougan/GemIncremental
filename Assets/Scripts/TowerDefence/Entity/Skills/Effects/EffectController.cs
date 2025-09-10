using System.Collections.Generic;
using TowerDefence.Entity.Skills.Effects;

namespace TowerDefence.Entity.Skills.ActionHandler
{
	public static class EffectController
	{
		public static Dictionary<ActionType, IActionHandler> EffectHandlers { get; }

		public static IActionHandler GetEffectHandler(ActionType actionType)
		{
			if (EffectHandlers.TryGetValue(actionType, out var handler))
			{
				return handler;
			}

			return null;
		}

		public static ApplyAction(GameContext context, Effect effect)
		{
			foreach (Action action in effect.Actions)
			{
				ApplyAction(context, action);
			}
		}

		public static ApplyAction(GameContext context, Action action)
		{
			EffectHandlers.TryGetValue(action.Type, out var handler);
			if (handler != null)
			{
				handler.ApplyAction(context, action);
			}
			else
			{
				Debug.LogWarning($"No IActionHandler found for ActionType: {action.Type}");
			}
		}
	}
}