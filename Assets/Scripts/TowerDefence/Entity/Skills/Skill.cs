
using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.ActionHandler;
using TowerDefence.Entity.Skills.Passives;
using TowerDefence.Context;
using TowerDefence.Stats;
using Util.Events;
using Util.Game;
using Util.Maths;


namespace TowerDefence.Entity.Skills
{
	public interface ISkill : ISource
	{
		// Info
		SkillPlan Plan { get; }
		bool IsPositive { get; }

		// Data
		public ddouble Scale { get; }

		// State
		List<CountdownTimer> Timers { get; }
		List<WrappedAction> WrappedActions { get; }

		// ===== Init =====
		// public void SetSelfInit();

		// ===== Skill Effects =====
		// public void ApplyAction(IEntity source, IEntity target);

		// Events
		public event Action<IEntity> OnLearned; // Applied on

		// Methods
		public void Tick(float t);

		/// <summary>
		/// Walks this skill's Plan.Effects -> Triggers, and subscribes a WrappedAction onto the
		/// matching event on Caster (or a CountdownTimer, for OnPeriodic) for each one. This is what
		/// actually makes a learned skill "live" - call once the skill has a Caster.
		/// </summary>
		public void RegisterCallbacks();

		/// <summary>
		/// Detaches every WrappedAction this skill registered (e.g. on unlearn/buff expiry).
		/// </summary>
		public void Deregister();

		/// <summary>
		/// Applies this Skill's own Plan.Passives directly onto Caster's stats - "the Skill would either
		/// do nothing, or loop through its Passives and just apply them" per the design discussion.
		/// Called automatically at the end of RegisterCallbacks, so a Skill's Passives go live the
		/// moment it's learned (at Spawn, or later via ApplyBuffActionHandler) without a separate step -
		/// there is no longer a whole-Entity "recompute every Passive" pass; each Skill applies only
		/// its own.
		/// </summary>
		public void ApplyPassive();

		/// <summary>
		/// Exactly undoes whatever ApplyPassive last applied - called automatically at the end of
		/// Deregister, so unlearning/cleansing a Skill always removes its Passive contribution along
		/// with its WrappedActions, never one without the other. Removes each IStatMod this Skill
		/// registered by reference (see Entity.DeregisterStatMod), so it doesn't disturb any other
		/// source's mods on the same stat, and order relative to them doesn't matter.
		/// </summary>
		public void UnapplyPassive();
	}

	[Serializable]
	public class Skill : ISkill
	{
		// Info
		public ddouble Scale { get; protected set; }


		// Meta
		public bool IsPassive { get; protected set; }
		public bool IsPositive { get; protected set; }
		public bool ForMonster { get; protected set; }
		public bool ForTower { get; protected set; }

		// Events
		public event Action<IEntity> OnLearned;

		// State
		public List<CountdownTimer> Timers { get; protected set; }
		public List<WrappedAction> WrappedActions { get; protected set; }

		// Main Skill Info

		public SkillPlan Plan { get; protected set; }

		public IEntity Caster { get; protected set; }

		public Skill(SkillPlan plan, IEntity caster = null, ddouble scale = default(ddouble))
		{
			Plan = plan;
			Caster = caster;
			Scale = scale;
			Timers = new List<CountdownTimer>();
			WrappedActions = new List<WrappedAction>();
		}

		#region Methods

		public void Tick(float t)
		{
			// Tick timers
			foreach (var timer in Timers)
			{
				timer.Tick(t);
			}
		}

		// virtual - Aura overrides this to also register itself with AuraPropagationService (only its
		// own source-owned copy, not a received IsInstance one - see Aura.cs). RegisterCallbacks/
		// Deregister are already "the only place a Skill/Buff goes live/dies" per the rest of this
		// codebase, so it's the correct single hook rather than a second parallel lifecycle.
		public virtual void RegisterCallbacks()
		{
			if (Caster == null)
			{
				Util.Debug.LogManager.Instance.LogWarning($"Skill {Plan?.Name} has no Caster - cannot register its triggers.");
				return;
			}
			foreach (IEffect effect in Plan.Effects)
			{
				foreach (ITrigger trigger in effect.Triggers)
				{
					RegisterTrigger(trigger, effect);
				}
			}
			OnLearned?.Invoke(Caster);
			Caster.RaiseEvent(TriggerType.OnSkillLearned, new TriggerContext { Entity = Caster, Source = this });
			ApplyPassive();
		}

