namespace TowerDefence.Stats
{
	using System;
	using System.Collections.Generic;
	using TowerDefence.Entity;
	using Util.Maths;

	/// <summary>
	/// Mileage represents the distance traveled by an entity.
	/// It can be used to trigger certain effects or conditions based on the distance traveled.
	/// 
	/// (Value is a float)
	/// </summary>
	public class Mileage : IStat
	{
		public MileageType Type { get; private set; }
		public float Value { get; private set; }

		public Mileage(MileageType type, float initialValue = 0)
		{
			Type = type;
			Value = initialValue;
		}

		public void Add(float amount)
		{
			Value += amount;
		}

		public void Reset()
		{
			Value = 0;
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

		public float GetMileage(MileageType type)
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