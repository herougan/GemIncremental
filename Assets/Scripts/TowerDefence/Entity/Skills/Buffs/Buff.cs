using System;
using TowerDefence.Entity.Skills.Effects;
using Util.Game;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Buffs
{
	public interface IBuff : ISkill, IExpirable // giving IBuff these interfaces forces the abstract object to have effect when the data ALREADY has effect.
	{

		// Functions
		bool EqualType(IBuff buff);
		void ApplyOtherEffects(); // Apply any other effects that are not part of the main effect

		// Bool
		bool IsExpireOnTrigger { get; }

		// Properties
		int Rank { get; }
		BuffType BuffType { get; }
		BuffStackType BuffStackType { get; }
		bool BuffStackCascade { get; } // Upon Skill stack, cascade stacking to the individual effects

		// Events
		public event Action<IBuff, IBuff> OnBuffStack;

		// Methods
		public void Stack(IBuff buff);
		public void Stack(float duration, int rank, float value, IBuff buff);
		public void Tick(float t);
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

		// Meta		
		public bool IsReceipient { get; protected set; }
		public bool IsShare { get; set; }
		public bool IsExpireOnTrigger { get; set; }


		// Plan
		public new BuffPlan Plan { get; protected set; }


		// Constructor
		public Buff(BuffPlan plan) : base(null)
		{
			Plan = plan;
		}

		public Buff(BuffPlan plan, ddouble scale) : base(null, scale)
		{
			Plan = plan;
		}

		public bool EqualType(IBuff buff)
		{
			return false;
		}

		#endregion Preamble

		#region Events

		// Events
		public event Action<IExpirable> OnExpired;
		public event Action<IBuff, IBuff> OnBuffStack;

		#endregion Events

		#region Methods
		public void Expire()
		{
			// TODO
		}

		public void Init()
		{

		}

		public bool IsExpired()
		{
			return false;
		}

		public void Tick(float time)
		{

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
			this.Rank = rank;
			this.Value = value;
			OnBuffStack?.Invoke(this, buff);
		}

		public void Stack(IBuff buff)
		{
			BuffStackType.StackValue(this, (Buff)buff);
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
			float value = (float)MathsLib.Operate(A.Duration, B.Duration, A.BuffStackType.ValueOperation);
			A.Stack(duration, rank, value, B);
		}
	}
}
