using System;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using Util.Maths;

namespace Util.Events
{
	/// <summary>
	/// Subscribes an Effect's action onto whatever actually triggers it - either an entity-level
	/// TriggerType (via Entity.SubscribeEvent) or a periodic skill's own CountdownTimer - and remembers
	/// enough to cleanly detach later (see Detach). Constructed once per Trigger registration; the
	/// owning Skill/Buff keeps it in its own WrappedActions list (and Entity.WrappedActions) so it can
	/// be torn down on unlearn/expiry.
	///
	/// Previously this subscribed by taking the *current value* of the triggering delegate as a
	/// parameter and doing `+=` on it - since delegates are immutable, that only ever rebinds the local
	/// copy, never the real field it was read from. Nothing here ever actually attached to a live event.
	/// Fixed by always subscribing through the real object (Entity.SubscribeEvent / CountdownTimer.OnRing
	/// on the CountdownTimer instance itself), never a delegate value copied out of one.
	/// </summary>
	public class WrappedAction
	{
		public ISkill Ref { get; }
		public Action<TriggerContext> Action { get; }
		public TriggerType TriggerType { get; }

		// Exactly one of these is set, depending on which constructor built this WrappedAction - it's
		// what Detach needs to unsubscribe from the right place.
		readonly IEntity entity;
		readonly CountdownTimer timer;

		/// <summary>Entity-level trigger (OnDeath, OnHit, stat events, ...) - dispatched via Entity's TriggerType dictionary.</summary>
		public WrappedAction(IEntity entity, TriggerType triggerType, Action<TriggerContext> action, ISkill skill)
		{
			this.entity = entity;
			TriggerType = triggerType;
			Action = action;
			Ref = skill;
			entity.SubscribeEvent(triggerType, Invoke);
		}

		/// <summary>
		/// Periodic trigger - one dedicated CountdownTimer per Effect (different periods can't share a
		/// dictionary slot without cross-talk). Ringing both applies the effect directly (private, no
		/// dictionary) and raises the public OnSkillActivate trigger on the entity, so anything watching
		/// "this entity did something" (EM bookkeeping, reactive auras) sees it via the normal path
		/// without periodic ticks needing any special-casing of their own.
		/// </summary>
		public WrappedAction(IEntity entity, CountdownTimer timer, Action<TriggerContext> action, ISkill skill)
		{
			this.entity = entity;
			this.timer = timer;
			TriggerType = TriggerType.OnPeriodic;
			Action = action;
			Ref = skill;
			timer.OnRing += OnTimerRing;
		}

		void OnTimerRing(CountdownTimer ring)
		{
			// Deliberately not entity.RaiseEvent - this application is private to this one timer/effect,
			// not a broadcast - but that means nothing else sets TriggerType on this context, so it's set
			// explicitly here rather than silently defaulting to TriggerType.OnReached (enum value 0).
			Invoke(new TriggerContext { TriggerType = TriggerType.OnPeriodic, Entity = entity, Period = ring.countdownTime });
			entity.RaiseEvent(TriggerType.OnSkillActivate, new TriggerContext { Entity = entity, Source = Ref });
		}

		/// <summary>
		/// Stamps ctx.Source with the Skill/Buff this WrappedAction belongs to before invoking, unless
		/// something upstream already set a more specific one - the one place a TriggerContext actually
		/// learns "which Skill caused this," since RaiseEvent itself only ever sets Entity/Target/Damage.
		/// Downstream, DamageActionHandler/ReflectActionHandler/ProjectileActionHandler copy this onto
		/// the Damage they construct (Damage.Source) - what a battle-analysis report attributes damage
		/// to (see Util.Game.BattleTracker), not something every Skill needs to report itself.
		/// </summary>
		public void Invoke(TriggerContext ctx)
		{
			ctx.Source ??= Ref;
			Action?.Invoke(ctx);
		}

		public void Detach()
		{
			if (timer != null) timer.OnRing -= OnTimerRing;
			else entity?.UnsubscribeEvent(TriggerType, Invoke);
		}
	}
}
