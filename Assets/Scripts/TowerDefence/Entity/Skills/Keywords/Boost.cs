using TowerDefence.Stats;
using Util.Maths;
using TowerDefence.Entity.Tower;
using TowerDefence.Entity.Monster;

namespace TowerDefence.Entity.Skills.Keywords
{

	// Note to all:
	// Boosts might not be required if, simply, they refer to things such as Spider's Bane (1.5x damage to spiders).
	// In the case, a handler might be uneccessary
	public interface IBoost
	{
		public Condition Condition { get; }
		public BoostType BoostType { get; }
		public ddouble Value { get; }
		public MathOperation Operator { get; }
	}

	/// <summary>
	/// Changes an actions properties based on certain conditions. Can be thought of as specialised elemental weaknesses.
	/// E.g. If enemy has some property, deal 1.5x damage instead.
	/// However, once the projectile is spawned, properties based on the source are fixed.
	/// The condition is checked against the target entity.
	/// 
	/// By default, parameter boosts a <Action>'s <Value> property. Depends on <BoostType>.
	/// </summary>
	public class Boost : IBoost
	{
		public BoostType BoostType { get; private set; }

		public ddouble Value { get; private set; }

		public MathOperation Operator { get; private set; }

		public Condition Condition { get; private set; }

		public Boost(BoostType boostType, ddouble value, MathOperation op, Condition condition)
		{
			BoostType = boostType;
			Value = value;
			Operator = op;
			Condition = condition;
		}
	}

	public class BoostBane : Boost
	{
		// public new BoostType BoostType { get; private set; } = BoostType.Bane;

		// public new MathOperation Operator { get; private set; } = MathOperation.Multiply;
		public BoostBane(ddouble value, Monster.MonsterType monsterType) : base(BoostType.Bane, value, MathOperation.Multiply, new RaceCondition(monsterType))
		{
			throw new System.NotImplementedException();
		}

		public BoostBane(ddouble value, Tower.TowerType towerType) : base(BoostType.Bane, value, MathOperation.Multiply, new RaceCondition(towerType))
		{
			throw new System.NotImplementedException();
		}
	}

	public class BiasedBoost : Boost
	{

		public ddouble Bias { get; set; }

		public MathOperation BiasOperation { get; set; }

		public BiasedBoost(BoostType boostType, ddouble value, MathOperation mathOperation, Condition condition, ddouble bias, MathOperation biasOperation)
		: base(boostType, value, mathOperation, condition)
		{
			Bias = bias;
			BiasOperation = biasOperation;
		}
	}

	/// <summary>
	/// Boost value changes based on stat of entity
	/// </summary>
	public class StatDynamicBoost : BiasedBoost
	{
		/// <summary>
		/// StatType determines which stat determines the value.
		/// </summary>
		public StatType StatType { get; set; }

		/// <summary>
		/// Entity that the value is dependent on.
		/// </summary>
		public IEntity DependencyEntity { get; set; }

		public new ddouble Value
		{
			get { return DependencyEntity.GetStat(StatType).Value + Bias; }
		}

		public void SetBias(ddouble bias, MathOperation biasOperation)
		{
			Bias = bias;
			BiasOperation = biasOperation;
		}

		public StatDynamicBoost(StatType statType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation)
		: base(BoostType.Value, 0, 0, null, bias, biasOperation)
		{
			StatType = statType;
			DependencyEntity = dependencyEntity;
		}

		public StatDynamicBoost(StatType statType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation, Condition condition)
		: base(BoostType.Value, 0, 0, condition, bias, biasOperation)
		{
			StatType = statType;
			DependencyEntity = dependencyEntity;
		}

		public StatDynamicBoost(StatType statType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation, Condition condition, MathOperation mathOperation)
		: base(BoostType.Value, 0, mathOperation, condition, bias, biasOperation)
		{
			StatType = statType;
			DependencyEntity = dependencyEntity;
		}
	}

	public class KinematicDynamicBoost : BiasedBoost
	{

		/// <summary>
		/// StatType determines which stat determines the value.
		/// </summary>
		public KinematicsType KinematicsType { get; set; }

		/// <summary>
		/// Entity that the value is dependent on.
		/// </summary>
		public IEntity DependencyEntity { get; set; }

		public new ddouble Value
		{
			get { return DependencyEntity.GetKinematics(KinematicsType); }
		}

		public KinematicDynamicBoost(KinematicsType kinematicsType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation)
		: base(BoostType.Value, 0, 0, null, bias, biasOperation)
		{
			KinematicsType = kinematicsType;
			DependencyEntity = dependencyEntity;
		}

		public KinematicDynamicBoost(KinematicsType kinematicsType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation, Condition condition)
		: base(BoostType.Value, 0, 0, condition, bias, biasOperation)
		{
			KinematicsType = kinematicsType;
			DependencyEntity = dependencyEntity;
		}

		public KinematicDynamicBoost(KinematicsType kinematicsType, IEntity dependencyEntity, ddouble bias, MathOperation biasOperation, Condition condition, MathOperation mathOperation)
		: base(BoostType.Value, 0, mathOperation, condition, bias, biasOperation)
		{
			KinematicsType = kinematicsType;
			DependencyEntity = dependencyEntity;
		}
	}

	/// <summary>
	/// This enum determines what Action parameters are boosted
	/// </summary>
	public enum BoostType
	{
		// Raw
		Value,
		Time,
		Distance,

		// Classic


		// Bane & Boon
		Bane,
	}

	// Boosts boost something about an action.
	// It can be a physics, time, or damage parameter.
}