		// Every IStatMod this Skill has registered onto Caster, so UnapplyPassive can remove exactly
		// those (by reference) without needing to know anything about what else is registered for the
		// same StatType - see Entity.RegisterStatMod/DeregisterStatMod.
		readonly List<IStatMod> registeredStatMods = new();

		public void ApplyPassive()
		{
			if (Caster == null || Plan?.Passives == null) return;

			foreach (IPassive passive in Plan.Passives)
			{
				if (passive.Conditions != null && !EntityUtil.Check(Caster, passive.Conditions)) continue;

				// A Passive's Multiplier/Bonus decompose into up to two StatMods, both sourced from this
				// Skill - Multiply and Add are separate entries in Caster's registry (see
				// EntityUtil.ApplyStatMods) rather than one combined "multiply-then-add" mod, so they
				// correctly bucket alongside every other source's Add/Multiply mods on the same stat,
				// not just this Passive's own two.
				if (passive.Multiplier != 1)
				{
					IStatMod multiplyMod = new StatMod(passive.Multiplier, passive.Type, MathOperation.Multiply, this);
					registeredStatMods.Add(multiplyMod);
					Caster.RegisterStatMod(multiplyMod);
				}
				if (passive.Bonus != 0)
				{
					IStatMod addMod = new StatMod(passive.Bonus, passive.Type, MathOperation.Add, this);
					registeredStatMods.Add(addMod);
					Caster.RegisterStatMod(addMod);
				}
			}
		}

		public void UnapplyPassive()
		{
			if (Caster == null) return;

			foreach (IStatMod mod in registeredStatMods) Caster.DeregisterStatMod(mod);
			registeredStatMods.Clear();
		}

		private void RegisterTrigger(ITrigger trigger, IEffect effect)
		{
			WrappedAction wrapped;
			if (trigger.Type == TriggerType.OnPeriodic)
			{
				// Periodic triggers aren't an Entity event - they're driven by their own timer.
				CountdownTimer timer = new CountdownTimer(trigger.Parameter, true);
				Timers.Add(timer);
				Caster.AddTimer(timer);
				wrapped = new WrappedAction(Caster, timer, ctx => EffectController.ApplyAction(ctx, Caster, effect), this);
			}
			else
			{
				// GetEvent returning null here would just mean "nobody's subscribed to this TriggerType
				// yet" (true for its first-ever subscriber) - every TriggerType has a real dictionary
				// slot now, so there's nothing to gate on; SubscribeEvent (inside WrappedAction) handles
				// both the first-subscriber and already-has-subscribers cases.
				wrapped = new WrappedAction(Caster, trigger.Type, ctx => EffectController.ApplyAction(ctx, Caster, effect), this);
			}
			WrappedActions.Add(wrapped);
			Caster.WrappedActions.Add(wrapped);
		}

		public virtual void Deregister()
		{
			foreach (WrappedAction wrapped in WrappedActions)
			{
				wrapped.Detach();
				Caster?.WrappedActions.Remove(wrapped);
			}
			WrappedActions.Clear();
			UnapplyPassive();
			Caster?.RaiseEvent(TriggerType.OnSkillUnlearned, new TriggerContext { Entity = Caster, Source = this });
		}

		#endregion Methods
	}

	// ===== Spirce and Affects =====
	public interface ISource
	{
		public IEntity Caster { get; }
		// public List<IAffect> Affects { get; }
	}

	// Affects point to source. Do source point back to affects? Do affects tell you what they are affecting?!

	public interface IAffect
	{
		public ISource Source { get; }
		public IEntity Affected { get; }
	}
}