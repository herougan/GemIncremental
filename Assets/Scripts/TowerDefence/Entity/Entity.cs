
using System;
using System.Collections.Generic;
using TowerDefence.Entity.Skills;
using TowerDefence.Stats;
using Util.Maths;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Resources;
using ResourceType = TowerDefence.Entity.Resources.ResourceType;
using Towerdefence.Entity.Items;
using Util.Serialisation;

namespace TowerDefence.Entity
{
	public interface IEntity : ISource
	{
		#region Information
		// ===== Meta =====
		IEntityData Plan { get; }
		SerialisableGuid Guid { get; }


		// ===== Stats =====
		StatBlock StatBlock { get; }
		ResourceBlock ResourceBlock { get; }
		ElementBlock ElementBlock { get; }
		#endregion Information

		#region State


		// ===== Game State =====
		List<IBuff> Buffs { get; }
		List<StatusStat> Statuses { get; }
		List<IAffect> Affects { get; }
		public EntityBehaviourType Behaviour { get; }

		#endregion State

		#region Abilities

		// ===== Abilities =====
		List<IBuff> TowerAuras { get; } // Active, tracked during game, influences towers within range.
		List<IBuff> MonsterAuras { get; } // Active, tracked during game, influences monsters within range.
		List<IBuff> AuraInstances { get; } // Added/Removed when In/out Aura source range
		List<SkillData> Skills { get; } // Starting skills
		List<SkillData> InitBuffs { get; } // On init, create and receive buffs based on InitBuff


		#endregion Abilities

		#region Events

		// ===== Event =====

		// == Stat ==
		// General
		public event Action<IEntity, IStat, ddouble> OnValueChanged;
		public event Action<IEntity, IStat, ddouble> OnValueDecreased;
		public event Action<IEntity, IStat, ddouble> OnValueIncreased;
		// Depletable
		public event Action<IEntity, IStat, ddouble> OnMaxValueChanged;
		public event Action<IEntity, IStat, ddouble> OnCurrentValueDecreased;
		public event Action<IEntity, IStat, ddouble> OnCurrentValueIncreased;
		// Bonus
		public event Action<IEntity, IStat, IStatMod> OnStatBonusAdded;
		public event Action<IEntity, IStat, IStatMod> OnStatNerfAdded;
		// Regenerable
		public event Action<IEntity, IStat, ddouble> OnRegenerate;
		public event Action<IEntity, IStat, ddouble> OnRegenValueChanged;
		public event Action<IEntity, IStat, ddouble> OnRegenRateChanged;
		// Element
		public event Action<IEntity, IElement, ddouble> OnResistChanged;
		public event Action<IEntity, IElement, ddouble> OnMasteryChanged;

		// = State =
		// Generic 
		public event Action<IEntity, IExpirable> OnBuffApplied;
		public event Action<IEntity, IExpirable> OnDebuffApplied;
		public event Action<IEntity, IExpirable> OnBuffExpire;
		public event Action<IEntity, IExpirable> OnDebuffExpire;

		// Game Space
		public event Action<IEntity> OnReached;
		public event Action<IEntity> OnSpawn;
		public event Action<IEntity, ISource> OnEnteredRange; // From, To
		public event Action<IEntity, ISource> OnExitRange; // From, To
		public event Action<IEntity, ISource> OnEnteredAttackRange;// From (this), To
		public event Action<IEntity, ISource> OnExitAttackRange; // From, To

		// Combat
		public event Action<IEntity, IEntity> OnHit;
		public event Action<IEntity, IEntity> OnDOT;
		public event Action<IEntity> OnDeath; // If Health <= 0
		public event Action<IEntity, ISource> OnHurt;
		public event Action<IEntity, ISource> OnHeal; // If Health > 0

		// // Token
		// public void RegisterTokenCallbacks();
		// public void RegisterTokenCallback(Token token);
		// public Action<IEntity, Token> OnTokenChange { get; }

		// // Resources
		// public void RegisterResourcesCallback();
		// public void RegisterResourceCallback(ResourceStat resourceStat);
		// public Action<IEntity, ResourceStat> OnResourceChange { get; }

		// Stat

		// Skills
		// Register Callbacks()
		void RegisterCallbacks();
		public void RegisterSkillTrigger(ISkill Skill);

		#endregion Events

		#region Methods
		// ===== Methods =====
		public IStat GetStat(StatType type);
		public float GetKinematics(KinematicsType type);
		public float GetMileage(MileageType type);
		public int GetResource(ResourceType type);
		public int GetCounter(CounterType type);
		public int GetInventory(ItemType type);

