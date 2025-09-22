
using System;
using System.Collections.Generic;
using TowerDefence.Stats;
using Util.Maths;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Items;
using Util.Serialisation;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Entity.Token;
using System.Linq;
using Debug;

namespace TowerDefence.Entity
{
	public interface IEntity
	{
		#region Information
		// ===== Meta =====
		IEntityPlan Plan { get; }
		SerialisableGuid Guid { get; }


		// ===== Stats =====
		StatBlock StatBlock { get; }
		ResourceBlock ResourceBlock { get; }
		ElementBlock ElementBlock { get; }
		#endregion Information

		#region State


		// ===== Game State =====
		List<Tag> Tags { get; }
		EntityBehaviourType Behaviour { get; }
		// TokenInventory TokenInventory { get; } // Use tokens
		MileageBlock MileageBlock { get; }
		TokenInventory TokenInventory { get; }
		Kinematics Kinematics { get; }

		#endregion State

		#region Abilities

		// ===== Abilities =====
		// Starting abilities
		List<IBuff> Buffs { get; }
		List<SkillPlan> Skills { get; } // Starting skills
		List<StatusBuff> Statuses { get; }
		List<IBuff> TowerAuras { get; } // Active, tracked during game, influences towers within range.
		List<IBuff> MonsterAuras { get; } // Active, tracked during game, influences monsters within range.
		List<IBuff> AuraInstances { get; } // Added/Removed when In/out Aura source range

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
		public event Action<IEntity, IExpirable> OnBuffExpired;
		public event Action<IEntity, IExpirable> OnDebuffExpired;
		public event Action<IEntity, IExpirable> OnBuffStacked;
		// TODO why do I need to track a damage's source? damage is from Fire Breath DOT
		// WHAT IF: On hurt, hurt entity doing the damage?
		// OR: (Likely not) Nerf the caster's Fire Breath damage DOT only (affects all)
		// Let's you track damagers I guess - IExpirable and IDamage can have ISource with can have IEntity
		// 1. DOT
		// 2. Attack
		// 3. Spell
		// 4. AoE

		// Fusion Event
		// public event Action<IEntity, IExpirable, IExpirable, IEffectInteraction> OnBuffFused;

		// Game Space
		public event Action<IEntity> OnReached;
		public event Action<IEntity> OnSpawn;
		public event Action<IEntity> OnPushBack;
		public event Action<IEntity> OnHidden;

		// Behaviour
		public event Action<IEntity> OnFirstHurt;
		public event Action<IEntity> OnFirstBuff;
		public event Action<IEntity> OnTargetted;
		public event Action<IEntity> OnUntargetted;
		// public event Action<IEntity> OnMeditateFinish; // Skills should settle this
		public event Action<IEntity> OnSpellCast;

		// Allies
		public event Action<IEntity, IEntity> OnAllySpawn;
		public event Action<IEntity, IEntity> OnAllyDeath;
		public event Action<IEntity, IEntity> OnAllyHurt;
		public event Action<IEntity, IEntity> OnAllyHeal;
		public event Action<IEntity, IEntity> OnAllyBuff;
		public event Action<IEntity, IEntity, IExpirable> OnAllyStatus;

		// = Range =
		// On Ally/On Enemy/On Tagged - Particular treatment handled within e.g. OnIsolation from "Piano" enemies
		public event Action<IEntity, IEntity> OnEnteredRange; // From, To
		public event Action<IEntity, IEntity> OnExitRange; // From, To
		public event Action<IEntity, IEntity> OnEnteredAttackRange;// From (this), To
		public event Action<IEntity, IEntity> OnExitAttackRange; // From, To
		public event Action<IEntity, IEntity> OnIsolated; // Leaving
		public event Action<IEntity, IEntity> OnNotIsolated; // Entering

		// Combat
		public event Action<IEntity, IEntity> OnHit; // On normal attack by enemy (Typically tower-monster)
		public event Action<IEntity, ISource> OnDOT; // On Damage over time tick (Skill/Expirable)
		public event Action<IEntity> OnDeath; // If Health <= 0
		public event Action<IEntity, ISource> OnHurt;
		public event Action<IEntity, ISource> OnHeal; // If Health > 0
		public event Action<IEntity, ISource> OnVamp;
		public event Action<IEntity, IStat> OnBloodied;
		public event Action<IEntity, IStat> OnDying;
		public event Action<IEntity, IStat> OnRage;
		public event Action<IEntity, IStat> OnManaDry;
		public event Action<IEntity> OnRest;

		// Register Callbacks()
		void RegisterCallbacks();
		void RegisterStatCallbacks();
		void RegisterResourceCallbacks();
		void RegisterTokenCallbacks();
		void RegisterElementCallbacks();

		#endregion Events

		#region Lifecycle
		// ===== Lifecycle =====
		public void Spawn();
		public void Regenerate();
		public void Rest();
		public void Tick(float t);
		public void Die();
		public void Attack(); // move attack timer

