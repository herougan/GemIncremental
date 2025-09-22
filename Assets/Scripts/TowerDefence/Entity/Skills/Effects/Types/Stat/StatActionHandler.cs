
using TowerDefence.Context;

namespace TowerDefence.Entity.Skills.Effects.Types.Stat
{
	public class StatActionHandler : IActionHandler
	{
		public ActionType Type => ActionType.Stat;

		public void ApplyAction(GameContext context, Action effect)
		{
			// Default implementation for applying effects
			TriggerContext triggerContext = context.GetTriggerContext(effect.Trigger);
			IEntity entity = triggerContext.Target;
			entity.GetStat(effect.StatType).OperateValue(effect.Value, effect.Operation);


		}

		public void Rollback(GameContext context, Action effect)
		{
			// Default implementation for rolling back effects

			// Effectively rollsback effects caused by this action
		}

		public void RemoveEffect(GameContext context, Action effect)
		{
			// Default implementation for removing effects

			// Removes attached effect but those not recalculate stats
		}

		public void ApplyAction(in GameContext context, in TriggerContext trigger, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void Rollback(in GameContext context, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveEffect(in GameContext context, Action effect)
		{
			throw new System.NotImplementedException();
		}
	}
}