		// Buff Manipulation
		public void AddBuff(IBuff buff);
		public void RemoveBuff(IBuff buff);
		public void ApplyDamage(ddouble damager);
		public void TickExpirables(float time);
		public void Scale(List<StatType> types, List<double> values);

		// Stat Manipulation
		/// <summary>
		/// Lowers health by effective value.
		/// </summary>
		/// <param name="value"></param>
		public void Hurt(ddouble value);

		// Status
		public bool ContainStatus(StatusType statusType);
		StatusStat GetStatus(StatusType statusType);

		// Tags
		public void AddTag(Tag tag);
		public void RemoveTag(Tag tag);
		public bool HasTag(Tag tag);

		// Meta
		public bool CheckMeta(MetaType metaType, string data);

		#endregion Methods
	}

	// ================================= //
	public class Entity : IEntity
	{
		#region Information
		// Meta
		public IEntityData Plan { get; protected set; }
		public SerialisableGuid Guid { get; protected set; }

		// Stats
		public StatBlock StatBlock { get; protected set; }
		public ResourceBlock ResourceBlock { get; protected set; }
		public ElementBlock ElementBlock { get; protected set; }

		#endregion Information

		#region State

		// ===== Game State =====
		public List<IBuff> Buffs { get; protected set; } // Active, tracked during game
		public List<Tag> Tags { get; protected set; } // Can be gained and lost during a round
		public List<StatusStat> Statuses { get; protected set; } // List of active status effects
		public List<IAffect> Affects { get; protected set; }
		public EntityBehaviourType Behaviour { get; protected set; } // Current Behaviour State

		#endregion State

		#region Abilities

		// ===== Skills =====
		public List<IBuff> TowerAuras { get; protected set; }// Active, tracked during game, influences towers within range.
		public List<IBuff> MonsterAuras { get; protected set; }// Active, tracked during game, influences monsters within range.
		public List<IBuff> AuraInstances { get; protected set; }// Added/Removed when In/out Aura source range
		public List<SkillData> Skills { get; protected set; }// Starting skills
		public List<SkillData> InitBuffs { get; protected set; }// On init, create and receive buffs based on InitBuff

		#endregion Abilities

		#region Events
		// ===== Events =====
		// = Stat =
		// General
		public event Action<IEntity, IStat, ddouble> OnValueChanged = delegate { };
		public event Action<IEntity, IStat, ddouble> OnValueDecreased = delegate { };
		public event Action<IEntity, IStat, ddouble> OnValueIncreased = delegate { };
		// Depletable
		public event Action<IEntity, IStat, ddouble> OnMaxValueChanged = delegate { };
		public event Action<IEntity, IStat, ddouble> OnCurrentValueDecreased = delegate { };
		public event Action<IEntity, IStat, ddouble> OnCurrentValueIncreased = delegate { };
		// Bonus
		public event Action<IEntity, IStat, IStatMod> OnStatBonusAdded = delegate { };
		public event Action<IEntity, IStat, IStatMod> OnStatNerfAdded = delegate { };
		// Regenerable
		public event Action<IEntity, IStat, ddouble> OnRegenerate = delegate { };
		public event Action<IEntity, IStat, ddouble> OnRegenValueChanged = delegate { };
		public event Action<IEntity, IStat, ddouble> OnRegenRateChanged = delegate { };
		// Element
		public event Action<IEntity, IElement, ddouble> OnResistChanged = delegate { };
		public event Action<IEntity, IElement, ddouble> OnMasteryChanged = delegate { };

		// = State =
		// Generic 
		public event Action<IEntity, IExpirable> OnBuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnBuffExpire = delegate { }; // TODO change all to Past Tense
		public event Action<IEntity, IExpirable> OnDebuffExpire = delegate { };
		public event Action<IEntity, IExpirable> OnBuffStacked = delegate { };

		// Game Space
		public event Action<IEntity> OnReached = delegate { };
		public event Action<IEntity> OnSpawn = delegate { };
		public event Action<IEntity, ISource> OnEnteredRange = delegate { }; // From, To
		public event Action<IEntity, ISource> OnExitRange = delegate { }; // From, To
		public event Action<IEntity, ISource> OnEnteredAttackRange = delegate { };// From (this), To
		public event Action<IEntity, ISource> OnExitAttackRange = delegate { }; // From, To

		// Combat
		public event Action<IEntity, IEntity> OnHit = delegate { };
		public event Action<IEntity, IEntity> OnDOT = delegate { };
		public event Action<IEntity> OnDeath = delegate { }; // If Health <= 0
		public event Action<IEntity, ISource> OnHurt = delegate { };
		public event Action<IEntity, ISource> OnHeal = delegate { }; // If Health > 0

