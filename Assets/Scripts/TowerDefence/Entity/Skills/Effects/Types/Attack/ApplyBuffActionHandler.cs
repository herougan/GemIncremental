using TowerDefence.Context;
using TowerDefence.Entity.Skills.Buffs;
using Util.Debug;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	/// <summary>
	/// Handles ActionType.ApplyBuff: grants action.BuffToApply to trigger.Target, falling back to the
	/// acting Entity itself if no Target is set (self-buffs/periodic ticks - same fallback
	/// StatActionHandler uses). This is *the* mechanism for a Skill to grant a Buff - "Buffs are a type
	/// of Skill" (BuffPlan : SkillPlan), and a Buff itself is granted the same way any other Action
	/// applies: an OnAttack-triggered Skill with an ApplyBuff Action, e.g. "on hit, apply Poisoned".
	///
	/// A periodically-*firing* Buff (e.g. "a Tower buff that spits out fireballs every few seconds") is
	/// a different thing from this - that's an OnPeriodic Effect living directly on the granted Buff's
	/// own BuffPlan (BuffPlan.Effects, inherited from SkillPlan), not something this handler needs to
	/// know about. Once the Buff object exists and is registered (see Entity.ApplyBuff ->
	/// Buff.RegisterCallbacks), its own Effects/Triggers/Passives wire up exactly like any other Skill's.
	/// </summary>
	public class ApplyBuffActionHandler : ActionHandler
	{
		public ApplyBuffActionHandler()
		{
			Type = ActionType.ApplyBuff;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (action is not Action buffAction || buffAction.BuffToApply == null)
			{
				LogManager.Instance.LogWarning("ApplyBuffActionHandler received an action with no BuffToApply.");
				return;
			}

			IEntity target = trigger?.Target ?? Entity;
			Buff buff = Buff.Create(buffAction.BuffToApply, Entity);
			target.ApplyBuff(buff);
		}
	}
}
