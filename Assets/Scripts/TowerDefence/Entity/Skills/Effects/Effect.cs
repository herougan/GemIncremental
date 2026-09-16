using System;
using System.Collections.Generic;
using UnityEngine;
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
		// Interface-typed lists - Unity's default serializer can't write through an interface reference
		// at all (silently ends up empty), so these need SerializeReference to survive being saved in a
		// SkillPlan asset. Every CONCRETE class ever written into these needs its own [Serializable] for
		// that to actually work - not just inherited from a [Serializable] base, since SerializeReference
		// checks obj.GetType() - Trigger has it (and is the only concrete ITrigger, so nothing else to
		// check there), and every Condition subclass (Condition.cs) and every Action subclass
		// (ProjectileAction/SpawnAction) has it now too. This was the actual cause of a real bug: Action
		// itself having [Serializable] was NOT enough for ProjectileAction/SpawnAction, which silently
		// lost their own extra fields (Plan, Monster, ...) on the next domain reload after being authored -
		// see ProjectileAction.cs's own doc comment. Any *new* Action/Condition subclass needs the same
		// attribute on the day it's written, not just "whenever this bites."
		[field: SerializeReference] public List<ITrigger> Triggers { get; private set; } = new List<ITrigger>();
		//
		[field: SerializeReference] public List<ICondition> Conditions { get; private set; } = new List<ICondition>();
		[field: SerializeReference] public List<ICondition> TargetConditions { get; private set; } = new List<ICondition>();
		//
		[field: SerializeReference] public List<IAction> Actions { get; private set; } = new List<IAction>();

		public Effect() { }

		public Effect(List<ITrigger> triggers, List<IAction> actions, List<ICondition> conditions = null, List<ICondition> targetConditions = null)
		{
			Triggers = triggers ?? new List<ITrigger>();
			Actions = actions ?? new List<IAction>();
			Conditions = conditions ?? new List<ICondition>();
			TargetConditions = targetConditions ?? new List<ICondition>();
		}

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