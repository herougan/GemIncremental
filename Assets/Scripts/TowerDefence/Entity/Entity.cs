
using System;
using System.Collections.Generic;
using TowerDefence.Stats;
using Util.Maths;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Resources;
using Util.Serialisation;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Entity.Token;
using System.Linq;
using Util.Debug;
using UnityEngine;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Context;
using TowerDefence.Entity.Behaviour;
using Util.Events;

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
		IBehaviour Behaviour { get; }

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

		// Fusion Event
		// public event Action<IEntity, IExpirable, IExpirable, IEffectInteraction> OnBuffFused;

		// Game Space
		public event Action<TriggerContext> OnReached;
		public event Action<TriggerContext> OnSpawn;
		public event Action<TriggerContext> OnPushBack;
		public event Action<TriggerContext> OnHidden;

		// Behaviour
		public event Action<TriggerContext> OnFirstHurt;
		public event Action<TriggerContext> OnFirstBuff;
		public event Action<TriggerContext> OnTargetted;
		public event Action<TriggerContext> OnUntargetted;
		public event Action<TriggerContext> OnCast;

		// Allies
		public event Action<TriggerContext> OnAllySpawn;
		public event Action<TriggerContext> OnAllyDeath;
		public event Action<TriggerContext> OnAllyHurt;
		public event Action<TriggerContext> OnAllyHeal;
		public event Action<TriggerContext> OnAllyBuff;
		public event Action<TriggerContext> OnAllyStatus;

		// = Range =
		// On Ally/On Enemy/On Tagged - Particular treatment handled within e.g. OnIsolation from "Piano" enemies
		public event Action<TriggerContext> OnEnteredRange; // From, To
		public event Action<TriggerContext> OnExitRange; // From, To
		public event Action<TriggerContext> OnEnteredAttackRange;// From (this), To
		public event Action<TriggerContext> OnExitAttackRange; // From, To
		public event Action<TriggerContext> OnIsolated; // Leaving
		public event Action<TriggerContext> OnNotIsolated; // Entering

		// Combat
		public event Action<TriggerContext> OnAttack;
		public event Action<TriggerContext> OnAttacked; // Upon being hit by an enemy with a normal attack
		public event Action<TriggerContext> OnHit; // On hitting an enemy with a normal attack
		public event Action<TriggerContext> OnDOT; // On Damage over time tick (Skill/Expirable)
		public event Action<TriggerContext> OnDeath; // If Health <= 0
		public event Action<TriggerContext> OnKill; // Kills another
		public event Action<TriggerContext> OnHurt;
		public event Action<TriggerContext> OnHeal; // If Health > 0
		public event Action<TriggerContext> OnVamp;
		public event Action<TriggerContext> OnBloodied;
		public event Action<TriggerContext> OnDying;
		public event Action<TriggerContext> OnRage;
		public event Action<TriggerContext> OnManaDry;

		// Stat
		public event Action<TriggerContext> OnValueChanged;
		public event Action<TriggerContext> OnValueDecreased;
		public event Action<TriggerContext> OnValueIncreased;
		public event Action<TriggerContext> OnMaxValueChanged;
		public event Action<TriggerContext> OnCurrentValueDecreased;
		public event Action<TriggerContext> OnCurrentValueIncreased;
		public event Action<TriggerContext> OnStatBonusAdded;
		public event Action<TriggerContext> OnStatNerfAdded;
		public event Action<TriggerContext> OnRegenerate;
		public event Action<TriggerContext> OnRest;
		public event Action<TriggerContext> OnRegenValueChanged;
		public event Action<TriggerContext> OnRegenRateChanged;
		//
		public event Action<TriggerContext> OnStatusResistChanged;
		public event Action<TriggerContext> OnStatusMasteryChanged;
		public event Action<TriggerContext> OnThresholdCrossed;
		public event Action<TriggerContext> OnThresholdChanged;

		// == Resource ==
		public event Action<TriggerContext> OnResourceIncreased;
		public event Action<TriggerContext> OnResourceDecreased;

		// == Element ==
		public event Action<TriggerContext> OnEleResistChanged;
		public event Action<TriggerContext> OnEleMasteryChanged;

		// == Token ==
		public event Action<TriggerContext> OnTokenChanged;
		public event Action<TriggerContext> OnTokenTransmute;
		public event Action<TriggerContext> OnTokenExchange;
		public event Action<TriggerContext> OnNewToken;

		// == Buffs == 
		public event Action<TriggerContext> OnBuffApplied;
		public event Action<TriggerContext> OnDebuffApplied;
		public event Action<TriggerContext> OnBuffRemoved;
		public event Action<TriggerContext> OnDebuffCleansed;
		public event Action<TriggerContext> OnBuffExpired;
		public event Action<TriggerContext> OnDebuffExpired;
		public event Action<TriggerContext> OnBuffStacked;

		// == Mileage ==
		// public event Action<TriggerContext> OnMileageIncreased;
		// Trigger.Parameter > Limit -> Invoke();


		// Register Callbacks()
		void RegisterCallbacks();
		void RegisterStatCallbacks();
		void RegisterResourceCallbacks();
		void RegisterTokenCallbacks();
		void RegisterElementCallbacks();
		void RegisterSkillCallbacks();

		// === Trigger Hooks ===
		public List<WrappedAction> WrappedActions { get; }
		public Action<TriggerContext> GetEvent(TriggerType triggerType);
		public void SubscribeEvent(TriggerType type, Action<TriggerContext> _delegate);

		#endregion Events

		#region Lifecycle
		// ===== Lifecycle =====
		public void Spawn();
		public void Regenerate();
		public void Rest();
		public void Tick(float t);
		public void Die();
		public void Attack(); // move attack timer

		// ===== Timer related =====
		public CountdownTimer AddTimer(float duration, bool repeat = false, Action<CountdownTimer> callback = null);

		#endregion Lifecycle

		#region Methods

		// ===== Getters =====
		IResistance GetStatus(StatusType type);
		float GetKinematics(KinematicsType type);
		IResource GetResource(ResourceType type);
		IStat GetStat(StatType type);
		IElement GetElement(ElementType type);
		IToken GetToken(TokenType type);
		IMileage GetMileage(MileageType type);


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
		public IBehaviour Behaviour { get; protected set; }

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

		// = State =
		// Game Space  // *Self*
		public event Action<TriggerContext> OnReached = delegate { };
		public event Action<TriggerContext> OnSpawn = delegate { };
		public event Action<TriggerContext> OnPushBack = delegate { };
		public event Action<TriggerContext> OnHidden = delegate { };

		// Behaviour  // *Self*
		public event Action<TriggerContext> OnFirstHurt = delegate { };
		public event Action<TriggerContext> OnFirstBuff = delegate { };
		public event Action<TriggerContext> OnTargetted = delegate { };
		public event Action<TriggerContext> OnUntargetted = delegate { };
		public event Action<TriggerContext> OnCast = delegate { };

		// Allies  // *Self*
		public event Action<TriggerContext> OnAllySpawn = delegate { };
		public event Action<TriggerContext> OnAllyDeath = delegate { };
		public event Action<TriggerContext> OnAllyHurt = delegate { };
		public event Action<TriggerContext> OnAllyHeal = delegate { };
		public event Action<TriggerContext> OnAllyBuff = delegate { };
		public event Action<TriggerContext> OnAllyStatus = delegate { };

		// Range  // *Self*
		public event Action<TriggerContext> OnEnteredRange = delegate { }; // From, To
		public event Action<TriggerContext> OnExitRange = delegate { }; // From, To
		public event Action<TriggerContext> OnEnteredAttackRange = delegate { };// From (this), To
		public event Action<TriggerContext> OnExitAttackRange = delegate { }; // From, To
		public event Action<TriggerContext> OnIsolated = delegate { };
		public event Action<TriggerContext> OnNotIsolated = delegate { };

		// Combat  // *Self*
		public event Action<TriggerContext> OnAttack = delegate { };
		public event Action<TriggerContext> OnAttacked = delegate { };
		public event Action<TriggerContext> OnHit = delegate { };
		public event Action<TriggerContext> OnDOT = delegate { };
		public event Action<TriggerContext> OnDeath = delegate { }; // If Health <= 0; Dier, Source (Killer)
		public event Action<TriggerContext> OnKill = delegate { };
		public event Action<TriggerContext> OnHurt = delegate { };
		public event Action<TriggerContext> OnHeal = delegate { }; // If Health > 0
		public event Action<TriggerContext> OnVamp = delegate { };
		public event Action<TriggerContext> OnBloodied = delegate { };
		public event Action<TriggerContext> OnDying = delegate { };
		public event Action<TriggerContext> OnRage = delegate { };
		public event Action<TriggerContext> OnManaDry = delegate { };

		// === Stat Block ===
		public event Action<TriggerContext> OnValueChanged = delegate { };
		public event Action<TriggerContext> OnValueDecreased = delegate { };
		public event Action<TriggerContext> OnValueIncreased = delegate { };
		//
		public event Action<TriggerContext> OnMaxValueChanged = delegate { };
		public event Action<TriggerContext> OnCurrentValueDecreased = delegate { };
		public event Action<TriggerContext> OnCurrentValueIncreased = delegate { };
		//
		public event Action<TriggerContext> OnStatBonusAdded = delegate { };
		public event Action<TriggerContext> OnStatNerfAdded = delegate { };
		//
		public event Action<TriggerContext> OnRegenerate = delegate { };
		public event Action<TriggerContext> OnRest = delegate { };
		public event Action<TriggerContext> OnRegenValueChanged = delegate { };
		public event Action<TriggerContext> OnRegenRateChanged = delegate { };
		//
		public event Action<TriggerContext> OnStatusResistChanged = delegate { };
		public event Action<TriggerContext> OnStatusMasteryChanged = delegate { };
		public event Action<TriggerContext> OnThresholdCrossed = delegate { };
		public event Action<TriggerContext> OnThresholdChanged = delegate { };

		// == Resource ==
		public event Action<TriggerContext> OnResourceIncreased = delegate { };
		public event Action<TriggerContext> OnResourceDecreased = delegate { };

		// == Element ==
		public event Action<TriggerContext> OnEleResistChanged = delegate { };
		public event Action<TriggerContext> OnEleMasteryChanged = delegate { };

		// == Token ==
		public event Action<TriggerContext> OnTokenChanged = delegate { };
		public event Action<TriggerContext> OnTokenTransmute = delegate { };
		public event Action<TriggerContext> OnTokenExchange = delegate { };
		public event Action<TriggerContext> OnNewToken = delegate { };

		// == Buffs ==  // *Self*
		public event Action<TriggerContext> OnBuffApplied = delegate { };
		public event Action<TriggerContext> OnDebuffApplied = delegate { };
		public event Action<TriggerContext> OnBuffRemoved = delegate { };
		public event Action<TriggerContext> OnDebuffCleansed = delegate { };
		public event Action<TriggerContext> OnBuffExpired = delegate { };
		public event Action<TriggerContext> OnDebuffExpired = delegate { };
		public event Action<TriggerContext> OnBuffStacked = delegate { };


		// === Register Callbacks ===
		public List<WrappedAction> WrappedActions { get; }
		public void RegisterCallbacks()
		{
			RegisterStatCallbacks();
			RegisterResourceCallbacks();
			RegisterElementCallbacks();
			RegisterTokenCallbacks();
		}
		public void RegisterStatCallbacks()
		{
			// Register Stats callbacks to itself
			StatBlock.RegisterCallbacks();

			// Register StatBlock's callbacks to this entity
			StatBlock.OnValueChanged += (s, v) => OnValueChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnValueChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnValueDecreased += (s, v) => OnValueDecreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnValueDecreased,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnValueIncreased += (s, v) => OnValueIncreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnValueIncreased,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnMaxValueChanged += (s, v) => OnMaxValueChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnMaxValueChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnCurrentValueDecreased += (s, v) => OnCurrentValueDecreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnCurrentValueDecreased,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnCurrentValueIncreased += (s, v) => OnCurrentValueIncreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnCurrentValueIncreased,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnStatBonusAdded += (s, m) => OnStatBonusAdded.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnStatBonusAdded,
				Entity = this,
				Stat = s,
				Mod = m,
			});
			StatBlock.OnStatNerfAdded += (s, m) => OnStatNerfAdded.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnStatNerfAdded,
				Entity = this,
				Stat = s,
				Mod = m,
			});
			StatBlock.OnRegenerate += (s, v) => OnRegenerate.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnRegenerate,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnRest += (s, v) => OnRest.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnRest,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnRegenValueChanged += (s, v) => OnRegenValueChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnRegenValueChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnRegenRateChanged += (s, v) => OnRegenRateChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnRegenRateChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnResistChanged += (s, v) => OnStatusResistChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnStatusResistChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnMasteryChanged += (s, v) => OnStatusMasteryChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnStatusMasteryChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnThresholdCrossed += (s, v) => OnThresholdCrossed.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnThresholdCrossed,
				Entity = this,
				Stat = s,
				Value = v,
			});
			StatBlock.OnThresholdChanged += (s, v) => OnThresholdChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnThresholdChanged,
				Entity = this,
				Stat = s,
				Value = v,
			});
		}
		public void RegisterResourceCallbacks()
		{
			ResourceBlock.RegisterCallbacks();
			ResourceBlock.OnResourceIncreased += (r, v) => OnResourceIncreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnResourceIncreased,
				Entity = this,
				Resource = r,
				Value = v,
			});
			ResourceBlock.OnResourceDecreased += (r, v) => OnResourceDecreased.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnResourceDecreased,
				Entity = this,
				Resource = r,
				Value = v,
			});
		}
		public void RegisterTokenCallbacks()
		{
			TokenInventory.RegisterCallbacks();
			TokenInventory.OnTokenChanged += (t, v) => OnTokenChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnTokenChanged,
				Entity = this,
				Token = t,
				Value = v,
			});
			TokenInventory.OnTokenTransmute += (f, t, v) => OnTokenTransmute.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnTokenTransmute,
				Entity = this,
				Token = f,
				NewToken = t,
				Value = v,
			});
			TokenInventory.OnTokenExchange += (f, t) => OnTokenExchange.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnTokenExchange,
				Entity = this,
				Token = f,
				NewToken = t,
			});
			TokenInventory.OnNewToken += (t) => OnNewToken.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnNewToken,
				Entity = this,
				Token = t,
			});
		}
		public void RegisterElementCallbacks()
		{
			ElementBlock.RegisterCallbacks();
			ElementBlock.OnResistChanged += (e, v) => OnEleResistChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnEleResistChanged,
				Entity = this,
				Element = e,
				Value = v,
			});
			ElementBlock.OnMasteryChanged += (e, v) => OnEleMasteryChanged.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnEleMasteryChanged,
				Entity = this,
				Element = e,
				Value = v,
			});

		}
		public void RegisterSkillCallbacks()
		{
			// Unused unless skill callbacks were reset and need to be re-wired
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
		public void RegisterStatCallback(IStat stat)
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
				RegisterTriggerCallback(trigger, effect.Actions);
			}
		}
		public void RegisterTriggerCallback(ITrigger trigger, List<IAction> actions)
		{


		}
		public void DeregisterSkillCallback(ISkill skill)
		{
			// skill.OnSkillUsed -= EffectController.Instance.ResolveSkill;

			// Instead of finding triggers under the skill (which may have changed somehow), 
			// Detach all references to this skill
		}

		// ===== Trigger Hooks =====
		public Action<TriggerContext> GetEvent(TriggerType triggerType)
		{
			switch (triggerType)
			{
				case TriggerType.OnCast:
					return OnCast;
				// Hi, please return all Entity Action<TriggerContext> based on TriggerType
				case TriggerType.OnPeriodic:
					LogManager.Instance.LogWarning($"Trigger type {triggerType} call is ambiguous");
					return null;
				default:
					LogManager.Instance.LogWarning($"Trigger type {triggerType} not implemented in GetEvent");
					return null;
			}
		}

		/// <summary>
		/// Raw hook for an Action<TriggerContext> to an Entity's events
		/// </summary>
		/// <param name="type"></param>
		/// <param name="_delegate"></param>
		public void SubscribeEvent(TriggerType type, Action<TriggerContext> _delegate)
		{
			Action<TriggerContext> action = GetEvent(type);
			action += _delegate;

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
			OnSpawn?.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnSpawn,
				Entity = this,
			});
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
			OnRest?.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnRest,
				Entity = this,
			});
		}

		public void Tick(float t)
		{
			StatBlock.Tick(t);
			MileageBlock.Tick(t);
		}

		public void Die()
		{
			OnDeath?.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnDeath,
				Entity = this,
			});
		}

		public void Attack()
		{
			OnAttack?.Invoke(new TriggerContext()
			{
				TriggerType = TriggerType.OnAttack,
				Entity = this,
			});
		}

		public CountdownTimer AddTimer(float duration, bool repeat = false, Action<TriggerContext> callback = null, ISkill skill = null)
		{
			CountdownTimer timer = new CountdownTimer(duration, repeat);
			if (callback != null)
			{
				WrappedAction wrapped = new WrappedAction(timer.OnRing, callback, skill, TriggerType.OnPeriodic);
			}

			return timer;
		}

		#endregion Lifecycle

		#region Methods

		// ===== Getters ===== 
		public Resistance GetStatus(StatusType statusType)
		{
			Resistance r = StatBlock.Resistances.Find(s => s.Status == statusType);
			if (r == null) LogManager.Instance.LogWarning($"Entity {this.Guid} does not have status {statusType}");
			return r;
		}

		public Kinematics GetKinematics(KinematicsCondition kinCon)
		{
			return Kinematics;
		}

		IResistance IEntity.GetStatus(StatusType type)
		{
			return GetStatus(type);
		}

		public float GetKinematics(KinematicsType type)
		{
			throw new NotImplementedException();
		}

		public IResource GetResource(ResourceType type)
		{
			throw new NotImplementedException();
		}

		public IStat GetStat(StatType type)
		{
			throw new NotImplementedException();
		}

		public IElement GetElement(ElementType type)
		{
			throw new NotImplementedException();
		}

		public IToken GetToken(TokenType type)
		{
			throw new NotImplementedException();
		}

		public IMileage GetMileage(MileageType type)
		{
			throw new NotImplementedException();
		}

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
				OnBuffStacked?.Invoke(new TriggerContext()
				{
					TriggerType = TriggerType.OnBuffStacked,
					Entity = this,
					Expirable = existing,
				});
			}
			else
			{
				// Add buff
				Buffs.Add(buff);
				OnBuffApplied?.Invoke(new TriggerContext()
				{
					TriggerType = TriggerType.OnBuffApplied,
					Entity = this,
					Expirable = buff,
				});
			}
		}

		public void CleanseBuff(IBuff buff)
		{
			if (Buffs.Any(b => b.BuffType == buff.BuffType))
			{
				// Remove
				var existing = Buffs.First(b => b.BuffType == buff.BuffType);
				Buffs.Remove(existing);
				if (existing.IsPositive)
					OnBuffExpired?.Invoke(new TriggerContext()
					{
						TriggerType = TriggerType.OnBuffExpired,
						Entity = this,
						Expirable = existing,
					});
				else OnDebuffCleansed?.Invoke(new TriggerContext()
				{
					TriggerType = TriggerType.OnDebuffCleansed,
					Entity = this,
					Expirable = existing,
				});
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

		// Tags
		public void AddTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void ApplyDamage(ddouble damage)
		{
			throw new NotImplementedException();
		}

		public void ApplyModification(IAction action)
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