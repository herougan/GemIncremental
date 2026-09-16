using System;
using TowerDefence.Entity.Skills.Effects;
using Util.Game;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Buffs
{
	public interface IBuff : ISkill, IExpirable // giving IBuff these interfaces forces the abstract object to have effect when the data ALREADY has effect.
	{
		// ISkill and IExpirable both declare Tick(float) independently - without IBuff re-declaring it
		// here, calling buff.Tick(t) on an IBuff-typed reference (e.g. Entity.Tick's `foreach (IBuff buff
		// in Buffs...) buff.Tick(t)`) is ambiguous (CS0121), since neither parent is "more specific" than
		// the other from IBuff's point of view. `new` picks IBuff's own as the one member call sites see;
		// Buff's own `public new void Tick(float time)` (see Buff.cs) satisfies it same as before.
		new void Tick(float time);

		// Functions
		bool EqualType(IBuff buff);
		void ApplyOtherEffects(); // Apply any other effects that are not part of the main effect

		// Bool
		bool IsExpireOnTrigger { get; }

		// Properties
		int Rank { get; }
		ddouble Value { get; }
		BuffType BuffType { get; }
		BuffStackType BuffStackType { get; }
		bool BuffStackCascade { get; } // Upon Skill stack, cascade stacking to the individual effects

		// Events
		public event Action<IBuff, IBuff> OnBuffStack;

		// Methods
		public void Stack(IBuff buff);
		public void Stack(float duration, int rank, float value, IBuff buff);
		public void Refresh();

		/// <summary>
		/// How many times this has been (re)applied, unlimited - only meaningful for BuffType.StatusBuff
		/// (DoT-style: Poison/Burn) per the design discussion. Buffs proper (BuffType.StatBuff/Aura/Saga)
		/// don't stack at all - reapplying just Refresh()es and Stacks stays at 1. CC-style statuses
		/// (Stun/Freeze/Paralyse) don't use this either - those go through Resistance's own buildup/
		/// threshold/reset model (Entity/Stats/Stat.cs), which doesn't stack in this sense at all.
		/// </summary>
		public int Stacks { get; }
		public void AddStack();

		/// <summary>Overwrites Duration directly - Duration &lt;= 0 means "permanent" (see IsExpired), so this is also how an aura-granted instance is kept alive indefinitely while in range and then given a fixed countdown once it leaves (AuraPropagationService/Aura.Leave) without a whole separate "linger" mechanism.</summary>
		void SetDuration(float duration);
	}

	public enum BuffType
	{
		StatBuff,
		StatusBuff,
		Aura,
		Saga,
	}

	[Serializable]
	public class Buff : Skill, IBuff
	{
		#region Preamble
		// Rank Properties
		public int Rank { get; protected set; } // Rank of the buff, e.g. 1, 2, 3, etc.
		public BuffType BuffType { get; protected set; }
		public BuffStackType BuffStackType { get; protected set; }
		public bool BuffStackCascade { get; protected set; }

		// Properties
		public float Duration { get; protected set; }
		public float Time { get; protected set; }
		public ddouble Value { get; protected set; }
		public ddouble Dynamic { get; protected set; }
		public int Stacks { get; protected set; } = 1;

		// Meta		
		public bool IsReceipient { get; protected set; }
		public bool IsShare { get; set; }
		public bool IsExpireOnTrigger { get; set; }


		// Plan
		public new BuffPlan Plan { get; protected set; }


		// Constructor
		public Buff(BuffPlan plan, IEntity caster = null) : base(plan, caster)
		{
			Plan = plan;
			// Was never copied from the Plan at all before - every fresh Buff had Duration stuck at 0
			// (Stack() was the only thing that ever set it, and only when stacking onto an *existing*
			// buff), which would have made Tick/IsExpired below treat every single Buff as permanent
			// regardless of what its BuffPlan actually authored.
			Duration = plan.Duration;
		}

		public Buff(BuffPlan plan, IEntity caster, ddouble scale) : base(plan, caster, scale)
		{
			Plan = plan;
			Duration = plan.Duration;
		}

		/// <summary>
		/// "Is this the same specific Buff" - by Plan identity (same authored asset), not BuffType (only
		/// 4 values - StatBuff/StatusBuff/Aura/Saga - so matching on that alone would treat every
		/// StatusBuff as interchangeable, e.g. Poison and Burn colliding into one slot). Always returned
		/// false before, which meant Entity.ApplyBuff's "same buff already active" check never actually
		/// used this - it matched on BuffType directly instead, which had the same coarse-collision
		/// problem. Both now go through this.
		/// </summary>
		public bool EqualType(IBuff buff)
		{
			return buff != null && Plan == buff.Plan;
		}

		public void SetDuration(float duration)
		{
			Duration = duration;
		}

		/// <summary>The one place a runtime Buff object gets constructed from a BuffPlan - both Entity's InitBuffs (Spawn) and ApplyBuffActionHandler (ActionType.ApplyBuff) route through this now, instead of each separately hardcoding `new Buff(...)`, so a Plan authored with IsAura actually produces a real Aura object wherever it's granted from, not just from one call site.</summary>
		public static Buff Create(BuffPlan plan, IEntity caster)
		{
			if (!plan.IsAura) return new Buff(plan, caster);
			return new Aura(plan, caster, plan.AuraRange, plan.AuraAffectSelf, plan.AuraAffectOthers, isInstance: false, plan.AuraSpreading, plan.AuraLingerDuration);
		}

		#endregion Preamble

		#region Events

		// Events
		public event Action<IExpirable> OnExpired;
		public event Action<IBuff, IBuff> OnBuffStack;

		#endregion Events

		#region Methods

		/// <summary>Fires OnExpired - the Entity holding this Buff subscribed to that at ApplyBuff time and reacts by cleansing itself (see Entity.ApplyBuff/HandleBuffExpired). Buff itself doesn't remove itself from anything - it has no reference to the Buffs list it's sitting in.</summary>
		public void Expire()
		{
			OnExpired?.Invoke(this);
		}

		public void Init()
		{

		}

		/// <summary>Duration &lt;= 0 means permanent (never expires on its own - Stat/Debuff-type Buffs with no authored Duration are this) - only a positive Duration that Time has caught up to counts as expired.</summary>
		public bool IsExpired()
		{
			return Duration > 0 && Time >= Duration;
		}

		/// <summary>Resets the countdown to full - "buffs of the same name combine such that the duration left refreshes" per the design discussion; see Entity.ApplyBuff's same-BuffType branch.</summary>
		public void Refresh()
		{
			Time = 0;
		}

		/// <summary>Increments Stacks and refreshes the countdown - reapplying a stacking (StatusBuff) status does both, per the design discussion, not just one or the other.</summary>
		public void AddStack()
		{
			Stacks++;
			Refresh();
		}

		public new void Tick(float time)
		{
			if (Duration <= 0) return; // permanent - nothing to count down
			Time += time;
			if (IsExpired()) Expire();
		}

		public void Recalculate(ddouble scale)
		{
			Dynamic = scale * Value;
			// Scale the buff's effects
			foreach (IEffect effect in Plan.Effects)
			{
				effect.Recalculate(scale);
			}
		}

		public void Rescale(double scale)
		{

		}

		public void ApplyOtherEffects()
		{
			// Apply any other effects that are not part of the main effect
			// This could be additional buffs, debuffs, or other modifications
		}

		public void Attach(IEntity source, IEntity target)
		{
			// Source = source;
			// Affected = target;
		}

		public void ApplyAction(IEntity source, IEntity target)
		{
			foreach (IEffect effect in Plan.Effects)
			{
				if (EntityUtil.Check(source, effect.Conditions))
				{
					effect.ApplyAction(source, target);
				}
			}
		}

		public void Stack(float duration, int rank, float value, IBuff buff)
		{
			this.Duration = duration;
			this.Time = 0; // The freshly-combined Duration should count down from zero, not from wherever the old one had ticked to.
			this.Rank = rank;
			this.Value = value;
			OnBuffStack?.Invoke(this, buff);
		}

		/// <summary>
		/// The real "reapplying this stacks it" path - computes new Duration/Rank/Value via
		/// BuffStackType.StackValue (this Buff's own MathOperation triple - see StatusStackTypes for the
		/// named archetypes: Fire/Poison/Debilitating/Disease), then bumps Stacks the same way AddStack
		/// always did. Previously these were two disconnected mechanisms (this one never touched Stacks,
		/// AddStack never touched Duration/Value) - unified so Entity.ApplyBuff's StatusBuff branch can
		/// call just this one method and get both effects.
		/// </summary>
		public void Stack(IBuff buff)
		{
			BuffStackType.StackValue(this, (Buff)buff);
			Stacks++;
		}

		#endregion Methods
	}

	// ====== Enum ======
	[Serializable]
	public struct BuffStackType
	{
		public MathOperation TimeOperation;
		public MathOperation RankOperation;
		public MathOperation ValueOperation;

		public BuffStackType(MathOperation timeOperation, MathOperation rankOperation, MathOperation valueOperation)
		{
			TimeOperation = timeOperation;
			RankOperation = rankOperation;
			ValueOperation = valueOperation;
		}

		public static void StackValue(IBuff A, IBuff B)
		{
			if (A == null || B == null)
				return;
			if (A.BuffType != B.BuffType)
				return;
			float duration = (float)MathsLib.Operate(A.Duration, B.Duration, A.BuffStackType.TimeOperation);
			int rank = (int)MathsLib.Operate(A.Rank, B.Rank, A.BuffStackType.RankOperation);
			// Was A.Duration, B.Duration here (copy-paste from the line above) - Value, not Duration, is
			// what ValueOperation is supposed to combine.
			float value = (float)MathsLib.Operate((double)A.Value, (double)B.Value, A.BuffStackType.ValueOperation);
			A.Stack(duration, rank, value, B);
		}
	}
}
