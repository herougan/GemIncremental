using System;
using TowerDefence.Context;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Effects;
using Util.Maths;

namespace Util.Events
{
	[Serializable]
	/// <summary>
	/// A wrapper class that encapsulates an action to be triggered by a specific event,
	/// along with a reference to the skill that created it.
	/// This class subscribes to the trigger event upon instantiation!
	/// </summary>
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

		public WrappedAction(Action<CountdownTimer> trigger, Action<TriggerContext> action, ISkill skill)
		{
			Ref = skill;
			Action = action;
			Action<TriggerContext> ctxTrigger = delegate { };
			// Create a adapter
			trigger += arg => ctxTrigger.Invoke(new TriggerContext
			{
				TriggerType = TriggerType.OnPeriodic,
				Period = arg.countdownTime,
			});

			// Listen to the adapter instead of the original trigger
			Trigger = ctxTrigger;

			// Subscribes the Invoke method to the trigger event
			Trigger += Invoke;
		}

		public WrappedAction(Action<object[]> trigger, Action<TriggerContext> action, ISkill skill, TriggerType triggerType)
		{
			Ref = skill;
			Action = action;
			Action<TriggerContext> ctxTrigger = delegate { };
			// Create a adapter
			trigger += args => ctxTrigger.Invoke(new TriggerContext
			{
				TriggerType = triggerType,
			});

			// Listen to the adapter instead of the original trigger
			Trigger = ctxTrigger;

			// Subscribes the Invoke method to the trigger event
			Trigger += Invoke;
		}

		public void Invoke(TriggerContext ctx) => Action?.Invoke(ctx);

		public void Detach()
		{
			// Unsubscribe the Invoke method from the trigger event
			Trigger -= Invoke;
		}
	}
}