		#endregion Lifecycle

		#region Methods

		// ===== Methods =====
		// Getters
		public IStat GetStat(StatType type);
		public float GetKinematics(KinematicsType type);
		public float GetMileage(MileageType type);
		public int GetResource(ResourceType type);
		public int GetCounter(CounterType type);
		public int GetInventory(ItemType type);

		// Buff Manipulation
		public void ApplyBuff(IBuff buff);
		public void CleanseBuff(IBuff buff);
		public void CleanseRandom();
		public void ApplyDamage(ddouble damage);
		public void Recalculate(List<StatType> types, List<double> values);

		// Entity Ability method
		public void RegisterSkillTrigger(ISkill Skill);

		// Stat Manipulation
		/// <summary>
		/// Lowers health by effective value.
		/// </summary>
		/// <param name="value"></param>
		public void Hurt(ddouble value);
		public void ApplyDamage(Damage damage);

		// Status
		public bool ContainStatus(StatusType statusType);
		Resistance GetStatus(StatusType statusType);

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
		public IEntityPlan Plan { get; protected set; }
		public SerialisableGuid Guid { get; protected set; }

		// Stats
		public StatBlock StatBlock { get; protected set; }
		public ResourceBlock ResourceBlock { get; protected set; }
		public ElementBlock ElementBlock { get; protected set; }

		#endregion Information

		#region State

		// ===== Game State =====
		public List<Tag> Tags { get; protected set; } // Can be gained and lost during a round
		public EntityBehaviourType Behaviour { get; protected set; } // Current Behaviour State
		public MileageBlock MileageBlock { get; protected set; }
		public TokenInventory TokenInventory { get; }
		public Kinematics Kinematics { get; }
		public CountdownTimer AttackTimer { get; protected set; }

		#endregion State

		#region Abilities

		// ===== Skills =====
		// Abilities
		public List<IBuff> Buffs { get; protected set; } // Active, tracked during game
		public List<SkillPlan> Skills { get; protected set; }// Starting skills
		public List<StatusBuff> Statuses { get; protected set; } // List of active status effects
		public List<IBuff> TowerAuras { get; protected set; }// Active, tracked during game, influences towers within range.
		public List<IBuff> MonsterAuras { get; protected set; }// Active, tracked during game, influences monsters within range.
		public List<IBuff> AuraInstances { get; protected set; }// Added/Removed when In/out Aura source range

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
		public event Action<IEntity, IExpirable> OnBuffExpired = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffExpired = delegate { };
		public event Action<IEntity, IExpirable> OnBuffStacked = delegate { };

		// Game Space
		public event Action<IEntity> OnReached = delegate { };
		public event Action<IEntity> OnSpawn = delegate { };
		public event Action<IEntity> OnPushBack = delegate { };
		public event Action<IEntity> OnHidden = delegate { };

		// Behaviour
		public event Action<IEntity> OnFirstHurt = delegate { };
		public event Action<IEntity> OnFirstBuff = delegate { };
		public event Action<IEntity> OnTargetted = delegate { };
		public event Action<IEntity> OnUntargetted = delegate { };
		public event Action<IEntity> OnSpellCast = delegate { };

		// Allies
		public event Action<IEntity, IEntity> OnAllySpawn = delegate { };
		public event Action<IEntity, IEntity> OnAllyDeath = delegate { };
		public event Action<IEntity, IEntity> OnAllyHurt = delegate { };
		public event Action<IEntity, IEntity> OnAllyHeal = delegate { };
		public event Action<IEntity, IEntity> OnAllyBuff = delegate { };
		public event Action<IEntity, IEntity, IExpirable> OnAllyStatus = delegate { };

		// Range
		public event Action<IEntity, IEntity> OnEnteredRange = delegate { }; // From, To
		public event Action<IEntity, IEntity> OnExitRange = delegate { }; // From, To
		public event Action<IEntity, IEntity> OnEnteredAttackRange = delegate { };// From (this), To
		public event Action<IEntity, IEntity> OnExitAttackRange = delegate { }; // From, To
		public event Action<IEntity, IEntity> OnIsolated = delegate { };
		public event Action<IEntity, IEntity> OnNotIsolated = delegate { };

		// Combat
		public event Action<IEntity, IEntity> OnHit = delegate { };
		public event Action<IEntity, ISource> OnDOT = delegate { };
		public event Action<IEntity> OnDeath = delegate { }; // If Health <= 0
		public event Action<IEntity, ISource> OnHurt = delegate { };
		public event Action<IEntity, ISource> OnHeal = delegate { }; // If Health > 0
		public event Action<IEntity, ISource> OnVamp = delegate { };
		public event Action<IEntity, IStat> OnBloodied = delegate { };
		public event Action<IEntity, IStat> OnDying = delegate { };
		public event Action<IEntity, IStat> OnRage = delegate { };
		public event Action<IEntity, IStat> OnManaDry = delegate { };
		public event Action<IEntity> OnRest = delegate { };

