
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
using TowerDefence.Entity.Skills.ActionHandler;
using TowerDefence.Entity.Skills.Passives;
// A separate alias, not a plain `using TowerDefence.Entity.Tower;` - this file's own namespace
// (TowerDefence.Entity) has TowerDefence.Entity.Tower as a *nested* namespace, which enclosing-
// namespace lookup finds before any using-directive is considered, so a bare `Tower` here always
// resolves to the namespace (CS0118), never the class - same issue EntityManager.cs's own
// MonsterEntity alias exists for.
using TowerEntity = TowerDefence.Entity.Tower.Tower;

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
		List<IBuff> Buffs { get; } // Temporary Skills, grantable by other Skills (ActionType.ApplyBuff) - see Buff : Skill.
		List<ISkill> Skills { get; } // Starting skills
		List<StatusBuff> Statuses { get; }
		// Auras this Entity is *receiving* - permabuffs from a nearby Tower/Monster source, not auras it
		// casts itself (the source's own Aura buff lives in its own Buffs list; these are what
		// propagates onto everyone in range - see Aura.cs's IsInstance). No separate AuraInstances list -
		// these two already are the received instances.
		List<IBuff> TowerAuras { get; }
		List<IBuff> MonsterAuras { get; }

		#endregion Abilities

		#region Events

		// Every notable thing that can happen to an entity is looked up by TriggerType rather than
		// declared as its own named field - see GetEvent/SubscribeEvent/RaiseEvent below. This used to
		// be ~60 separate `event Action<TriggerContext>` fields plus a hand-maintained switch mapping
		// TriggerType -> field in GetEvent; every addition meant touching four places in lockstep
		// (interface field, class field, enum value, switch case) and a miss failed silently at
		// runtime, not compile time. Same "pigeon-hole -> single concrete dispatch" lesson as Action/
		// ActionType.

		// Register Callbacks()
		void RegisterCallbacks();
		void RegisterResourceCallbacks();
		void RegisterTokenCallbacks();
		void RegisterElementCallbacks();
		void RegisterSkillCallbacks();

		// === Trigger Hooks ===
		public List<WrappedAction> WrappedActions { get; }

		/// <summary>Current multicast delegate for a TriggerType, or null if nobody has subscribed to it.</summary>
		public Action<TriggerContext> GetEvent(TriggerType triggerType);

		/// <summary>
		/// The one legal way to attach a handler to a trigger - do not `GetEvent(type) += handler`
		/// yourself, that mutates a local copy of the delegate and silently attaches nothing.
		/// </summary>
		public void SubscribeEvent(TriggerType type, Action<TriggerContext> del);

		/// <summary>Symmetric detach for SubscribeEvent - see WrappedAction.Detach.</summary>
		public void UnsubscribeEvent(TriggerType type, Action<TriggerContext> del);

		/// <summary>
		/// Fires a trigger: invokes every handler currently subscribed to it. Entity's own lifecycle
		/// methods (Spawn/Die/Attack/...) use this internally, and periodic skill ticks (see
		/// WrappedAction) call it externally to raise OnSkillActivate.
		/// </summary>
		public void RaiseEvent(TriggerType type, TriggerContext ctx);

		#endregion Events

		#region Lifecycle
		// ===== Lifecycle =====
		public void Spawn();
		public void Regenerate();
		public void Rest();
		public void Tick(float t);
		public void Die();
		// damage is optional and purely informational here - Attack/GotHit still just raise the
		// trigger; carrying the Damage that caused the hit onto TriggerContext.Damage lets a listener
		// (e.g. ReflectActionHandler) see what actually happened without a separate lookup. The
		// intended call order for a real hit is ApplyDamage(damage) first, then Attack/GotHit with that
		// same object, so FinalValue is already resolved by the time anything reacts to OnHit.
		public void Attack(IEntity target, Damage damage = null); // move attack timer
		public void GotHit(IEntity source, Damage damage = null);

		// The "memory" a Revenge-style skill needs - see Entity.GotHit/ClearDamageLog and ActionType.Revenge.
		public Dictionary<IEntity, ddouble> DamageReceivedLog { get; }
		public void ClearDamageLog();

		// No range/collision system exists yet to call these automatically - see EnterRange/
		// EnterAttackRange on Entity for why they're exposed as direct calls in the meantime.
		public void EnterRange(IEntity other);
		public void ExitRange(IEntity other);
		public void EnterAttackRange(IEntity other);
		public void ExitAttackRange(IEntity other);

		// STUB - see EntityState's own comment.
		public EntityState CurrentState { get; }
		public void EntityStateChange(EntityState entityState);

		// ===== Timer related =====
		public CountdownTimer AddTimer(float duration, bool repeat = false, Action<TriggerContext> callback = null, ISkill skill = null);

		public void AddTimer(CountdownTimer timer);

		public void RemoveTimer(CountdownTimer timer);

		#endregion Lifecycle

		#region Methods

		// ===== Getters =====
		IResistance GetStatus(StatusType type);
		float GetKinematics(KinematicsType type);
		IResource GetResource(ResourceType type);
		/// <summary>Returns the stat's current Value, or 0 if it was never created - never creates it.</summary>
		ddouble GetStat(StatType type);
		/// <summary>Returns the element object if it exists, or null - never creates it.</summary>
		IElement GetElement(ElementType type);
		IToken GetToken(TokenType type);
		IMileage GetMileage(MileageType type);
		/// <summary>Generic per-Entity "+1"-style counter (ConditionType.Counter/CounterCondition) - 0 if never set. Unlike Mileage, author/handler-set to an arbitrary value directly, not only ever accumulated.</summary>
		int GetCounter(CounterType type);
		void SetCounter(CounterType type, int value);
		void IncrementCounter(CounterType type, int amount = 1);


		// Buff Manipulation
		public void ApplyBuff(IBuff buff);
		public void CleanseBuff(IBuff buff);
		public void CleanseRandom();

		/// <summary>Grants a received Aura instance (IAura.IsInstance == true) - routed into TowerAuras/MonsterAuras by who cast it, not Buffs (see those two lists' own doc comment on IEntity). Only AuraPropagationService/Aura.Grant should call this.</summary>
		public void ApplyAuraInstance(IAura instance);
		/// <summary>Symmetric removal for ApplyAuraInstance - only AuraPropagationService/Aura.Leave should call this (a lingering instance removes itself by expiring naturally instead - see Aura.Leave).</summary>
		public void RemoveAuraInstance(IAura instance);
		public void ApplyDamage(ddouble damage);
		public void Recalculate(List<StatType> types, List<double> values);

		// Entity Ability method
		public void RegisterSkill(ISkill Skill);
		public void DeregisterSkill(ISkill skill);

		public void RegisterSkills();

		// StatMods - see Entity's own StatMods region for the full picture.
		public void RegisterStatMod(IStatMod mod);
		public void DeregisterStatMod(IStatMod mod);

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
		readonly Dictionary<CounterType, int> counters = new();

		// Behaviour
		public IBehaviour Behaviour { get; protected set; }

		#endregion State

		#region Abilities

		// ===== Skills =====
		// Abilities
		public List<IBuff> Buffs { get; protected set; } // Active, tracked during game
		public List<ISkill> Skills { get; protected set; }// Starting skills
		public List<StatusBuff> Statuses { get; protected set; } // List of active status effects
		public List<IBuff> TowerAuras { get; protected set; } // Received - see IEntity's own comment
		public List<IBuff> MonsterAuras { get; protected set; }

		#endregion Abilities

		#region Constructor

		/// <summary>
		/// Builds fresh, per-instance runtime state from a Plan's sparse design-time data. StatBlock/
		/// ElementBlock/ResourceBlock are always new objects here - never the Plan's own - so two Entities
		/// spawned from the same Plan never end up sharing one Health pool.
		/// </summary>
		public Entity(IEntityPlan plan)
		{
			Plan = plan;
			Guid = new SerialisableGuid(System.Guid.NewGuid());

			StatBlock = new StatBlock(plan.StartingHealth, plan.StatEntries, plan.StatusEntries);
			ElementBlock = new ElementBlock();
			foreach (ElementEntry entry in plan.ElementEntries)
			{
				ElementBlock.ApplyEntry(entry);
			}
			ResourceBlock = new ResourceBlock();

			Tags = new List<Tag>();
			MileageBlock = new MileageBlock();
			TokenInventory = new TokenInventory();
			Kinematics = new Kinematics();

			Buffs = new List<IBuff>();
			Skills = new List<ISkill>();
			Statuses = new List<StatusBuff>();
			TowerAuras = new List<IBuff>();
			MonsterAuras = new List<IBuff>();

			WrappedActions = new List<WrappedAction>();
		}

		#endregion Constructor

		#region Events

		Dictionary<TriggerType, Action<TriggerContext>> _events = new();

		public Action<TriggerContext> GetEvent(TriggerType type) =>
			_events.TryGetValue(type, out var a) ? a : null;

		public void SubscribeEvent(TriggerType type, Action<TriggerContext> del)
		{
			_events[type] = _events.TryGetValue(type, out var a) ? a + del : del;
		}

		public void UnsubscribeEvent(TriggerType type, Action<TriggerContext> del)
		{
			if (!_events.TryGetValue(type, out var a)) return;
			a -= del;
			if (a == null) _events.Remove(type);
			else _events[type] = a;
		}

		public void RaiseEvent(TriggerType type, TriggerContext ctx)
		{
			ctx.TriggerType = type;
			GetEvent(type)?.Invoke(ctx);
			// The bus itself, not EntityManager - see Util.Events.EntityEventBus. Static, so this
			// reaches every subscriber even with no scene/GameObject in play at all (e.g. a plain-C# test).
			Util.Events.EntityEventBus.Publish(ctx);
		}

		// === Register Callbacks ===
		public List<WrappedAction> WrappedActions { get; }
		public void RegisterCallbacks()
		{
			StatBlock.RegisterCallbacks();
			RegisterStatCallbacks();
			RegisterResourceCallbacks();
			RegisterElementCallbacks();
			RegisterTokenCallbacks();
		}

		/// <summary>
		/// Bridges StatBlock's raw, narrowly-typed events (Action&lt;IStat, ddouble&gt;, etc.) into this
		/// entity's TriggerType-keyed dictionary, populating the Stat field on the way so subscribers
		/// can tell which stat actually changed - see TriggerContext.
		/// </summary>
		public void RegisterStatCallbacks()
		{
			StatBlock.OnValueChanged += (s, v) => RaiseEvent(TriggerType.OnValueChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnValueDecreased += (s, v) => RaiseEvent(TriggerType.OnValueDecreased, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnValueIncreased += (s, v) => RaiseEvent(TriggerType.OnValueIncreased, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnMaxValueChanged += (s, v) => RaiseEvent(TriggerType.OnMaxValueChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnCurrentValueDecreased += (s, v) => RaiseEvent(TriggerType.OnCurrentValueDecreased, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnCurrentValueIncreased += (s, v) => RaiseEvent(TriggerType.OnCurrentValueIncreased, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnStatBonusAdded += (s, m) => RaiseEvent(TriggerType.OnStatBonusAdded, new TriggerContext { Entity = this, Stat = s, Mod = m });
			StatBlock.OnStatNerfAdded += (s, m) => RaiseEvent(TriggerType.OnStatNerfAdded, new TriggerContext { Entity = this, Stat = s, Mod = m });
			StatBlock.OnRegenerate += (s, v) => RaiseEvent(TriggerType.OnRegenerate, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnRest += (s, v) => RaiseEvent(TriggerType.OnRest, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnRegenValueChanged += (s, v) => RaiseEvent(TriggerType.OnRegenValueChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnRegenRateChanged += (s, v) => RaiseEvent(TriggerType.OnRegenRateChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnResistChanged += (s, v) => RaiseEvent(TriggerType.OnStatusResistChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnMasteryChanged += (s, v) => RaiseEvent(TriggerType.OnStatusMasteryChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnThresholdCrossed += (s, v) => RaiseEvent(TriggerType.OnThresholdCrossed, new TriggerContext { Entity = this, Stat = s, Value = v });
			StatBlock.OnThresholdChanged += (s, v) => RaiseEvent(TriggerType.OnThresholdChanged, new TriggerContext { Entity = this, Stat = s, Value = v });
		}
		public void RegisterResourceCallbacks()
		{
			ResourceBlock.RegisterCallbacks();
			ResourceBlock.OnResourceIncreased += (r, v) => RaiseEvent(TriggerType.OnResourceIncreased, new TriggerContext { Entity = this, Resource = r, Value = v });
			ResourceBlock.OnResourceDecreased += (r, v) => RaiseEvent(TriggerType.OnResourceDecreased, new TriggerContext { Entity = this, Resource = r, Value = v });
		}
		public void RegisterTokenCallbacks()
		{
			TokenInventory.RegisterCallbacks();
			TokenInventory.OnTokenChanged += (t, v) => RaiseEvent(TriggerType.OnTokenChanged, new TriggerContext { Entity = this, Token = t, Value = v });
			TokenInventory.OnTokenTransmute += (f, t, v) => RaiseEvent(TriggerType.OnTokenTransmute, new TriggerContext { Entity = this, Token = f, NewToken = t, Value = v });
			TokenInventory.OnTokenExchange += (f, t) => RaiseEvent(TriggerType.OnTokenExchange, new TriggerContext { Entity = this, Token = f, NewToken = t });
			TokenInventory.OnNewToken += (t) => RaiseEvent(TriggerType.OnNewToken, new TriggerContext { Entity = this, Token = t });
		}
		public void RegisterElementCallbacks()
		{
			ElementBlock.RegisterCallbacks();
			ElementBlock.OnResistChanged += (e, v) => RaiseEvent(TriggerType.OnEleResistChanged, new TriggerContext { Entity = this, Element = e, Value = v });
			ElementBlock.OnMasteryChanged += (e, v) => RaiseEvent(TriggerType.OnEleMasteryChanged, new TriggerContext { Entity = this, Element = e, Value = v });
		}
		public void RegisterSkillCallbacks()
		{
			// Unused unless skill callbacks were reset and need to be re-wired.
			// The actual wiring lives on Skill itself now - see Skill.RegisterCallbacks/RegisterSkills.
			RegisterSkills();
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
		public void DeregisterSkillCallback(ISkill skill)
		{
			skill.Deregister();
		}

		#endregion Events

		#region Lifecycle

		// ===== Lifecycle =====
		public void Spawn()
		{
			// Apply InitBuffs - Buff.Create, not `new Buff(...)` directly, so a Plan authored with
			// IsAura actually spawns a real Aura (and starts propagating - see Aura.RegisterCallbacks)
			// instead of an inert plain Buff that just happens to carry unused Aura fields.
			foreach (var buffPlan in Plan.InitBuffs)
			{
				ApplyBuff(Buff.Create(buffPlan, this));
			}

			// Get starting tokens
			foreach (var token in Plan.StartingTokens)
			{
				TokenInventory.Add(token);
			}

			// Get Skills based on EntityPlan
			foreach (var skillPlan in Plan.InitSkills)
			{
				Skills.Add(new Skill(skillPlan, this));
			}

			// Wire StatBlock/ResourceBlock/ElementBlock/TokenInventory bubbling, then each
			// Skill/Buff's own triggers (which needs Skills/Buffs already populated above).
			// NOTE: once EntityManager actually spawns entities (currently mostly commented-out wave
			// logic - see EntityManager.RegisterEntityCallbacks), this is expected to move there instead
			// of being called from Spawn() directly.
			RegisterCallbacks();
			RegisterSkills();

			// Events
			RaiseEvent(TriggerType.OnSpawn, new TriggerContext { Entity = this });
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
					// Fires that stat's own OnRest, bubbled StatBlock -> RegisterStatCallbacks -> here.
					// No separate generic invoke - that would double-fire once per stat plus once overall.
				}
			}
		}

		public void Tick(float t)
		{
			StatBlock.Tick(t);
			MileageBlock.Tick(t);
			foreach (var timer in countdownTimers) timer.Tick(t);
			// ToList() - a Buff's own Tick can fire OnExpired synchronously (see HandleBuffExpired ->
			// CleanseBuff -> Buffs.Remove), which would otherwise mutate this collection while it's
			// still being enumerated.
			foreach (IBuff buff in Buffs.ToList()) buff.Tick(t);
		}

		public void Die()
		{
			RaiseEvent(TriggerType.OnDeath, new TriggerContext { Entity = this });
		}

		public void Attack(IEntity target, Damage damage = null)
		{
			RaiseEvent(TriggerType.OnAttack, new TriggerContext { Entity = this, Target = target, Damage = damage });
		}

		public void GotHit(IEntity source, Damage damage = null)
		{
			if (damage != null)
			{
				// The "memory" a Revenge-style skill needs - who hit this Entity and how much, since the
				// last time something cleared it (see ClearDamageLog, ActionType.Revenge). Grows forever
				// otherwise: nothing ever culls old entries on its own, only a full clear.
				DamageReceivedLog[source] = DamageReceivedLog.GetValueOrDefault(source) + damage.FinalValue;
			}
			RaiseEvent(TriggerType.OnHit, new TriggerContext { Entity = this, Target = source, Damage = damage });
		}

		/// <summary>Who has hit this Entity, and how much total FinalValue damage, since the last ClearDamageLog() - see GotHit.</summary>
		public Dictionary<IEntity, ddouble> DamageReceivedLog { get; } = new();

		public void ClearDamageLog() => DamageReceivedLog.Clear();

		// Stand-ins for a real range/collision system (nothing computes distances or fires these
		// automatically yet) - called directly by whatever eventually tracks positions (or by hand, for
		// testing), same shape as Attack/GotHit above.
		public void EnterRange(IEntity other)
		{
			RaiseEvent(TriggerType.OnEnteredRange, new TriggerContext { Entity = this, Target = other });
		}

		public void ExitRange(IEntity other)
		{
			RaiseEvent(TriggerType.OnExitRange, new TriggerContext { Entity = this, Target = other });
		}

		public void EnterAttackRange(IEntity other)
		{
			RaiseEvent(TriggerType.OnEnteredAttackRange, new TriggerContext { Entity = this, Target = other });
		}

		public void ExitAttackRange(IEntity other)
		{
			RaiseEvent(TriggerType.OnExitAttackRange, new TriggerContext { Entity = this, Target = other });
		}

		// STUB - see EntityState's own comment. Nothing calls this automatically yet; a listener reads
		// the new state off ctx.Entity.CurrentState rather than a dedicated TriggerContext field, since
		// ctx.Entity already gives you that for free.
		public EntityState CurrentState { get; private set; } = EntityState.Idle;

		public void EntityStateChange(EntityState entityState)
		{
			CurrentState = entityState;
			RaiseEvent(TriggerType.OnEntityStateChange, new TriggerContext { Entity = this });
		}

		List<CountdownTimer> countdownTimers = new();
		public CountdownTimer AddTimer(float duration, bool repeat = false, Action<TriggerContext> callback = null, ISkill skill = null)
		{
			CountdownTimer timer = new CountdownTimer(duration, repeat);
			AddTimer(timer);
			return timer;
		}

		public void AddTimer(CountdownTimer timer)
		{
			countdownTimers.Add(timer);
			// Without this, a periodic skill's timer (see Skill.RegisterTrigger) sits in this list
			// forever with `started == false` - Entity.Tick's `foreach (timer) timer.Tick(t)` is a no-op
			// for any timer that was never Start()ed, so the periodic effect would never fire.
			timer.Start();
		}

		public void RemoveTimer(CountdownTimer timer)
		{
			countdownTimers.Remove(timer);
		}

		#endregion Lifecycle

		#region Methods

		// ===== Getters ===== 
		public IResistance GetStatus(StatusType statusType)
		{
			IResistance r = StatBlock.Resistances.Find(s => s.Status == statusType);
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

		/// <summary>
		#region StatMods

		// Every source (a Skill's Passives, a Buff, an Aura, Foundry, World/Player) that wants to modify
		// this Entity's stats registers an IStatMod here, keyed by which StatType it affects - not on
		// StatBlock, not on the Stat itself. Entity-level was the deliberate choice: recalculating a
		// stat only ever needs *this* dictionary (no per-Stat scanning), and tearing down a whole
		// source's contribution (DeregisterSkill) is one pass over one list, not a hunt through every
		// Stat's own mod list for entries matching that source.
		readonly Dictionary<StatType, List<IStatMod>> statMods = new();
		readonly HashSet<StatType> dirtyStats = new();

		/// <summary>Registers a mod and marks its StatType dirty - the next GetStat(type) call recalculates instead of returning a stale cached value.</summary>
		public void RegisterStatMod(IStatMod mod)
		{
			if (!statMods.TryGetValue(mod.StatType, out List<IStatMod> mods))
			{
				mods = new List<IStatMod>();
				statMods[mod.StatType] = mods;
			}
			mods.Add(mod);
			dirtyStats.Add(mod.StatType);
		}

		/// <summary>Removes one specific mod (reference equality) and marks its StatType dirty. See DeregisterSkill for removing everything a given source registered at once.</summary>
		public void DeregisterStatMod(IStatMod mod)
		{
			if (statMods.TryGetValue(mod.StatType, out List<IStatMod> mods) && mods.Remove(mod))
			{
				dirtyStats.Add(mod.StatType);
			}
		}

		/// <summary>
		/// The stat, as every currently-registered IStatMod modifies it - recalculated only if this
		/// StatType is marked dirty (a mod was registered/deregistered since the last read), otherwise
		/// this is a plain cached-value return. See EntityUtil.ApplyStatMods for the actual Add-then-
		/// Multiply-then-Exponent application order.
		/// </summary>
		public ddouble GetStat(StatType type)
		{
			if (dirtyStats.Remove(type))
			{
				ddouble raw = StatBlock.GetBase(type);
				ddouble computed = statMods.TryGetValue(type, out List<IStatMod> mods)
					? Util.Game.EntityUtil.ApplyStatMods(raw, mods)
					: raw;
				StatBlock.SetStat(type, computed);
			}
			return StatBlock.GetStat(type);
		}

		#endregion StatMods

		public IElement GetElement(ElementType type)
		{
			return ElementBlock.GetElement(type);
		}

		public IToken GetToken(TokenType type)
		{
			throw new NotImplementedException();
		}

		public IMileage GetMileage(MileageType type)
		{
			throw new NotImplementedException();
		}

		public int GetCounter(CounterType type) => counters.TryGetValue(type, out int value) ? value : 0;
		public void SetCounter(CounterType type, int value) => counters[type] = value;
		public void IncrementCounter(CounterType type, int amount = 1) => counters[type] = GetCounter(type) + amount;

		// None

		// Buff Manipulation


		// Entity Ability method
		public void ApplyBuff(IBuff buff)
		{
			// Same specific Buff (EqualType - Plan identity), not just the same 4-value BuffType, so a
			// second Poison doesn't collide with an unrelated Burn just because both are StatusBuffs.
			if (Buffs.Any(b => b.EqualType(buff)))
			{
				var existing = Buffs.First(b => b.EqualType(buff));
				if (existing.BuffType == BuffType.StatusBuff)
				{
					// Statuses stack in intensity (unlimited) per the design discussion - unlike Buffs
					// proper, which never stack, only refresh. CC-style statuses (Stun/Freeze/Paralyse)
					// don't come through here at all - those go through Resistance's own buildup/
					// threshold/reset model, not the Buffs list.
					// Stack (not AddStack) - combines Duration/Rank/Value via this Buff's own
					// BuffStackType (see StatusStackTypes' named archetypes: Fire/Poison/Debilitating/
					// Disease) AND bumps Stacks, so which numbers actually change on reapplication depends
					// on which archetype this StatusType was assigned, not always "just +1 stack."
					existing.Stack(buff);
				}
				else
				{
					// No stacking - "buffs of the same name combine such that the duration left
					// refreshes" per the design discussion. The newly-constructed `buff` argument is
					// discarded; the existing one just has its countdown reset, so anything already
					// wired to it (Effects, StatMod contributions) stays exactly as it was, only the
					// clock restarts. The generic BuffStackType/Stack(IBuff) machinery is still here for
					// whatever DOES want numeric stacking later (Rank-based buffs) - just not the
					// default path anymore.
					existing.Refresh();
				}
				RaiseEvent(TriggerType.OnBuffStacked, new TriggerContext { Entity = this, Expirable = existing });
			}
			else
			{
				// Add buff
				Buffs.Add(buff);
				// Wires this Buff's own Effects/Triggers (e.g. a periodic "spits out fireballs" Effect on
				// the granted BuffPlan) AND applies its own Passives (see Skill.RegisterCallbacks/
				// ApplyPassive) - RegisterSkills() at Spawn only ever covers what was on InitBuffs/
				// InitSkills from the start, not anything granted afterward (e.g. via
				// ApplyBuffActionHandler), so a dynamically-applied Buff needs this call itself.
				buff.RegisterCallbacks();
				// Auto-cleans itself up once its Duration runs out (see Entity.Tick -> Buff.Tick ->
				// Buff.Expire -> OnExpired) - the Entity that owns the Buffs list is what reacts to
				// expiry, not the Buff itself (it has no reference to that list).
				buff.OnExpired += HandleBuffExpired;
				// Mirrors the IsPositive branch CleanseBuff does on the way out.
				RaiseEvent(buff.IsPositive ? TriggerType.OnBuffApplied : TriggerType.OnDebuffApplied, new TriggerContext { Entity = this, Expirable = buff });
			}
		}

		void HandleBuffExpired(IExpirable expirable)
		{
			if (expirable is IBuff buff) CleanseBuff(buff);
		}

		public void CleanseBuff(IBuff buff)
		{
			if (Buffs.Any(b => b.EqualType(buff)))
			{
				// Remove
				var existing = Buffs.First(b => b.EqualType(buff));
				Buffs.Remove(existing);
				existing.OnExpired -= HandleBuffExpired;
				// Detaches its WrappedActions AND undoes its Passive contribution (see Skill.Deregister/
				// UnapplyPassive) - without this, a cleansed buff's Effects kept firing forever
				// (RegisterCallbacks with no matching Deregister on the way out).
				DeregisterSkill(existing);
				RaiseEvent(existing.IsPositive ? TriggerType.OnBuffExpired : TriggerType.OnDebuffCleansed, new TriggerContext { Entity = this, Expirable = existing });
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

		/// <summary>Which received-instance list a granted Aura belongs in - by who cast it (TowerEntity vs Monster), not by who's receiving it, per TowerAuras/MonsterAuras' own doc comment.</summary>
		List<IBuff> AuraListFor(IAura instance) => instance.Caster is TowerEntity ? TowerAuras : MonsterAuras;

		public void ApplyAuraInstance(IAura instance)
		{
			List<IBuff> list = AuraListFor(instance);
			if (list.Any(b => b.EqualType(instance)))
			{
				// Already have a copy of this exact Aura (e.g. a second source's identical Aura overlapping
				// range) - refresh rather than double-apply, same "same Plan = same slot" rule ApplyBuff uses.
				list.First(b => b.EqualType(instance)).Refresh();
				return;
			}

			list.Add((IBuff)instance);
			instance.RegisterCallbacks();
			instance.OnExpired += HandleAuraInstanceExpired;
		}

		void HandleAuraInstanceExpired(IExpirable expirable)
		{
			// Only reached via a linger countdown running out (Aura.Leave gives it a real Duration) -
			// while still in range an instance's Duration is 0 (permanent, see Aura's constructor), so
			// this never fires just from standing in the aura.
			if (expirable is IAura instance) RemoveAuraInstance(instance);
		}

		public void RemoveAuraInstance(IAura instance)
		{
			List<IBuff> list = AuraListFor(instance);
			if (!list.Remove((IBuff)instance)) return;

			instance.OnExpired -= HandleAuraInstanceExpired;
			DeregisterSkill(instance);
		}

		// STUB - placeholder tuning constant for EntityUtil.Mitigate's curve, not a balanced number.
		static readonly ddouble MitigationConstant = 100;

		public void ApplyDamage(Damage damage)
		{
			// Runs the full modifier tree (Player/Map/Game/Boss - see DamageCalculator) fresh for this
			// one hit before it touches Health. damage.Value itself is left untouched - Calculate reads
			// it, never mutates it - so nothing here changes if this same Damage object is ever applied
			// more than once (e.g. hitting multiple targets with one attack).
			ddouble incoming = DamageCalculator.Calculate(damage);

			// Defence mitigation, before any absorb - Magic damage (Tag.Magic, same "AttackTag" reading
			// the Nullifier line below already uses) mitigates against MagicResist; everything else
			// (untagged = Physical, per Tag.cs) against Defence. See EntityUtil.Mitigate for the curve;
			// MitigationConstant is a placeholder tuning number, not a balanced one.
			StatType defenceType = damage.HasTag(Tag.Magic) ? StatType.MagicResist : StatType.Defence;
			incoming = Util.Game.EntityUtil.Mitigate(incoming, GetStat(defenceType), MitigationConstant);

			// Absorb order: Nullifier (by damage School) first, then Shield, then whatever's left hits
			// Health - "first deplete Shield, then the remaining amount hits Health," with
			// DamageNullifier/SpellNullifier layered in front per the design discussion. All three are
			// Regenerable stats (see StatBlock.StatKinds), so they read/behave like extra health bars
			// that regen over time. AbsorbWith is a no-op if the entity never has that stat at all
			// (GetCurrent returns 0 without creating one), so an entity with no Shield/Nullifier falls
			// straight through to Health exactly as before this change.
			StatType nullifierType = damage.HasTag(Tag.Magic) ? StatType.SpellNullifier : StatType.DamageNullifier;
			incoming = AbsorbWith(nullifierType, incoming);
			incoming = AbsorbWith(StatType.Shield, incoming);

			if ((double)incoming > 0) StatBlock.Health.Deplete(incoming);

			// What actually got through, after every layer - see Damage.FinalValue for why this (not
			// Value) is what a reflect/mirror effect should read.
			damage.SetFinalValue(incoming);
		}

		/// <summary>Depletes up to `incoming` from absorberType's Current, returning whatever's left over. Safe to call on a stat the entity has never touched - GetCurrent/Deplete both no-op rather than creating one.</summary>
		ddouble AbsorbWith(StatType absorberType, ddouble incoming)
		{
			if ((double)incoming <= 0) return incoming;

			ddouble available = StatBlock.GetCurrent(absorberType);
			if ((double)available <= 0) return incoming;

			ddouble absorbed = (double)available < (double)incoming ? available : incoming;
			StatBlock.Deplete(absorberType, absorbed);
			return incoming - absorbed;
		}

		public void Recalculate(List<StatType> types, List<double> values)
		{
			throw new NotImplementedException();
		}

		// Tags
		public void AddTag(Tag tag)
		{
			if (!Tags.Contains(tag)) Tags.Add(tag);
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
			return Tags.Contains(tag);
		}

		public void Hurt(ddouble value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Wires a single skill's (or buff's - Buff : ISkill) triggers onto this entity.
		/// The actual Trigger -> WrappedAction -> EffectController wiring lives on the skill itself
		/// (see Skill.RegisterCallbacks), since a skill knows its own Plan/Caster.
		/// </summary>
		public void RegisterSkill(ISkill skill)
		{
			skill.RegisterCallbacks();
		}

		/// <summary>
		/// The root deregistration method - the mirror of RegisterSkill, and the one thing CleanseBuff/
		/// unlearning/etc. should call rather than skill.Deregister() directly, so there's exactly one
		/// place that means "this Entity is done with this Skill/Buff/Aura."
		/// </summary>
		public void DeregisterSkill(ISkill skill)
		{
			skill.Deregister();
		}

		/// <summary>
		/// Registers - and, via each one's own RegisterCallbacks -&gt; ApplyPassive, activates - every
		/// ability this Entity starts with: Skills, Buffs, Statuses, and any Tower/MonsterAuras it
		/// already holds. Called once, from Spawn() - there's no separate "recalculate all Passives"
		/// pass anymore; each Skill/Buff/Status/Aura applies only its own the moment it's registered
		/// (here, or later via ApplyBuff for anything granted mid-game), and undoes only its own via
		/// Deregister (CleanseBuff, or unlearning). See Skill.ApplyPassive/UnapplyPassive.
		/// </summary>
		public void RegisterSkills()
		{
			foreach (ISkill skill in Skills) RegisterSkill(skill);
			foreach (IBuff buff in Buffs) RegisterSkill(buff);
			foreach (StatusBuff status in Statuses) RegisterSkill(status);
			foreach (IBuff aura in TowerAuras) RegisterSkill(aura);
			foreach (IBuff aura in MonsterAuras) RegisterSkill(aura);
		}

		public void RemoveTag(Tag tag)
		{
			Tags.Remove(tag);
		}
		#endregion Methods

		#region Dipose

		#endregion Dispose

		#region Admin

		public void ForceSetStat(StatType type, ddouble value)
		{
			StatBlock.SetStat(type, value);
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
		// Stat - sparse, design-time data (see StatEntry/StatusEntry/ElementEntry). An Entity built from
		// this Plan constructs its own fresh StatBlock/ElementBlock from these rather than sharing the
		// Plan's - two Entities from the same Plan must never share one Health pool.
		ddouble StartingHealth { get; }
		List<StatEntry> StatEntries { get; }
		List<StatusEntry> StatusEntries { get; }
		List<ElementEntry> ElementEntries { get; }
		ResourceBlock ResourceBlock { get; }

		// Initial
		List<IToken> StartingTokens { get; }
		List<BuffPlan> InitBuffs { get; }
		List<SkillPlan> InitSkills { get; }
	}

	// STUB: real values, no transition logic driving them yet - nothing currently calls
	// Entity.EntityStateChange automatically (attacking/being hit/using a skill don't set state on
	// their own). Answers the old "how do I let this contain ANY property" TODO: a plain enum, not a
	// bag of bools - an Entity is in exactly one of these at a time.
	public enum EntityState
	{
		Idle,
		Attacking,
		UnderAttack,
		UsingSkill,
		OutOfRange,
	}
}