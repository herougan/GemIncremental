namespace TowerDefence.Stats
{
	using System;
	using System.Collections.Generic;
	using Util.Maths;

	public interface IMileage : IStat
	{
		public MileageType Type { get; }
		public ddouble Threshold { get; }
		void Add(ddouble amount);
		void Reset();

		// Events
		public event Action<IMileage, ddouble> OnThresholdCrossed;
		public event Action<IMileage, ddouble> OnMileageReset;
	}

	/// <summary>
	/// Mileage represents the distance traveled by an entity.
	/// It can be used to trigger certain effects or conditions based on the distance traveled.
	/// 
	/// (Value is a float)
	/// </summary>
	public class Mileage : Stat, IMileage
	{
		public MileageType Type { get; private set; }
		public float Accum { get; private set; }
		public ddouble Threshold { get; private set; }
		//
		public event Action<IMileage, ddouble> OnThresholdCrossed;
		public event Action<IMileage, ddouble> OnMileageReset;

		public Mileage(MileageType type, float initialValue = 0) : base(StatType.Mileage, initialValue)
		{
			Type = type;
			Value = initialValue;
		}

		public void Reset()
		{
			Value = 0;
			OnMileageReset?.Invoke(this, Value);
		}

		public void Add(ddouble amount)
		{
			Value += amount;
			if (Value >= Threshold)
			{
				OnThresholdCrossed?.Invoke(this, Threshold);
			}
		}
	}

	public enum MileageType
	{
		DistanceTraveled, // Total distance traveled by the entity
		DistanceToTarget, // Distance to the current target
		TargetsKilled, // Number of targets killed
		SkillsUsed, // Number of skills used
	}

	#region Blocks

	public class MileageBlock
	{
		public Dictionary<MileageType, Mileage> MileageStats { get; private set; }

		public MileageBlock()
		{
			MileageStats = new Dictionary<MileageType, Mileage>();
			foreach (MileageType type in Enum.GetValues(typeof(MileageType)))
			{
				MileageStats[type] = new Mileage(type);
			}
		}

		public void AddMileage(MileageType type, float amount)
		{
			if (MileageStats.ContainsKey(type))
			{
				MileageStats[type].Add(amount);
			}
			else
			{
				throw new ArgumentException($"Mileage type {type} does not exist.");
			}
		}

		public ddouble GetMileage(MileageType type)
		{
			return MileageStats.ContainsKey(type) ? MileageStats[type].Value : 0;
		}

		public void ResetMileage(MileageType type)
		{
			if (MileageStats.ContainsKey(type))
			{
				MileageStats[type].Reset();
			}
			else
			{
				throw new ArgumentException($"Mileage type {type} does not exist.");
			}
		}
	}

	#endregion Blocks
}