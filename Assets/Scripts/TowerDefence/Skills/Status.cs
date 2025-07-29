using System;
using TowerDefence.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Skills
{
	public interface IStatus
	{
		StatusType Type { get; }
		string Name { get; }
		string Description { get; }
		float Duration { get; }
	}

	public class Status
	{
		public StatusType Type { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public float Duration { get; set; }
		public bool IsPositive { get; set; }
		public bool ForMonster { get; set; }
		public bool ForTower { get; set; }


		// public event Action<IStat, ddouble> OnResistChanged;
	}

	public enum StatusType
	{
		Burn,
		Paralyse,
		Confuse,
		Silence,
		Weak,
		Charm,
		Curse,
		Stun,
		Poison,
		Freeze,
		Slow,
		Blind,
	}

	// ===== Stat =====
	public interface IStatusStat : IStat
	{
		public StatusType Status { get; }
		public event Action<IStat, ddouble> OnResistChanged;
	}

	[Serializable]
	public class StatusStat : IStatusStat
	{
		// Cooky hack (see IStat)
		[FormerlySerializedAs("Status")]
		[SerializeField] private StatusType _Status;
		public StatusType Status { get { return _Status; } set { _Status = value; } }


		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;
		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnValueChanged?.Invoke(this, value);
				_Value = value;
			}
		}


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

		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;
		public StatType Type { get { return _Type; } set { _Type = StatType.Status; } }


		[FormerlySerializedAs("Dynamic")]
		[SerializeField] private ddouble _Dynamic;
		public ddouble Dynamic { get { return _Dynamic; } set { _Dynamic = value; } }

		[FormerlySerializedAs("Threshold")]
		[SerializeField] private ddouble _Threshold;
		public ddouble Threshold
		{
			get { return _Threshold; }
			set
			{
				if (_Threshold != value) OnValueChanged?.Invoke(this, value);
				_Threshold = value;
			}
		}

		public StatusStat(StatusType status, ddouble value = default(ddouble), ddouble resist = default(ddouble))
		{
			Status = status;
			Value = value;
			Resist = resist;
		}

		event Action<IStat, ddouble> IStat.OnValueChanged
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event Action<IStat, ddouble> OnValueChanged;
		public event Action<IStat, ddouble> OnResistChanged;

		public void Scale(ddouble scale)
		{
			throw new NotImplementedException();
		}

		public void Recalculate(ddouble scale)
		{
			throw new NotImplementedException();
		}
	}

}