		public void RegisterCallbacks()
		{
			RegisterStatCallbacks();
			RegisterResourceCallbacks();
			RegisterElementCallbacks();
		}
		public void RegisterStatCallbacks()
		{
			StatBlock.RegisterCallbacks();

			// = Hook statblock events to entity events =
			// General
			StatBlock.OnValueChanged += (stat, value) =>
			{
				OnValueChanged?.Invoke(this, stat, value);
			};
			StatBlock.OnValueDecreased += (stat, value) =>
			{
				OnValueDecreased?.Invoke(this, stat, value);
			};
			StatBlock.OnValueIncreased += (stat, value) =>
			{
				OnValueIncreased?.Invoke(this, stat, value);
			};
			// Depletable
			StatBlock.OnMaxValueChanged += (stat, value) =>
			{
				OnMaxValueChanged?.Invoke(this, stat, value);
			};
			StatBlock.OnCurrentValueDecreased += (stat, value) =>
			{
				OnCurrentValueDecreased?.Invoke(this, stat, value);
			};
			StatBlock.OnCurrentValueIncreased += (stat, value) =>
			{
				OnCurrentValueIncreased?.Invoke(this, stat, value);
			};
			// Bonus
			StatBlock.OnStatBonusAdded += (stat, mod) =>
			{
				OnStatBonusAdded?.Invoke(this, stat, mod);
			};
			StatBlock.OnStatNerfAdded += (stat, mod) =>
			{
				OnStatNerfAdded?.Invoke(this, stat, mod);
			};
			// Regenerable
			StatBlock.OnRegenerate += (stat, regen) =>
			{
				OnRegenerate?.Invoke(this, stat, regen);
			};
			StatBlock.OnRegenValueChanged += (stat, value) =>
			{
				OnRegenValueChanged?.Invoke(this, stat, value);
			};
			StatBlock.OnRegenRateChanged += (stat, rate) =>
			{
				OnRegenRateChanged?.Invoke(this, stat, rate);
			};
		}
		public void RegisterResourceCallbacks()
		{
			ResourceBlock.RegisterCallbacks();
		}
		public void RegisterElementCallbacks()
		{
			ElementBlock.RegisterCallbacks();
			ElementBlock.OnResistChanged += (element, value) =>
			{
				OnResistChanged?.Invoke(this, element, value);
			};
			ElementBlock.OnMasteryChanged += (element, value) =>
			{
				OnMasteryChanged?.Invoke(this, element, value);
			};
		}

		#endregion Events

		#region Methods
		public void AddBuff(IBuff buff)
		{
			// Check if Buff already exists

			// Add Buff

			// Otherwise Stack buff

			// Trigger event
		}

		public void AddTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void ApplyDamage(ddouble damage)
		{
			throw new NotImplementedException();
		}

		public void ApplyModification(Action action)
		{
			throw new NotImplementedException();
		}

		public bool CheckMeta(MetaType metaType, string data)
		{
			throw new NotImplementedException();
		}

		public bool ContainStatus(StatusType statusType)
		{
			throw new NotImplementedException();
		}

		public int GetCounter(CounterType type)
		{
			throw new NotImplementedException();
		}

		public int GetInventory(ItemType type)
		{
			throw new NotImplementedException();
		}

		public float GetKinematics(KinematicsType type)
		{
			throw new NotImplementedException();
		}

		public float GetMileage(MileageType type)
		{
			throw new NotImplementedException();
		}

		public int GetResource(ResourceType type)
		{
			throw new NotImplementedException();
		}

		public IStat GetStat(StatType type)
		{
			return StatBlock.StatMap[type];
		}

		public ddouble GetStatus(StatusType type)
		{
			throw new NotImplementedException();
		}

		public bool HasTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void Hurt(ddouble value)
		{
			throw new NotImplementedException();
		}

		public void RegisterSkillTrigger(ISkill Skill)
		{
			throw new NotImplementedException();
		}

		public void RemoveBuff(IBuff buff)
		{
			throw new NotImplementedException();
		}

		public void RemoveTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void Scale(List<StatType> types, List<double> values)
		{
			throw new NotImplementedException();
		}

		public void TickExpirables(float time)
		{
			throw new NotImplementedException();
		}

		StatusStat IEntity.GetStatus(StatusType statusType)
		{
			throw new NotImplementedException();
		}
		#endregion Methods
	}

	public interface IEntityData
	{
		UnityEditor.GUID Guid { get; }
		string Name { get; }
		StatBlock StatBlock { get; }
		ResourceBlock ResourceBlock { get; }
	}

	public enum EntityBehaviourType
	{
		HasBuff,
		HasDebuff,
		Attacking,
		Channeling,
		Fight,
	}
}