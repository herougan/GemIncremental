using System;
using System.Collections.Generic;
using TowerDefence.Calculation;
using TowerDefence.Entity;
using TowerDefence.Expirable;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Skills
{
	public interface IBuff : IExpirable, ISkill // giving IBuff these interfaces forces the abstract object to have effect when the data ALREADY has effect.
	{
		// Functions
		public bool EqualType(IBuff buff);
		public void ApplyOtherEffects(); // Apply any other effects that are not part of the main effect

		// Bool
		public bool IsExpireOnTrigger { get; }

		// Properties
		public BuffStackType BuffStackType { get; }
		public BuffType BuffType { get; }
		public BuffPlan Data { get; } // Data of the buff, e.g. BuffPlan
	}

	public enum BuffType
	{
		StatBuff,
		StatusBuff,
		Aura,
		Saga,
	}


	[Serializable]
	public class Buff : IBuff
	{
		#region Preamble
		// Rank Properties
		public int Rank { get; private set; } // Rank of the buff, e.g. 1, 2, 3, etc.
		public BuffType BuffType { get; private set; }
		public BuffStackType BuffStackType { get; private set; }

		// Properties
		public float Duration { get; private set; }
		public float Time { get; private set; }
		public ddouble Value { get; private set; }
		public ddouble Dynamic { get; private set; }

		// Meta		
		public bool IsPositive { get; private set; }
		public bool ForMonster { get; private set; }
		public bool ForTower { get; private set; }
		public bool IsReceipient { get; private set; }
		public bool IsShare { get; set; }
		public bool IsExpireOnTrigger { get; set; }
		public ISource Source { get; private set; }
		public IEntity Affected { get; private set; }


		// Data
		public BuffPlan Data { get; private set; }

		public ddouble Scale => throw new NotImplementedException();


		// Events
		public event Action<IExpirable> OnExpired;
		public event Action<ISource> OnApplied;

		// Constructor
		public Buff(BuffPlan data)
		{
			Data = data;
		}

		public bool EqualType(IBuff buff)
		{
			return false;
		}

		#endregion Preamble

		#region Methods
		public void Expire()
		{

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
			foreach (Effect effect in Effects)
			{
				effect.Scale(scale);
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
			Source = source;
			Affected = target;
		}

		public void ApplyEffect(IEntity source, IEntity target)
		{
			foreach (SkillEffect effect in Data.Effects)
			{
				if (EntityUtil.Check(source, effect.Conditions))
				{
					effect.ApplyEffect(source, target);
				}
			}
		}
		#endregion Methods
	}

	public class StatBuff : Buff
	{
		public StatBuff(BuffPlan data) : base(data)
		{
		}

		public StatType Type { get; set; }

		public List<IModification> Modifications { get; set; }

		public bool EqualType(StatBuff buff)
		{
			return Type == buff.Type;
		}
	}

	public class StatusBuff : Buff
	{
		public StatusBuff(BuffPlan data) : base(data)
		{
		}

		public StatusType Type { get; set; }

		public List<IModification> Modifications { get; set; }

		public bool EqualType(StatusBuff buff)
		{
			return Type == buff.Type;
		}
	}


	public class BuffBundle
	{
		public List<Buff> buffs;
	}

	// ====== Enum ======
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

		public static void StackValue(Buff A, Buff B)
		{
			if (A == null || B == null)
				return;
			if (A.BuffType != B.BuffType)
				return;
			A.Duration = (float)MathsLib.Operate(A.Duration, B.Duration, A.BuffStackType.TimeOperation);
			A.Rank = (int)MathsLib.Operate(A.Rank, B.Rank, A.BuffStackType.RankOperation);
			// foreach (SkillEffect effect in A.Effects)
			// {
			// 	effect.Scale(B.Data.Scale);
			// }
			A.Value = (float)MathsLib.Operate(A.Duration, B.Duration, A.BuffStackType.ValueOperation);
		}
	}
}