		// Register Callbacks
		public void RegisterCallbacks()
		{
			RegisterStatCallbacks();
			RegisterResourceCallbacks();
			RegisterElementCallbacks();
			RegisterTokenCallbacks();
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
		public void RegisterTokenCallbacks()
		{
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

		// Register/Deregister Single
		public void RegisterStatCallback()
		{

		}
		public void DeregisterStatCallback()
		{

		}
		public void RegisterResourceCallback()
		{

		}
		public void DeregisterResourceCallback()
		{

		}
		public void RegisterElementCallback()
		{

		}
		public void DeregisterElementCallback()
		{

		}
		public void RegisterTokenCallback()
		{

		}
		public void DeregisterTokenCallback()
		{

		}

		#endregion Events

		#region Lifecycle
		// ===== Lifecycle =====
		public void Spawn()
		{
			// Apply InitBuffs
			foreach (var buffPlan in Plan.InitBuffs)
			{
				ApplyBuff(new Buff(buffPlan));
			}
			OnSpawn?.Invoke(this);

			// Get starting tokens
			foreach (var token in Plan.StartingTokens)
			{
				TokenInventory.Add(token);
			}

			// Get Skills based on EntityPlan
			foreach (var skillPlan in Plan.Skills)
			{
				Skills.Add(skillPlan);
			}
		}

		public void Regenerate()
		{
			foreach (var stat in StatBlock.StatMap.Values)
			{
				if (stat is IRegenerable regStat)
				{
					regStat.Regenerate();
				}
			}
		}

		public void Rest()
		{
			foreach (var stat in StatBlock.StatMap.Values)
			{
				if (stat is IRegenerable regStat)
				{
					regStat.Rest();
				}
			}
			OnRest?.Invoke(this);
		}

		public void Tick(float t)
		{
			StatBlock.Tick(t);
			MileageBlock.Tick(t);
		}

		public void Die()
		{
			OnDeath?.Invoke(this);
			// GameManager.Instance.EntityDied(this);
		}

		public void Attack() { }
		#endregion Lifecycle

		#region Methods

		// ===== Methods ===== 
		// Getters
		public IStat GetStat(StatType type)
		{
			return StatBlock.StatMap[type];
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

		public int GetCounter(CounterType type)
		{
			throw new NotImplementedException();
		}

		public int GetInventory(ItemType type)
		{
			throw new NotImplementedException();
		}


		// Buff Manipulation


		// Entity Ability method
		public void ApplyBuff(IBuff buff)
		{
			// Check if buff of same type exists
			if (Buffs.Any(b => b.BuffType == buff.BuffType))
			{
				// Stack buff
				var existing = Buffs.First(b => b.BuffType == buff.BuffType);
				existing.Stack(buff);
				OnBuffStacked?.Invoke(this, existing);
			}
			else
			{
				// Add buff
				Buffs.Add(buff);
				OnBuffApplied?.Invoke(this, buff);
			}
		}

		public void CleanseBuff(IBuff buff)
		{
			if (Buffs.Any(b => b.BuffType == buff.BuffType))
			{
				// Remove
				var existing = Buffs.First(b => b.BuffType == buff.BuffType);
				Buffs.Remove(existing);
				OnBuffExpired?.Invoke(this, existing);
			}
			else
			{
				// Do nothing
				LogManager.Instance.LogWarning($"Trying to remove non-existing buff {buff.BuffType} from entity {this.Guid}");
			}
		}

		public void CleanseRandom()
		{
			if (Buffs.Count > 0)
			{
				var randomIndex = UnityEngine.Random.Range(0, Buffs.Count);
				var buffToRemove = Buffs[randomIndex];
				CleanseBuff(buffToRemove);
			}
		}

		public void ApplyDamage(Damage damage)
		{
			StatBlock.Health.Deplete(damage.Value);
		}

		public void Recalculate(List<StatType> types, List<double> values)
		{
			throw new NotImplementedException();
		}

		// Status
		public Resistance GetStatus(StatusType statusType)
		{
			Resistance r = StatBlock.Resistances.Find(s => s.Status == statusType);
			if (r == null) LogManager.Instance.LogWarning($"Entity {this.Guid} does not have status {statusType}");
			return r;
		}

		// Tags
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

		public void RemoveTag(Tag tag)
		{
			throw new NotImplementedException();
		}
		#endregion Methods

		#region Dipose

		#endregion Dispose

		#region Admin

		public void ForceSetStat(StatType type, ddouble value)
		{
			GetStat(type).SetValue(value);
		}

		#endregion Admin

	}

	public interface IPlan
	{
		SerialisableGuid Guid { get; }
		string Name { get; }
	}

	public interface IEntityPlan : IPlan
	{
		StatBlock StatBlock { get; }
		ResourceBlock ResourceBlock { get; }
		List<IToken> StartingTokens { get; }
		List<BuffPlan> InitBuffs { get; }
		List<SkillPlan> Skills { get; }
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