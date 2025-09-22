using System;
using System.Collections.Generic;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects
{
	public interface IEffect
	{
		// When
		public List<ITrigger> Triggers { get; }
		// If
		public List<ICondition> Conditions { get; }
		public List<ICondition> TargetConditions { get; }
		// Do
		public List<IAction> Actions { get; }


		// Methods
		public void Recalculate(ddouble scale);
		public void ApplyAction(IEntity source, IEntity target);
	}

	/// <summary>
	/// Effect is a data object containing triggers, conditions, and actions.
	/// ActionHandler manages the logic of subscribing actions based on trigger(s). Upon any trigger,
	/// it checks all condition(s), and if they are met, executes the action(s).
	/// 
	/// A simple effect is one that has one trigger, no conditions, and one action.
	/// </summary>
	[Serializable]
	public class Effect : IEffect
	{
		#region Preamble
		public List<ITrigger> Triggers { get; private set; }
		//
		public List<ICondition> Conditions { get; private set; }
		public List<ICondition> TargetConditions { get; private set; }
		//
		public List<IAction> Actions { get; private set; }

		public override string ToString()
		{
			string effectString = "";

			// Build triggers
			if (Triggers.Count > 0) effectString = "Upon ";
			foreach (var trigger in Triggers)
			{
				effectString += trigger + ", ";
			}

			// Build conditions
			if (Conditions.Count + TargetConditions.Count > 0) effectString += "if ";
			foreach (var condition in Conditions)
			{
				effectString += condition + ", ";
			}
			if (TargetConditions.Count > 0)
			{
				foreach (var targetCondition in TargetConditions)
				{
					effectString += "Target " + targetCondition + ", ";
				}
			}

			// Build actions
			if (Actions.Count > 0) effectString += " then";
			foreach (var action in Actions)
			{
				effectString += action + ", ";
			}
			return effectString;
		}
		#endregion Preamble

		#region Method

		public void Recalculate(ddouble scale)
		{
			foreach (IAction action in Actions)
			{
				action.Recalculate(scale);
			}
		}

		public void ApplyAction(IEntity source, IEntity target)
		{
			foreach (IAction action in Actions)
			{
				action.ApplyAction(source, target);
			}
		}

		#endregion Method

	}
}