using TowerDefence.Entity.Items;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills
{
	#region Conditions
	public interface ICondition
	{
		public ConditionType ConditionType { get; }
	}

	public abstract class Condition : ICondition
	{
		// Implementation of condition logic
		public virtual ConditionType ConditionType { get; }
		public Condition(ConditionType conditionType)
		{
			ConditionType = conditionType;
		}
	}

	public enum ConditionType
	{
		Stat,
		Kinematics,
		Mileage,
		Resource,
		Counter,
		Status,
		EntityInventory,
		Behaviour,
		Tag,
		Meta,
	}


	// ===== Specific condition implementations =====
	public class StatCondition : Condition
	{
		public StatType StatType { get; private set; }
		public ddouble Value { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Stat;
		public bool CheckCurrent { get; private set; } = false; // Whether to check current value or max value (IDepletables)

		public StatCondition(StatType statType, double value, MathOperation comparative) : base(ConditionType.Stat)
		{
			StatType = statType;
			Value = value;
			Comparative = comparative;
		}
	}

	public class KinematicsCondition : Condition
	{
		public KinematicsType Param { get; private set; }
		public double Value { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Kinematics;

		public KinematicsCondition(double value, MathOperation comparative) : base(ConditionType.Kinematics)
		{
			Value = value;
			Comparative = comparative;
		}
	}

	public class MileageCondition : Condition
	{
		public MileageType MileageType { get; private set; }
		public float Mileage { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Mileage;

		public MileageCondition(MileageType mileageType, float mileage, MathOperation comparative) : base(ConditionType.Mileage)
		{
			MileageType = mileageType;
			Mileage = mileage;
			Comparative = comparative;
		}
	}

	public class ResourceCondition : Condition
	{
		public ResourceType ResourceType { get; private set; }
		public double RequiredAmount { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Resource;

		public ResourceCondition(ResourceType resourceType, double requiredAmount, MathOperation comparative) : base(ConditionType.Resource)
		{
			ResourceType = resourceType;
			RequiredAmount = requiredAmount;
			Comparative = comparative;
		}
	}

	public class CounterCondition : Condition
	{
		public CounterType CounterType { get; private set; }
		public int RequiredCount { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Counter;

		public CounterCondition(CounterType counterType, int requiredCount, MathOperation comparative) : base(ConditionType.Counter)
		{
			CounterType = counterType;
			RequiredCount = requiredCount;
			Comparative = comparative;
		}
	}

	public class EntityInventoryCondition : Condition
	{
		public ItemType ItemType { get; private set; }
		public int RequiredCount { get; private set; }
		public bool RequireNoItems { get; private set; } // Whether the entity is holding the item
		public bool RequireHoldingItems { get; private set; } // Whether the entity is not holding the item
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.EntityInventory;

		public EntityInventoryCondition(ItemType itemType, int requiredCount, MathOperation comparative, bool requireNoItems = false, bool requireHoldingItems = false) : base(ConditionType.EntityInventory)
		{
			ItemType = itemType;
			RequiredCount = requiredCount;
			Comparative = comparative;
			RequireNoItems = requireNoItems;
			RequireHoldingItems = requireHoldingItems;
		}

		public EntityInventoryCondition(bool requireNoItems = false, bool requireHoldingItems = false) : base(ConditionType.EntityInventory)
		{
			ItemType = ItemType.DefaultBerry; // Default value, can be set later
			RequiredCount = -1; // Default value, can be set later
			Comparative = MathOperation.Equal; // Default operation, can be set later

			RequireNoItems = requireNoItems;
			RequireHoldingItems = requireHoldingItems;
		}
	}

	public class EntityBehaviourCondition : Condition
	{
		public EntityBehaviourType BehaviourType { get; private set; }
		public override ConditionType ConditionType => ConditionType.Behaviour;

		public EntityBehaviourCondition(EntityBehaviourType behaviourType) : base(ConditionType.Behaviour)
		{
			BehaviourType = behaviourType;
		}
	}

	public class StatusCondition : Condition
	{
		public StatusType StatusType { get; private set; }
		public override ConditionType ConditionType => ConditionType.Status;

		public StatusCondition(StatusType statusType) : base(ConditionType.Status)
		{
			StatusType = statusType;
		}
	}

	public class TagCondition : Condition
	{
		public Tag Tag { get; private set; }
		public override ConditionType ConditionType => ConditionType.Tag;

		public TagCondition(Tag tag) : base(ConditionType.Tag)
		{
			Tag = tag;
		}
	}

	public class MetaCondition : Condition
	{
		public string Text { get; private set; }
		public MetaType MetaType { get; private set; }
		public override ConditionType ConditionType => ConditionType.Meta;

		public MetaCondition(MetaType metaType, string text) : base(ConditionType.Meta)
		{
			MetaType = metaType;
			Text = text;
		}
	}

	#endregion Conditions

	#region Enums
	public enum CounterType
	{

	}
	public enum EntityInventoryType
	{

	}
	public enum TagType
	{

	}
	public enum MetaType
	{

	}

	#endregion Enums
}