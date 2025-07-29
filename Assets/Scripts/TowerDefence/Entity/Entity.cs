
using System;
using System.Collections.Generic;
using TowerDefence.Expirable;
using TowerDefence.Skills;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity
{
	public interface IEntity : ISource
	{
		// ===== Main =====
		public IEntityPlan Plan { get; }


		// ===== Stats =====
		public StatBlock StatBlock { get; }
		public ResourceBlock ResourceBlock { get; }
		public IStat GetStat(StatType type);
		public float GetKinematics(KinematicsType type);
		public float GetMileage(MileageType type);
		public int GetResource(EntityResourceType type);
		public int GetCounter(CounterType type);
		public int GetInventory(ItemType type);


		// ===== Meta =====
		// public ...


		// ===== Game State =====
		List<IBuff> Buffs { get; }
		// List<ISkill> Skills { get; } // Use the Skills from the Plan, calculating scaling automatically

		public void AddBuff(IBuff buff);
		public void RemoveBuff(IBuff buff);
		public void ApplyModification(Skills.Action action);
		public void ApplyDamage(ddouble damager);
		public void TickExpirables(float time);
		public void Scale(List<StatType> types, List<double> values);
		// Behaviour State Machine
		public EntityBehaviourType Behaviour { get; }
		// public BehaviourSystem BehaviourSystem { get; set; }
		public bool ContainStatus(StatusType statusType);
		public StatusStat GetStatus(StatusType statusType);
		public List<IStatus> Statuses { get; } // List of active status effects


		// ===== Event =====
		void RegisterCallbacks();

		// === Generic ===
		public event Action<IEntity, IExpirable> OnBuffApplied;
		public event Action<IEntity, IExpirable> OnDebuffApplied;
		public event Action<IEntity, IExpirable> OnBuffExpire;
		public event Action<IEntity, IExpirable> OnDebuffExpire;
		public event Action<IEntity, IStat, double> OnStatChange; // IEntity, IStat
		public event Action<IEntity, IDepletable, double> OnStatDecreased;
		public event Action<IEntity, IRegenerable, double> OnRegenValueChanged;
		public event Action<IEntity, IRegenerable> OnRegenerate;

		// === Game Space ===
		public Action<IEntity> OnReached { get; }
		public Action<IEntity> OnSpawn { get; set; }
		public Action<IEntity, ISource> OnEnteredRange { get; set; } // From, To
		public Action<IEntity, ISource> OnExitRange { get; set; } // From, To
		public Action<IEntity, ISource> OnEnteredAttackRange { get; set; } // From (this), To
		public Action<IEntity, ISource> OnExitAttackRange { get; set; } // From, To


		// Combat
		public Action<IEntity, Tower> OnHit { get; set; }
		public Action<IEntity, Tower> OnDOT { get; set; }
		public Action<IEntity> OnDeath { get; set; } // If Health <= 0


		// Skills
		public void RegisterSkillTrigger(ISkill Skill);

		// Tags
		public List<Tag> Tags { get; }
		public void AddTag(Tag tag);
		public void RemoveTag(Tag tag);
		public bool HasTag(Tag tag);

		// Meta
		public bool CheckMeta(MetaType metaType, string data);
	}

	public class Entity : IEntity
	{
		public IPlan Plan { get; private set; }

		public StatBlock StatBlock => throw new NotImplementedException();

		public ResourceBlock ResourceBlock => throw new NotImplementedException();

		public List<IBuff> Buffs => throw new NotImplementedException();
		public List<Tag> Tags { get; private set; } = new List<Tag>();

		public EntityBehaviourType Behaviour { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Action<IEntity> OnReached => throw new NotImplementedException();

		public Action<IEntity> OnSpawn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, ISource> OnEnteredRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, ISource> OnExitRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, ISource> OnEnteredAttackRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, ISource> OnExitAttackRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, Tower> OnHit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity, Tower> OnDOT { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Action<IEntity> OnDeath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public List<IAffect> Affects => throw new NotImplementedException();

		public List<IStatus> Statuses => throw new NotImplementedException();

		IEntityPlan IEntity.Plan => throw new NotImplementedException();

		public event Action<IEntity, IExpirable> OnBuffApplied;
		public event Action<IEntity, IExpirable> OnDebuffApplied;
		public event Action<IEntity, IExpirable> OnBuffExpire;
		public event Action<IEntity, IExpirable> OnDebuffExpire;
		public event Action<IEntity, IStat, double> OnStatChange;
		public event Action<IEntity, IDepletable, double> OnStatDecreased;
		public event Action<IEntity, IRegenerable, double> OnRegenValueChanged;
		public event Action<IEntity, IRegenerable> OnRegenerate;

		public void AddBuff(IBuff buff)
		{
			throw new NotImplementedException();
		}

		public void AddTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void ApplyDamage(ddouble damage)
		{
			throw new NotImplementedException();
		}

		public void ApplyModification(Skills.Action action)
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

		public int GetResource(EntityResourceType type)
		{
			throw new NotImplementedException();
		}

		public IStat GetStat(StatType type)
		{
			throw new NotImplementedException();
		}

		public ddouble GetStatus(StatusType type)
		{
			throw new NotImplementedException();
		}

		public bool HasTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void RegisterCallbacks()
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
	}

	public interface IEntityResource
	{
		public EntityResourceType resourceType { get; }
	}

	public interface IEntityController
	{
		// ===== Game State =====
		void ApplyBuff(Tower source, IBuff buff);
		void ApplyBuff(Monster monster, IBuff buff);

		// ===== Event =====
		void RegisterCallbacks();
		void Died();
		void Buffed(IBuff buff);
		void Debuffed(IBuff duff);
		void Hit(Tower tower);
		void Reached();
		void HealthDecreased();
		void StatChanged(IStat stat, double value);
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