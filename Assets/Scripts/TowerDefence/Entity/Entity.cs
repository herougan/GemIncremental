
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
using Util.Debug;
using UnityEngine;
using TowerDefence.Entity.Skills.Effects;
using Action = TowerDefence.Entity.Skills.Effects.Action;
using TowerDefence.Context;

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
		MileageBlock MileageBlock { get; }
		TokenInventory TokenInventory { get; }
		Kinematics Kinematics { get; }

		// Current Behaviour
		Behaviour Behaviour { get; }

		#endregion State

		#region Abilities

		// ===== Abilities =====
		// Starting abilities
		List<IBuff> Buffs { get; }
		List<ISkill> Skills { get; } // Starting skills
		List<StatusBuff> Statuses { get; }
		List<IBuff> TowerAuras { get; } // Active, tracked during game, influences towers within range.
		List<IBuff> MonsterAuras { get; } // Active, tracked during game, influences monsters within range.
		List<IBuff> AuraInstances { get; } // Added/Removed when In/out Aura source range

		#endregion Abilities

		#region Events

		// == Stat ==
		// View in StatBlock

		// = State =
		// Generic 
		public event Action<IEntity, IExpirable> OnBuffApplied;
		public event Action<IEntity, IExpirable> OnDebuffApplied;
		public event Action<IEntity, IExpirable> OnBuffExpired;
		public event Action<IEntity, IExpirable> OnDebuffExpired;
		public event Action<IEntity, IExpirable> OnBuffStacked;

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
		public event Action<IEntity, IEntity> OnAttack; // Attacker, Target
		public event Action<IEntity, IEntity> OnHit; // On normal attack by enemy (Typically tower-monster)
		public event Action<IEntity, ISource> OnDOT; // On Damage over time tick (Skill/Expirable)
		public event Action<IEntity, ISource> OnDeath; // If Health <= 0
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
		void RegisterSkillCallbacks();

		// === Trigger Hooks ===

		public Action<TriggerContext, IEntity, ISkill, IEffect> GetEvent(TriggerType triggerType);

		public void SubscribeEvent(TriggerType type, Delegate _delegate);

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

		// ===== Getters =====

		// None

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

		#region Visuals

		Texture2D Texture { get; }

		#endregion Visuals
	}

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
		public MileageBlock MileageBlock { get; protected set; }
		public TokenInventory TokenInventory { get; }
		public Kinematics Kinematics { get; }

		// Behaviour
		public Behaviour Behaviour { get; protected set; }

		#endregion State

		#region Abilities

		// ===== Skills =====
		// Abilities
		public List<IBuff> Buffs { get; protected set; } // Active, tracked during game
		public List<ISkill> Skills { get; protected set; }// Starting skills
		public List<StatusBuff> Statuses { get; protected set; } // List of active status effects
		public List<IBuff> TowerAuras { get; protected set; }// Active, tracked during game, influences towers within range.
		public List<IBuff> MonsterAuras { get; protected set; }// Active, tracked during game, influences monsters within range.
		public List<IBuff> AuraInstances { get; protected set; }// Added/Removed when In/out Aura source range

		#endregion Abilities

		#region Events
		// ===== Stat =====
		// View in StatBlock

		// = State =
		// Generic 
		public event Action<IEntity, IExpirable> OnBuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnBuffExpired = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffExpired = delegate { };
		public event Action<IEntity, IExpirable> OnBuffStacked = delegate { };

		// Fusion Event
		// public event Action<IEntity, IExpirable, IExpirable, IEffectInteraction> OnBuffFused = delegate { };

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
		public event Action<IEntity, IEntity> OnAttack = delegate { };
		public event Action<IEntity, IEntity> OnHit = delegate { };
		public event Action<IEntity, ISource> OnDOT = delegate { };
		public event Action<IEntity, ISource> OnDeath = delegate { }; // If Health <= 0; Dier, Source (Killer)
		public event Action<IEntity, ISource> OnHurt = delegate { };
		public event Action<IEntity, ISource> OnHeal = delegate { }; // If Health > 0
		public event Action<IEntity, ISource> OnVamp = delegate { };
		public event Action<IEntity, IStat> OnBloodied = delegate { };
		public event Action<IEntity, IStat> OnDying = delegate { };
		public event Action<IEntity, IStat> OnRage = delegate { };
		public event Action<IEntity, IStat> OnManaDry = delegate { };
		public event Action<IEntity> OnRest = delegate { };

		// === Register Callbacks ===
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
		public void RegisterSkillCallbacks()
		{
			foreach (var skill in Skills)
			{
				RegisterSkillCallback(skill);
			}
			foreach (var buff in Buffs)
			{
				RegisterSkillCallback(buff);
			}
		}

		// === Register/Deregister Single ===
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
		public void RegisterSkillCallback(ISkill skill)
		{
			foreach (IEffect effect in skill.Plan.Effects)
			{
				RegisterEffectCallback(effect);
			}
		}
		public void RegisterEffectCallback(IEffect effect)
		{
			foreach (var trigger in effect.Triggers)
			{
				RegisterTriggerCallback(trigger);
			}
		}
		public void RegisterTriggerCallback(ITrigger trigger, ISkill skill)
		{
			switch (trigger.Type)
			{
				case TriggerType.Attack:
					OnAttack += (entity, target) =>
					{
						// ConditionUtil
						// EffectManager plays Skill (then it should be the EM that registers...)
						// I can Trigger the Skill I guess? Then the EM resolves it? Skill holds the Trigger Context
						trigger.CheckConditions(this, target);
					};
					break;
				case TriggerType.Hit:
					OnHit += (entity, target) =>
					{
						trigger.CheckConditions(this, target);
					};
					break;
				case TriggerType.Death:
					OnDeath += (entity) =>
					{
						trigger.CheckConditions(this, null);
					};
					break;
				case TriggerType.Hurt:
					OnHurt += (entity, source) =>
					{
						trigger.CheckConditions(this, null);
					};
					break;
				case TriggerType.Heal:
					OnHeal += (entity, source) =>
					{
						trigger.CheckConditions(this, null);
					};
					break;
				case TriggerType.DOT:
					OnDOT += (entity, source) =>
					{
						trigger.CheckConditions(this, null);
					};
					break;
				default:
					LogManager.Instance.LogWarning($"Trigger type {trigger.Type} not implemented in RegisterTriggerCallback");
					break;
			}
		}
		public void DeregisterSkillCallback(ISkill skill)
		{
			skill.OnSkillUsed -= EffectController.Instance.ResolveSkill;

			// Instead of finding triggers under the skill (which may have changed somehow), 
			// Detach all references to this skill
		}

		// ===== Trigger Hooks =====
		public List<Action<TriggerContext, IEntity, ISkill, IEffect>> EffectActivations = new();
		public Action<TriggerContext, IEntity, ISkill, IEffect> GetEvent(TriggerType triggerType)
		{
			switch (triggerType)
			{
				case TriggerType.Attack:
					return OnAttack;
				case TriggerType.Hit:
					return OnHit;
				case TriggerType.Death:
					return OnDeath;
				case TriggerType.Hurt:
					return OnHurt;
				case TriggerType.Heal:
					return OnHeal;
				case TriggerType.DOT:
					return OnDOT;
				default:
					LogManager.Instance.LogWarning($"Trigger type {triggerType} not implemented in GetEvent");
					return null;
			}
		}

		public void SubscribeEvent(TriggerType type, Delegate _delegate)
		{
			GetEvent(type) += () => _delegate;
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
			foreach (var skillPlan in Plan.InitSkills)
			{
				Skills.Add(new Skill(skillPlan));
			}

			// Events
			OnSpawn?.Invoke(this);
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
		}

		public void Attack()
		{
			OnAttack?.Invoke(this, null);
		}
		#endregion Lifecycle

		#region Methods

		// ===== Getters ===== 

		// None

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
			StatBlock.GetStat(type).SetValue(value);
		}

		#endregion Admin

		#region Visuals

		public Texture2D Texture { get; protected set; }

		#endregion Visuals
	}
	// ================================= //

	public interface IPlan
	{
		// Meta
		SerialisableGuid Guid { get; }
		string Name { get; }
	}

	public interface IEntityPlan : IPlan
	{
		// Stat
		StatBlock StatBlock { get; }
		ResourceBlock ResourceBlock { get; }

		// Initial
		List<IToken> StartingTokens { get; }
		List<BuffPlan> InitBuffs { get; }
		List<SkillPlan> InitSkills { get; }
	}
}