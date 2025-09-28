using Util.Debug;
using UnityEngine;

namespace TowerDefence.Entity.Skills.Trigger
{
	public class TriggerController : MonoBehaviour
	{
		/* This class is responsible for managing triggers in the game
		It can handle various types of triggers and their associated actions and is responsible for
		hooking them up to their wouldbe listeners*/

		public void HandleTrigger(IEntity entity, ISkill skill)
		{
			// if (action is ProjectileAction projectileAction)
			// {
			// 	var handler = new ProjectileActionHandler();
			// 	handler.ApplyAction(context, triggerContext, action);
			// }
			// else
			// {
			// 	Debug.LogWarning($"Unhandled action type: {action.Type}");
			// }

			// entity.onTrigger?.Invoke(context, triggerContext, action);
			// entity.onDeath?.Invoke(context, triggerContext, action);

			// switch ()

		}

		public void HandleTrigger(IEntity entity, ITrigger trigger)
		{
			switch (trigger.Type)
			{
				case TriggerType.OnHit:
					// Handle OnHit trigger
					// entity.OnHit += (ctx, trgCtx, action) =>
					// {
					// 	EffectController.ApplyAction(ctx, trgCtx, action);
					// };
					break;
				case TriggerType.OnDeath:
					// Handle OnDeath trigger
					break;
				// case TriggerType.OnBuffApplied:
				// 	// Handle OnBuffApplied trigger
				// 	break;
				// case TriggerType.OnBuffExpired:
				// 	// Handle OnBuffExpired trigger
				// 	break;
				default:
					LogManager.Instance.LogWarning($"Unhandled trigger type: {trigger.Type}");
					break;
			}
		}
	}
}