using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity.Skills.Effects;
using Util.Game;

namespace TowerDefence.Entity.Skills.Buffs
{
	// Aura : Buff, not a merged/parallel type - an Aura *is* a Buff (same Skill/Effect/Trigger backing,
	// same Stack/Expire handling), just one that also propagates to other entities within Range rather
	// than only affecting whoever holds it. BuffType.Aura already exists for cheap categorization
	// (e.g. filtering a UI list) without needing an `is Aura` check, but `is Aura`/pattern-matching is
	// the source of truth - keep BuffType in sync with the actual runtime type, don't let them drift.
	//
	// An entity holding a received IsInstance=true copy is an "aura-satellite" - it doesn't itself
	// re-spread the aura to anyone in *its* range unless AuraSpreading is on, i.e. relay/chained
	// propagation is opt-in per Aura, not automatic just because an entity happens to have one.
	public interface IAura : IBuff
	{
		SkillPlan Data { get; }
		float Range { get; }
		bool IsAffectSelf { get; }
		bool IsAffectOthers { get; }
		bool IsInstance { get; } // true if this object is a copy applied to a nearby entity because it's in range of the source; false on the source's own copy.
		bool AuraSpreading { get; } // if true, an aura-satellite (IsInstance == true) holding this re-propagates it to entities within *its own* Range too, chaining outward.

		/// <summary>See Aura.Propagate - AuraPropagationService calls this once per tick interval with exactly the candidates already known to be within this Aura's own Range.</summary>
		void Propagate(IEntity source, IEnumerable<IEntity> candidatesInRange);
	}

	public class Aura : Buff, IAura
	{
		/// <summary>Range is in units of tiles. (1 tile is x=1, y=1 big)</summary>
		public float Range { get; private set; }
		public bool IsAffectSelf { get; private set; }
		public bool IsAffectOthers { get; private set; } = true;
		public bool IsInstance { get; private set; }
		public bool AuraSpreading { get; private set; }
		public float LingerDuration { get; private set; }

		// SkillPlan, not BuffPlan, on the interface - BuffPlan : SkillPlan (see BuffPlan.cs), so Buff's
		// own Plan (typed BuffPlan) already satisfies this without needing a second field.
		SkillPlan IAura.Data => Plan;

		public Aura(BuffPlan plan, IEntity caster = null, float range = 0, bool isAffectSelf = false, bool isAffectOthers = true, bool isInstance = false, bool auraSpreading = false, float lingerDuration = 0)
			: base(plan, caster)
		{
			Range = range;
			IsAffectSelf = isAffectSelf;
			IsAffectOthers = isAffectOthers;
			IsInstance = isInstance;
			AuraSpreading = auraSpreading;
			LingerDuration = lingerDuration;

			// A received instance stays alive for as long as its recipient is in range, not for
			// Plan.Duration (that's how long the effect should persist on the SOURCE, an unrelated
			// number) - Leave() below is the only thing that ever gives it a real countdown, via
			// SetDuration(LingerDuration). Duration <= 0 already means "permanent" (see Buff.IsExpired).
			if (isInstance) SetDuration(0);
		}

		#region Spatial propagation

		// Who currently holds a granted copy of THIS aura, keyed by recipient - "the aura checks," not
		// a central registry trying to reconstruct who's affected by what. Only ever populated/read on
		// the source's own (IsInstance == false) copy; a received instance never calls Propagate itself
		// (see AuraPropagationService, which only registers non-instance auras) unless/until
		// AuraSpreading's relay behaviour is built - still a STUB, same as before this pass.
		readonly Dictionary<IEntity, IAura> affected = new();

		/// <summary>
		/// Called by AuraPropagationService once per tick interval with exactly the candidates already
		/// known to be within THIS aura's own Range (the service does the spatial/distance work once per
		/// source, batched across every aura that source owns - see its own doc comment) - this method
		/// only ever does the conditional-grant/enter/exit bookkeeping, never a spatial query itself.
		/// </summary>
		public void Propagate(IEntity source, IEnumerable<IEntity> candidatesInRange)
		{
			if (!IsAffectOthers) return; // This aura never leaves its caster - nothing to propagate.

			HashSet<IEntity> qualifying = new();
			foreach (IEntity candidate in candidatesInRange)
			{
				if (!PassesConditions(candidate)) continue;
				qualifying.Add(candidate);
				if (!affected.ContainsKey(candidate)) Grant(source, candidate);
			}

			// Snapshot via ToList - Leave() mutates `affected` (via RemoveAuraInstance's own bookkeeping
			// on the way back, indirectly), which would otherwise be modifying this dictionary's key set
			// while still enumerating it.
			foreach (IEntity previous in affected.Keys.Except(qualifying).ToList())
			{
				Leave(previous);
			}
		}

		/// <summary>"Conditionally" grant - reuses whatever TargetConditions this Aura's own Plan.Effects already declare (same ConditionsMet gate EffectController uses for a normal Effect's Target), rather than inventing a second condition system just for Auras. No conditions on any Effect = no restriction.</summary>
		bool PassesConditions(IEntity candidate)
		{
			foreach (IEffect effect in Plan.Effects)
			{
				if (effect.TargetConditions.Count > 0 && !EntityUtil.Check(candidate, effect.TargetConditions)) return false;
			}
			return true;
		}

		void Grant(IEntity source, IEntity candidate)
		{
			Aura instance = new Aura(Plan, source, Range, IsAffectSelf, IsAffectOthers, isInstance: true, AuraSpreading, LingerDuration);
			affected[candidate] = instance;
			candidate.ApplyAuraInstance(instance);
		}

		/// <summary>candidate left Range (or stopped qualifying) - either it keeps the Buff a while longer as a normal timed one (LingerDuration > 0) or it's cleansed immediately.</summary>
		void Leave(IEntity candidate)
		{
			IAura instance = affected[candidate];
			affected.Remove(candidate);

			if (LingerDuration > 0)
			{
				instance.SetDuration(LingerDuration);
				instance.Refresh(); // Time = 0 - the linger window starts now, not whenever it happened to last tick.
			}
			else
			{
				candidate.RemoveAuraInstance(instance);
			}
		}

		#endregion Spatial propagation

		#region Lifecycle

		/// <summary>Registers with AuraPropagationService too - only the source's own copy (IsInstance == false) drives propagation; a received instance doesn't scan for anyone itself (see AuraSpreading's own still-unbuilt relay note above).</summary>
		public override void RegisterCallbacks()
		{
			base.RegisterCallbacks();
			if (!IsInstance) AuraPropagationService.Register(Caster);
		}

		public override void Deregister()
		{
			base.Deregister();
			if (!IsInstance) AuraPropagationService.Unregister(Caster);
		}

		#endregion Lifecycle
	}
}
