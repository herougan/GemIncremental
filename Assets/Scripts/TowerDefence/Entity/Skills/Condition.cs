using System;
using TowerDefence.Entity.Behaviour;
using TowerDefence.Entity.Items;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Token;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills
{
	#region Conditions
	// Pure data - Condition/StatCondition/etc. don't evaluate themselves. Util.Game.EntityUtil.Check
	// (a static IEntity/ICondition -> bool dispatch, pre-existing in this codebase) is the one place
	// conditions actually get evaluated - EffectController.ApplyAction and Skill.ApplyPassive
	// both route through it, so there's exactly one evaluation mechanism, not two.
	public interface ICondition
	{
		public ConditionType ConditionType { get; }
	}

	// [Serializable] on every class in this file (base and every concrete subclass) is required, not
	// decorative - Effect.Conditions/TargetConditions are `[SerializeReference] List<ICondition>`, and
	// Unity's SerializeReference checks the CONCRETE runtime type (obj.GetType()), not an inherited
	// attribute from the base class. A subclass missing its own [Serializable] silently fails to
	// round-trip through a domain reload/asset reimport - exactly the bug that made ProjectileAction's
	// Plan field vanish after a reload (see ProjectileAction.cs's own doc comment) - so every one of
	// these gets it too, since Condition has the identical shape.
	[Serializable]
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
		Token, // Specific, e.g. +1 Wild Gem token
		Counter, // Generic e.g. +1
		Status,
		EntityInventory,
		Behaviour,
		Tag,
		Meta,
		Race,
	}


	// ===== Specific condition implementations =====
	[Serializable]
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

	[Serializable]
	public class RaceCondition : Condition
	{
		public RaceCondition(Monster.MonsterType type) : base(ConditionType.Race)
		{

		}

		public RaceCondition(Tower.TowerType type) : base(ConditionType.Race)
		{

		}
	}

	[Serializable]
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

	[Serializable]
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

	[Serializable]
	public class ResourceCondition : Condition
	{
		public ResourceType ResourceType { get; private set; }
		public ddouble RequiredAmount { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Resource;

		public ResourceCondition(ResourceType resourceType, ddouble requiredAmount, MathOperation comparative) : base(ConditionType.Resource)
		{
			ResourceType = resourceType;
			RequiredAmount = requiredAmount;
			Comparative = comparative;
		}
	}

	[Serializable]
	public class TokenCondition : Condition
	{
		public TokenType TokenType { get; private set; }
		public int RequiredCount { get; private set; }
		public MathOperation Comparative { get; private set; }
		public override ConditionType ConditionType => ConditionType.Token;

		public TokenCondition(TokenType tokenType, int requiredCount, MathOperation comparative) : base(ConditionType.Token)
		{
			TokenType = tokenType;
			RequiredCount = requiredCount;
			Comparative = comparative;
		}
	}

	[Serializable]
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

	[Serializable]
	public class EntityBehaviourCondition : Condition
	{
		public EntityBehaviourType BehaviourType { get; private set; }
		public override ConditionType ConditionType => ConditionType.Behaviour;

		public EntityBehaviourCondition(EntityBehaviourType behaviourType) : base(ConditionType.Behaviour)
		{
			BehaviourType = behaviourType;
		}
	}

	[Serializable]
	public class StatusCondition : Condition
	{
		public StatusType StatusType { get; private set; }
		public override ConditionType ConditionType => ConditionType.Status;

		public StatusCondition(StatusType statusType) : base(ConditionType.Status)
		{
			StatusType = statusType;
		}
	}

	/// <summary>Generic "+1"-style per-Entity counter (ConditionType.Counter) - see Entity.GetCounter/SetCounter and CounterType for what's actually tracked. Distinct from Mileage (which only ever accumulates, per-type, never author-set) - a Counter can be set to an arbitrary value directly, e.g. SpawnActionHandler stamping CounterType.SplitDepth onto a freshly spawned Monster.</summary>
	[Serializable]
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

	[Serializable]
	public class TagCondition : Condition
	{
		public Tag Tag { get; private set; }
		public override ConditionType ConditionType => ConditionType.Tag;

		public TagCondition(Tag tag) : base(ConditionType.Tag)
		{
			Tag = tag;
		}
	}

	[Serializable]
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
		// How many times a "splits into mini versions of itself on death" chain has already produced
		// this Entity - see SpawnAction.SpawnSelf/SpawnActionHandler. 0 for anything not spawned that way.
		SplitDepth,
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
