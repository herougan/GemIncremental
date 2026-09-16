using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	/// <summary>
	/// Handles ActionType.Revenge: "a unit that takes revenge against all its damage sources every 10s"
	/// from the design discussion - pair with an OnPeriodic(10) Trigger on the Effect that carries this
	/// Action. Deals action.Value% of however much each attacker in Entity.DamageReceivedLog dealt back
	/// at that same attacker, then clears the log - this is the "memory" the skill needs, and it's
	/// already being kept for free by every GotHit call (see Entity.GotHit), not something this handler
	/// maintains itself.
	///
	/// Deliberately its own ActionType rather than reusing Reflect: Reflect answers one specific hit
	/// (trigger.Damage, on OnHit) with one target (trigger.Target); this answers a *log* of many hits
	/// from potentially many different attackers, on a schedule unrelated to any single hit.
	/// </summary>
	public class RevengeActionHandler : ActionHandler
	{
		public RevengeActionHandler()
		{
			Type = ActionType.Revenge;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (Entity.DamageReceivedLog.Count == 0) return;

			double percent = (double)action.Value;
			// Copy the keys - Entity.ApplyDamage on an attacker could theoretically trigger effects that
			// loop back and touch this same Entity's log mid-iteration; iterating a snapshot avoids that
			// mutating the collection out from under this foreach.
			foreach (KeyValuePair<IEntity, ddouble> received in new List<KeyValuePair<IEntity, ddouble>>(Entity.DamageReceivedLog))
			{
				IEntity attacker = received.Key;
				ddouble amount = received.Value * percent;
				if ((double)amount <= 0) continue;

				Damage revenge = new Damage(StatType.Health, amount, Entity);
				revenge.SetTarget(attacker);
				revenge.SetSource(trigger?.Source);
				attacker.ApplyDamage(revenge);
			}

			Entity.ClearDamageLog();
		}
	}
}
