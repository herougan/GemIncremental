using TowerDefence.Context;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	/// <summary>
	/// Handles ActionType.Reflect: deals a % (read from the acting Entity's Reflect stat) of
	/// trigger.Damage.FinalValue back at trigger.Target - the mechanism behind a "Mirror" passive
	/// (OnHit trigger + this action). Reflects FinalValue (what actually landed on Health after
	/// Nullifier/Shield absorption), not the hit's original pre-mitigation Value - see Damage.FinalValue
	/// for why; a fully-shielded hit reflects nothing back, which is the intended read for now.
	///
	/// No-ops (rather than warning) if there's nothing to reflect (no incoming Damage on this trigger,
	/// no Target, or a Reflect stat of 0) - OnHit fires for every hit, most of which won't have this
	/// skill attached or won't have a nonzero Reflect stat, so silence is the right default here.
	/// </summary>
	public class ReflectActionHandler : ActionHandler
	{
		public ReflectActionHandler()
		{
			Type = ActionType.Reflect;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (trigger?.Damage == null || trigger.Target == null) return;

			ddouble reflectPercent = Entity.GetStat(StatType.Reflect);
			if ((double)reflectPercent <= 0) return;

			Damage reflected = new Damage(StatType.Health, trigger.Damage.FinalValue * reflectPercent, Entity);
			reflected.SetTarget(trigger.Target);
			reflected.SetElement(trigger.Damage.Element);
			reflected.SetSource(trigger.Source); // the reflecting Skill (e.g. Mirror) itself, not the original attacker's Source - this is a new hit it's dealing
			foreach (Tag tag in trigger.Damage.Tags) reflected.AddTag(tag);
			trigger.Target.ApplyDamage(reflected);
		}
	}
}
