using System;
using TowerDefence.Context;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;

namespace Util.Events
{
	[Serializable]
	public class WrappedAction
	{
		public ISkill Ref { get; }
		public Action<TriggerContext> Action { get; }
		public Action<TriggerContext> Trigger { get; set; }
		public TriggerType TriggerType { get; set; }

		public WrappedAction(Action<TriggerContext> trigger, Action<TriggerContext> action, ISkill skill, TriggerType triggerType)
		{
			Ref = skill;
			Action = action;
			Trigger = trigger;
			TriggerType = triggerType;

			// Subscribes the Invoke method to the trigger event
			Trigger += Invoke;
		}

		public WrappedAction(Action<object[]> trigger, Action<TriggerContext> action, ISkill skill, TriggerType triggerType)
		{
			Ref = skill;
			Action = action;
			Action<TriggerContext> ctxTrigger = delegate { };
			// Create a adapter
			trigger += args => ctxTrigger.Invoke(new TriggerContext());

			// Listen to the adapter instead of the original trigger
			Trigger = ctxTrigger;
			TriggerType = triggerType;

			// Subscribes the Invoke method to the trigger event
			Trigger += Invoke;
		}

		public void Invoke(TriggerContext ctx) => Action?.Invoke(ctx);
	}
}