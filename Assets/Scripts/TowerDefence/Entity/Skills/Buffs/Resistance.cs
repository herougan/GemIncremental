using System;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Buffs
{
	// ===== Stat =====
	public interface IResistance : IStat
	{
		public StatusType Status { get; }
		public ddouble Resist { get; set; }
		public ddouble Mastery { get; set; }
		public ddouble Threshold { get; set; }
		public event Action<IResistance, ddouble> OnResistChanged;
		public event Action<IResistance, ddouble> OnMasteryChanged;
		public event Action<IResistance, ddouble> OnThresholdCrossed;
		public event Action<IResistance, ddouble> OnThresholdChanged;
	}

	[Serializable]
	public class Resistance : Stat, IResistance
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

		[FormerlySerializedAs("Mastery")]
		[SerializeField] private ddouble _Mastery;
		public ddouble Mastery
		{
			get { return _Mastery; }
			set
			{
				if (_Mastery != value) OnMasteryChanged?.Invoke(this, value);
				_Mastery = value;
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

		public Resistance(StatusType status, ddouble value = default(ddouble), ddouble resist = default(ddouble), ddouble threshold = default(ddouble)) : base(StatType.Status, value)
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
		public event Action<IResistance, ddouble> OnResistChanged;
		public event Action<IResistance, ddouble> OnMasteryChanged;
		public event Action<IResistance, ddouble> OnThresholdCrossed;
		public event Action<IResistance, ddouble> OnThresholdChanged;

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
}