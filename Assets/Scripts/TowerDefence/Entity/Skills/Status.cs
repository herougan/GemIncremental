using System;
using TowerDefence.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Entity.Skills
{
	// ===== Stat =====
	public interface IStatus : IStat
	{
		public StatusType Status { get; }
		public ddouble Resist { get; set; }
		public ddouble Threshold { get; set; }
		public event Action<IStat, ddouble> OnResistChanged;
		public event Action<IStat, ddouble> OnThresholdCrossed;
		public event Action<IStat, ddouble> OnThresholdChanged;
	}

	[Serializable]
	public class StatusStat : Stat, IStatus
	{
		// Cooky hack (see IStat)
		[FormerlySerializedAs("Status")]
		[SerializeField] private StatusType _Status;
		public StatusType Status { get { return _Status; } set { _Status = value; } }

		[FormerlySerializedAs("Resist")]
		[SerializeField] private ddouble _Resist;
		public ddouble Resist
		{
			get { return _Resist; }
			set
			{
				if (_Resist != value) OnResistChanged?.Invoke(this, value);
				_Resist = value;
			}
		}

		[FormerlySerializedAs("Threshold")]
		[SerializeField] private ddouble _Threshold;
		public ddouble Threshold
		{
			get { return _Threshold; }
			set
			{
				if (_Threshold != value) OnThresholdChanged?.Invoke(this, value);
				_Threshold = value;
			}
		}

		public StatusStat(StatusType status, ddouble value = default(ddouble), ddouble resist = default(ddouble), ddouble threshold = default(ddouble)) : base(StatType.Status, value)
		{
			Status = status;
			Value = value;
			Resist = resist;
			Threshold = threshold;
		}

		// event Action<IStat, ddouble> IStat.OnValueChanged
		// {
		// 	add
		// 	{
		// 		throw new NotImplementedException();
		// 	}

		// 	remove
		// 	{
		// 		throw new NotImplementedException();
		// 	}
		// }
		public event Action<IStat, ddouble> OnResistChanged;
		public event Action<IStat, ddouble> OnThresholdCrossed;
		public event Action<IStat, ddouble> OnThresholdChanged;

		// ===== Methods =====

		public void AddStatus(ddouble amount)
		{
			if (amount <= 0) return;
			Value += amount * (1 - Resist);
			//
			if (Value >= Threshold)
			{
				OnThresholdCrossed?.Invoke(this, Value);
				Reset();
			}
		}

		public void Reset()
		{
			Value = 0;
		}
	}

	public enum StatusType
	{
		Stun,
		Poison,
		Freeze,
		Slow,
		Blind,
		Knockback,
		Burn,
		Paralyse,
		Confuse,
		Silence,
		Weak,
		Charm,
		Curse,
	}
}