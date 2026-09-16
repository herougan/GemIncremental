using System;
using System.Collections.Generic;
using System.Linq;
using Util.Debug;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Skills.Keywords;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Stats
{
	#region Interfaces & Classes
	// ===== Interfaces =====
	public interface IStat
	{
		/// <summary>
		/// Gets or sets the base value of the stat.
		/// Base cannot be changed.
		/// </summary>
		public ddouble Base { get; }

		/// <summary>
		/// Gets or sets the type of the stat.
		/// </summary>
		public StatType StatType { get; }

		/// <summary>
		/// Gets or sets the dynamic value of the stat.
		/// This dynamic value can be used to store the calculated values that take in Modifiers and such.
		/// This value is not used internally in classes that implement this interface.
		/// Dynamic divided by Value gives the scaling factor of the stat.
		/// </summary>
		public ddouble Value { get; }

		/// <summary>
		/// Occurs when the stat value changes through new Mods (dynamic changes from current mods don't trigger this)
		/// </summary>
		public event Action<IStat, ddouble> OnValueChanged;


		/// <summary>
		/// Resets value to base value and re-scales it by the specified factor.
		/// </summary>
		/// <param name="scale">The factor by which to scale the stat value.</param>
		public void Recalculate(ddouble scale);
		public void AddScale(ddouble scale);
		public void SetScale(ddouble scale);
		public void AddValue(ddouble value);
		public void SetValue(ddouble value);
	}

	[Serializable]
	public class Stat : IStat
	{
		public ddouble Base
		{
			get { return _Base; }
			set
			{
				if (_Base != value) OnValueChanged?.Invoke(this, value);
				_Base = value;
			}
		}
		[FormerlySerializedAs("Base")]
		[SerializeField] protected ddouble _Base;

		public StatType StatType { get { return _StatType; } set { _StatType = value; } }
		[FormerlySerializedAs("StatType")]
		[SerializeField] protected StatType _StatType;

		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnValueChanged?.Invoke(this, value);
				_Value = value;
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] protected ddouble _Value;

		public ddouble Scale => Value / Base; // Dynamic divided by Value gives the scaling factor of the stat

		public void Recalculate(ddouble scale)
		{
			Value = Base;
			Value *= scale;
		}

		public void AddScale(ddouble scale)
		{
			Value = (scale + this.Scale) * Base;
		}

		public void SetValue(ddouble value)
		{
			Value = value;
		}

		public void SetScale(ddouble scale)
		{
			Value = scale * Base;
		}

		public void AddValue(ddouble value)
		{
			Value += value;
		}

		/// <summary>
		/// Triggers with (itself, and value it changed to)
		/// </summary>
		public event Action<IStat, ddouble> OnValueChanged = delegate { };

		public Stat(StatType statType, ddouble value = default(ddouble))
		{
			StatType = statType;
			// Both, not just Value - a freshly-constructed stat has no mods yet, so Base and Value start
			// equal. Without this, GetBase(type) silently returned 0 for every stat instead of what it
			// was actually constructed with - a real bug that would have broken EntityManager's spawn-
			// time wave scaling and every IStatMod recompute (both read Base as the "no mods applied"
			// starting point) the moment either touched a stat that hadn't already gone through it.
			Base = value;
			Value = value;
		}

		public Stat(Stat original)
		{
			StatType = original.StatType;
			Value = original.Value;
		}
	}

	[Serializable]
	public class ImmutableStat : Stat
	{
		public new void Recalculate(ddouble scale)
		{
			// Cannot Scale
			LogManager.Instance.LogWarning($"Cannot scale ImmutableStat {StatType}");
			return;
		}

		public ImmutableStat(StatType type, ddouble value = default(ddouble)) : base(type, value)
		{
			Base = value; // ImmutableStat's Base is the same as Value
		}
	}

	public interface IDepletable : IStat
	{
		public ddouble Current { get; }
		public void SetMax(ddouble value);
		public void Deplete(ddouble value);
		public event Action<IDepletable, ddouble> OnCurrentValueDecreased;
		public event Action<IDepletable, ddouble> OnCurrentValueIncreased;
	}

	[Serializable]
	public class DepletableStat : Stat, IDepletable
	{
		/// <summary>
		///  Sets the <paramref name="value"/> parameter. This value represents the maximum value. 
		/// </summary>
		/// <param name="value"></param>
		public void SetMax(ddouble value)
		{
			// if (Value != value) OnValueChanged?.Invoke(this, value);
			Value = value;
		}

		/// <summary>
		/// Depletes the current value by the specified <paramref name="value"/> parameter.
		/// </summary>
		/// <param name="value"></param>
		public void Deplete(ddouble value)
		{

			Current = Math.Max(0, Current - value);
		}

		public ddouble Current
		{
			get { return _Current; }
			set
			{
				if (_Current > value) OnCurrentValueDecreased?.Invoke(this, _Current - value);
				else if (_Current < value) OnCurrentValueIncreased?.Invoke(this, value - _Current);
				_Current = value;
			}
		}
		[FormerlySerializedAs("Current")]
		[SerializeField] protected ddouble _Current;

		// Events
		public event Action<IDepletable, ddouble> OnCurrentValueDecreased = delegate { };
		public event Action<IDepletable, ddouble> OnCurrentValueIncreased = delegate { };

		// Constructor
		public DepletableStat(StatType statType, ddouble value, ddouble current) : base(statType, value)
		{
			StatType = statType;
			Value = value;
			Current = current;
		}

		public DepletableStat(StatType statType, ddouble value = default(ddouble)) : base(statType, value)
		{
			StatType = statType;
			Value = value;
			Current = Value;
		}

		public DepletableStat(DepletableStat original) : base(original.StatType, original.Value)
		{
			Current = original.Current;
		}
	}

	public interface IRegenerable : IDepletable
	{
		public ddouble Regeneration { get; }
		public float Rate { get; }
		//
		public event Action<IRegenerable, ddouble> OnRegenerate;
		public event Action<IRegenerable, ddouble> OnRest;
		public event Action<IRegenerable, ddouble> OnRegenValueChanged;
		public event Action<IRegenerable, float> OnRegenRateChanged;
		//
		public void Regenerate();
		public void Rest(); // Regenerates faster, and greater; at least 1 point if 0
		public void Tick(float time);
		public void SetRate(float value);
		public void SetRegeneration(ddouble value);
	}

	[Serializable]
	public class RegenerableStat : DepletableStat, IRegenerable
	{
		// Properties
		public ddouble Regeneration
		{
			get { return _Regeneration; }
			set
			{
				if (_Regeneration != value) OnRegenValueChanged?.Invoke(this, _Regeneration);
				_Regeneration = value;
			}
		}
		[FormerlySerializedAs("Regeneration")]
		[SerializeField] protected ddouble _Regeneration;

		public float Rate
		{
			get { return _Rate; }
			set
			{
				if (Math.Abs(_Rate - value) > float.Epsilon)
				{
					OnRegenRateChanged?.Invoke(this, value);
					_Rate = value;
				}
			}
		}
		[FormerlySerializedAs("Rate")]
		[SerializeField] protected float _Rate;

		public event Action<IRegenerable, ddouble> OnRegenerate = delegate { };
		public event Action<IRegenerable, ddouble> OnRest = delegate { };
		public event Action<IRegenerable, ddouble> OnRegenValueChanged = delegate { };
		public event Action<IRegenerable, float> OnRegenRateChanged = delegate { };


		// Constructor
		public RegenerableStat(StatType statType, ddouble value, ddouble current, ddouble regeneration) : base(statType, value, current)
		{
			StatType = statType;
			Value = value;
			Current = current;
			Regeneration = regeneration;
		}

		public RegenerableStat(StatType statType, ddouble value = default(ddouble)) : base(statType, value)
		{
			StatType = statType;
			Value = value;
			Current = Value;
			Regeneration = 0;
		}

		public RegenerableStat(RegenerableStat original) : base(statType: original.StatType, value: original.Value, current: original.Current)
		{
			StatType = original.StatType;
			Value = original.Value;
			Current = original.Current;
			Regeneration = original.Regeneration;
		}

		// Methods
		public void Regenerate()
		{
			// Invoke Event
			ddouble regen = Math.Min(Regeneration, Math.Max(0, Value - Current));
			if (regen > 0)
			{
				OnRegenerate?.Invoke(this, regen);
				// Regenerate, Capped at Max health
				Current += regen;
			}
		}

		public void Rest()
		{
			// Invoke Event
			ddouble regen = Math.Min(2 * Regeneration, Math.Max(0, Value - Current));
			if (regen > 0)
			{
				OnRest?.Invoke(this, regen);
				// Regenerate, Capped at Max health
				Current += regen;
			}
		}

		private float accumulatedTime = 0f;
		public void Tick(float time)
		{
			accumulatedTime += time;
			if (time > Rate)
			{
				Regenerate();
				accumulatedTime = 0f;
			}
		}
		public void SetRate(float value)
		{
			Rate = value;
		}

		public void SetRegeneration(ddouble value)
		{
			Regeneration = value;
		}
	}

	public interface IResistance : IStat
	{
		public StatusType Status { get; }
		public ddouble Resist { get; set; }
		public ddouble Mastery { get; set; }
		public ddouble Threshold { get; set; }
		public event Action<IResistance, ddouble> OnResistChanged;
		public event Action<IResistance, ddouble> OnMasteryChanged;
		public event Action<IResistance, ddouble> OnThresholdCrossed;
		public event Action<IResistance, ddouble> OnThresholdChanged;
	}

	[Serializable]
	public class Resistance : Stat, IResistance
	{
		// Cooky hack (see IStat)
		[FormerlySerializedAs("Status")]
		[SerializeField] private StatusType _Status;
		public StatusType Status { get { return _Status; } set { _Status = value; } }

		[FormerlySerializedAs("Resist")]
		[SerializeField] private ddouble _Resist;
		public ddouble Resist
		{
			get { return _Resist; }
			set
			{
				if (_Resist != value) OnResistChanged?.Invoke(this, value);
				_Resist = value;
			}
		}

		[FormerlySerializedAs("Mastery")]
		[SerializeField] private ddouble _Mastery;
		public ddouble Mastery
		{
			get { return _Mastery; }
			set
			{
				if (_Mastery != value) OnMasteryChanged?.Invoke(this, value);
				_Mastery = value;
			}
		}

		[FormerlySerializedAs("Threshold")]
		[SerializeField] private ddouble _Threshold;
		public ddouble Threshold
		{
			get { return _Threshold; }
			set
			{
				if (_Threshold != value) OnThresholdChanged?.Invoke(this, value);
				_Threshold = value;
			}
		}

		public Resistance(StatusType status, ddouble value = default(ddouble), ddouble resist = default(ddouble), ddouble threshold = default(ddouble)) : base(StatType.Status, value)
		{
			Status = status;
			Value = value;
			Resist = resist;
			Threshold = threshold;
		}

		// event Action<IStat, ddouble> IStat.OnValueChanged
		// {
		// 	add
		// 	{
		// 		throw new NotImplementedException();
		// 	}

		// 	remove
		// 	{
		// 		throw new NotImplementedException();
		// 	}
		// }
		public event Action<IResistance, ddouble> OnResistChanged;
		public event Action<IResistance, ddouble> OnMasteryChanged;
		public event Action<IResistance, ddouble> OnThresholdCrossed;
		public event Action<IResistance, ddouble> OnThresholdChanged;

		// ===== Methods =====

		public void AddStatus(ddouble amount)
		{
			if (amount <= 0) return;
			Value += amount * (1 - Resist);
			//
			if (Value >= Threshold)
			{
				OnThresholdCrossed?.Invoke(this, Value);
				Reset();
			}
		}

		public void Reset()
		{
			Value = 0;
		}
	}

	#endregion Interfaces & Classes

	#region Enums
	// ====== Enum ======
	public enum StatType
	{
		// Basic
		Health,
		Mana,
		Attack,
		MagicAttack,
		Defence,
		MagicResist,
		Shield,
		Energy,
		// Promoted to stats (not the Passives/Skill system) specifically so they get a depletable "bar"
		// like Health/Shield's - see ApplyDamage's absorb order (Nullifier -> Shield -> Health).
		DamageNullifier, // absorbs incoming Physical damage
		SpellNullifier,  // absorbs incoming Magical damage
		// Defensive
		ArmourHealth,
		ArmourDefence,
		CritResist,
		DodgeChance,
		BlockChance,
		BlockDefence,
		SpellCounter,
		SpellCounterResist,
		Thorns,
		SpellThorns,
		Reflect,

		// Anti-Defensive
		IgnoreBlock,
		DefenceReduction,
		MagicResistReduction,
		ArmourPenetration, // %Extra Damage on armoured opponents - to Armour
		ShieldPenetration, // %Extra Damage on shielded opponents - to Shield & Health (While Shielded)
		AttackReduction,

		// Attack & Critical
		AttackSpeed,
		CritChance,
		CritDamage,
		Accuracy,
		FinalAttack,
		ExtraAttackChance,

		// Advantage
		Lifesteal,
		SkillVamp,
		ManaCostReduction,
		EnergyCostReduction,
		CooldownReduction,
		AbilityPower, // Chance to use special ability

		// Physics
		TurnSpeed,
		Speed,
		BulletSpeed,
		Range,
		RangeCloak,
		AoEResist, // Reduces range of AoE attacks
		ArcOfFire,

		// Meta
		Level,
		Difficulty,
		Reward,
		Cost,

		// Status Effect
		StunResist,
		PoisonResist,
		FreezeResist,
		SlowResist,
		BlindResist,
		KnockbackResist, // %, as it triggers, % requirement increases


		// Other Stat Types
		Element,
		Status,
		Resource,
		Mileage,
	}

	public enum StatCategory
	{
		Offensive,
		Defensive,
		Utility, //???
	}

	public enum StatMetaType
	{
		Basic,
		Immutable,
		Regenerable,
		Depletable,
	}


	#endregion Enums

	#region Plan Data

	/// <summary>
	/// Design-time entry so a Plan (MonsterPlan/TowerPlan) can hand StatBlock a sparse list of starting
	/// stats without StatMap itself (a Dictionary) needing to be Unity-serializable. Health is not
	/// represented here - it's always eagerly present on StatBlock and configured separately.
	/// </summary>
	[Serializable]
	public struct StatEntry
	{
		public StatType Type;
		public double Value;
	}

	/// <summary>
	/// Design-time entry for a starting status Resistance (see StatEntry).
	/// </summary>
	[Serializable]
	public struct StatusEntry
	{
		public StatusType Type;
		public double Resist;
		public double Threshold;
	}

	#endregion Plan Data

	#region Blocks

	[Serializable]
	public class StatBlock
	{
		#region Stats

		/// <summary>
		/// Every stat besides Health is lazy: it does not exist in StatMap until something actually
		/// Adds/Sets/Depletes it. Reading a stat that was never touched (GetStat) returns 0 rather than
		/// creating a wasted Stat object - see GetStat/GetOrCreate.
		/// </summary>
		public Dictionary<StatType, IStat> StatMap { get; protected set; } = new Dictionary<StatType, IStat>();
		public Dictionary<StatusType, Resistance> StatusMap { get; protected set; } = new Dictionary<StatusType, Resistance>();

		/// <summary>
		/// Which concrete Stat subclass a given StatType should be instantiated as, the first time it's
		/// lazily created. Anything not listed here is a plain Stat.
		/// </summary>
		private static readonly Dictionary<StatType, StatMetaType> StatKinds = new Dictionary<StatType, StatMetaType>
		{
			{ StatType.Health, StatMetaType.Depletable }, // Documented here too, though Health itself bypasses StatMap - see Health field
			{ StatType.Mana, StatMetaType.Depletable },
			{ StatType.Energy, StatMetaType.Depletable },
			{ StatType.ArmourHealth, StatMetaType.Depletable },
			{ StatType.Shield, StatMetaType.Regenerable },
			{ StatType.DamageNullifier, StatMetaType.Regenerable },
			{ StatType.SpellNullifier, StatMetaType.Regenerable },
			{ StatType.Level, StatMetaType.Immutable },
			{ StatType.Difficulty, StatMetaType.Immutable },
		};

		/// <summary>
		/// Base value a StatType is lazily created with if a Plan never gave it an explicit StatEntry -
		/// everything not listed here defaults to 0 (ddouble's own default), same as before this
		/// existed. Only add an entry where 0 would be a wrong/dangerous default - e.g. 0 Accuracy would
		/// silently mean "always misses," which nothing authors on purpose.
		/// </summary>
		private static readonly Dictionary<StatType, ddouble> StatDefaults = new Dictionary<StatType, ddouble>
		{
			{ StatType.Accuracy, 100 },
		};

		private static IStat CreateStat(StatType type, ddouble value)
		{
			StatMetaType kind = StatKinds.TryGetValue(type, out var k) ? k : StatMetaType.Basic;
			return kind switch
			{
				StatMetaType.Depletable => new DepletableStat(type, value),
				StatMetaType.Regenerable => new RegenerableStat(type, value),
				StatMetaType.Immutable => new ImmutableStat(type, value),
				_ => new Stat(type, value),
			};
		}

		// Health is the one stat every entity must have valid at all times (it's what "alive" means),
		// so unlike everything else it's a real eager field, guaranteed >= 1. See constructors.
		[Header("Basic Stats")]
		public DepletableStat Health; // Health is NOT inherently regenerable

		#endregion Stats

		#region Status

		[Header("Status")]
		public List<IResistance> Resistances => StatusMap.Values.Cast<IResistance>().ToList();

		#endregion Status

		#region Events

		/*
		Value: Main value of a stat, also represents maximum value for depletables.
		Current: Current value of a depletable.
		RegenValue: Value to be added onto Current every regenerate
		RegenRate: Rate at which stat regenerates
		*/

		// General
		public event Action<IStat, ddouble> OnValueChanged = delegate { };
		public event Action<IStat, ddouble> OnValueDecreased = delegate { };
		public event Action<IStat, ddouble> OnValueIncreased = delegate { };
		// Depletable
		public event Action<IDepletable, ddouble> OnMaxValueChanged = delegate { };
		public event Action<IDepletable, ddouble> OnCurrentValueDecreased = delegate { };
		public event Action<IDepletable, ddouble> OnCurrentValueIncreased = delegate { };
		// Bonus
		public event Action<IStat, IStatMod> OnStatBonusAdded = delegate { };
		public event Action<IStat, IStatMod> OnStatNerfAdded = delegate { };
		// Regenerable
		public event Action<IRegenerable, ddouble> OnRegenerate = delegate { };
		public event Action<IRegenerable, ddouble> OnRest = delegate { };
		public event Action<IRegenerable, ddouble> OnRegenValueChanged = delegate { };
		public event Action<IRegenerable, ddouble> OnRegenRateChanged = delegate { };

		// Resist
		public event Action<IResistance, ddouble> OnResistChanged;
		public event Action<IResistance, ddouble> OnMasteryChanged;
		public event Action<IResistance, ddouble> OnThresholdCrossed;
		public event Action<IResistance, ddouble> OnThresholdChanged;

		// Element
		// public event Action<ElementType, ddouble> OnElementExplosion = delegate { };
		// public event Action<ElementType, ddouble> OnElementSynergy = delegate { };

		/// <summary>
		/// Wires Health's callbacks into this block's bubbled events. Health is the only stat that
		/// bypasses GetOrCreate (see field above), so it's the only one that doesn't already get
		/// registered the moment it's created - everything else is wired inline by GetOrCreate/AddStatus
		/// at the point it's first lazily instantiated.
		/// </summary>
		public void RegisterCallbacks()
		{
			RegisterStatCallbacks(Health);
			RegisterDepletableCallbacks(Health);
		}

		public void RegisterResistanceCallbacks(IResistance resistance)
		{
			resistance.OnResistChanged += (r, v) => OnResistChanged?.Invoke(r, v);
			resistance.OnMasteryChanged += (r, v) => OnMasteryChanged?.Invoke(r, v);
			resistance.OnThresholdCrossed += (r, v) => OnThresholdCrossed?.Invoke(r, v);
			resistance.OnThresholdChanged += (r, v) => OnThresholdChanged?.Invoke(r, v);
		}

		public void RegisterRegenerableCallbacks(IRegenerable regenerable)
		{
			regenerable.OnRegenerate += (s, v) => OnRegenerate?.Invoke(s, v);
			regenerable.OnRest += (s, v) => OnRest?.Invoke(s, v);
			regenerable.OnRegenValueChanged += (s, v) => OnRegenValueChanged?.Invoke(s, v);
			regenerable.OnRegenRateChanged += (s, v) => { OnRegenRateChanged?.Invoke(s, v); };

			// IRegenerables are IDepletables
			// RegisterDepletableCallbacks(regenerable);
		}

		public void RegisterDepletableCallbacks(IDepletable depletable)
		{
			depletable.OnValueChanged += (s, v) => OnMaxValueChanged?.Invoke((IDepletable)s, v); // Value = MaxValue, CurrentValue = Current
			depletable.OnCurrentValueDecreased += (s, v) => OnCurrentValueDecreased?.Invoke(s, v);
			depletable.OnCurrentValueIncreased += (s, v) => OnCurrentValueIncreased?.Invoke(s, v);
		}

		public void RegisterStatCallbacks(IStat stat)
		{
			stat.OnValueChanged += (s, v) =>
							{
								OnValueChanged?.Invoke(s, v);
								if (v < s.Value) OnValueDecreased?.Invoke(s, v);
								if (v > s.Value) OnValueIncreased?.Invoke(s, v);
							};
		}

		#endregion Events

		#region Constructor

		/// <summary>
		/// Only Health is ever eagerly created. Everything else - the ~40 combat stats, elements,
		/// statuses - stays out of StatMap entirely until something Adds/Sets/Depletes it.
		/// Health must always be a valid, living value: non-positive/unset input is clamped to 1.
		/// </summary>
		public StatBlock(ddouble health = default(ddouble))
		{
			if (health <= 0) health = 1;
			Health = new DepletableStat(StatType.Health, health);
			RegisterCallbacks();
		}

		/// <summary>
		/// Builds a StatBlock from a Plan's sparse, Inspector-authored data (see StatEntry/StatusEntry).
		/// Only the stats/statuses actually listed get instantiated - nothing is created speculatively.
		/// </summary>
		public StatBlock(ddouble health, IEnumerable<StatEntry> statEntries, IEnumerable<StatusEntry> statusEntries = null) : this(health)
		{
			if (statEntries != null)
			{
				foreach (StatEntry entry in statEntries)
				{
					if (entry.Type == StatType.Health) continue; // Health is configured via the health parameter above
					// Both - GetOrCreate(entry.Type) here first constructs the stat with StatDefaults'
					// value (or 0), not entry.Value, so SetStat alone would leave Base stuck at that
					// construction-time default forever. SetBase makes entry.Value the real starting
					// point for later IStatMod recomputes (see Entity.GetStat) to build on.
					SetBase(entry.Type, entry.Value);
					SetStat(entry.Type, entry.Value);
				}
			}
			if (statusEntries != null)
			{
				foreach (StatusEntry entry in statusEntries)
				{
					IResistance resistance = GetOrCreateStatus(entry.Type);
					resistance.Resist = entry.Resist;
					resistance.Threshold = entry.Threshold;
				}
			}
		}

		#endregion Constructor

		#region Lifecycle

		public void Regenerate()
		{
			foreach (IRegenerable stat in GetRegenerableStats())
			{
				stat.Regenerate();
			}
		}

		#endregion Lifecycle

		#region Methods

		/// <summary>
		/// Gets the stat object, lazily creating (and wiring the callbacks for) one if it doesn't exist
		/// yet. Internal - callers that only want a number should use GetStat/GetCurrent instead, which
		/// never create anything.
		/// </summary>
		private IStat GetOrCreate(StatType type)
		{
			if (type == StatType.Health) return Health;
			if (!StatMap.TryGetValue(type, out IStat stat))
			{
				ddouble defaultValue = StatDefaults.TryGetValue(type, out var d) ? d : default(ddouble);
				stat = CreateStat(type, defaultValue);
				StatMap[type] = stat;
				RegisterStatCallbacks(stat);
				if (stat is IDepletable depletable) RegisterDepletableCallbacks(depletable);
				if (stat is IRegenerable regenerable) RegisterRegenerableCallbacks(regenerable);
			}
			return stat;
		}

		private Resistance GetOrCreateStatus(StatusType type)
		{
			if (!StatusMap.TryGetValue(type, out Resistance resistance))
			{
				resistance = new Resistance(type);
				StatusMap[type] = resistance;
				RegisterResistanceCallbacks(resistance);
			}
			return resistance;
		}

		// Adders
		/// <summary>
		/// Inserts a pre-built stat object directly (e.g. from deserialisation). Prefer AddStat(type, value)
		/// for the common "add this much" case, which lazily creates the right concrete Stat subtype itself.
		/// </summary>
		public void AddStat(IStat stat)
		{
			if (stat == null)
			{
				LogManager.Instance.LogError("Cannot add null stat!");
				return;
			}
			if (StatMap.ContainsKey(stat.StatType))
			{
				LogManager.Instance.LogWarning($"Stat {stat.StatType} already exists in StatBlock!");
				return;
			}
			StatMap[stat.StatType] = stat;
			RegisterStatCallbacks(stat);
			if (stat is IDepletable depletable) RegisterDepletableCallbacks(depletable);
			if (stat is IRegenerable regenerable) RegisterRegenerableCallbacks(regenerable);
		}

		/// <summary>
		/// Adds to a stat's value, creating it (at 0) first if it doesn't exist yet.
		/// </summary>
		public void AddStat(StatType type, ddouble value)
		{
			GetOrCreate(type).AddValue(value);
		}

		/// <summary>
		/// Forces a stat's value, creating it first if it doesn't exist yet.
		/// </summary>
		public void SetStat(StatType type, ddouble value)
		{
			GetOrCreate(type).SetValue(value);
		}

		/// <summary>
		/// Sets a stat's raw Base directly - for *permanent* scaling, e.g. EntityManager's spawn-time
		/// wave-progression scaling, so a Passive applied later (see Skill.ApplyPassive, which reads
		/// Base to compute its own contribution) builds on top of it correctly. IStat.Base has no public
		/// setter (only the concrete Stat class does), so this reaches past the interface rather than
		/// adding one - every concrete stat type here already extends Stat.
		/// </summary>
		public void SetBase(StatType type, ddouble value)
		{
			if (GetOrCreate(type) is Stat stat) stat.Base = value;
		}

		/// <summary>
		/// Depletes a stat's Current value (e.g. Mana/Health), creating it first if it doesn't exist yet.
		/// No-ops with a warning if the StatType isn't a depletable kind.
		/// </summary>
		public void Deplete(StatType type, ddouble amount)
		{
			if (GetOrCreate(type) is IDepletable depletable) depletable.Deplete(amount);
			else LogManager.Instance.LogWarning($"Stat {type} is not depletable.");
		}

		public void ModifyStat(StatType type, IStatMod mod)
		{
			IStat stat = GetOrCreate(type);
			stat.SetValue(MathsLib.Operate(stat.Value, mod.Value, mod.Operation));
			if (mod.IsPositive)
				OnStatBonusAdded?.Invoke(stat, mod);
			if (!mod.IsPositive)
				OnStatNerfAdded?.Invoke(stat, mod);
			LogManager.Instance.Log($"Modified stat {type} by {mod.Value} ({mod.Operation}) to {stat.Value}");
		}

		public void RemoveStat(IStat stat)
		{
			if (stat == null)
			{
				LogManager.Instance.LogError("Cannot remove null stat!");
				return;
			}
			if (!StatMap.ContainsKey(stat.StatType))
			{
				LogManager.Instance.LogWarning($"Stat {stat.StatType} does not exist in StatBlock!");
				return;
			}
			StatMap.Remove(stat.StatType);
		}

		/// <summary>
		/// Adds status buildup, creating the Resistance (at its default Resist/Threshold) first if it
		/// doesn't exist yet.
		/// </summary>
		public void AddStatus(StatusType type, ddouble amount)
		{
			GetOrCreateStatus(type).AddStatus(amount);
		}

		public void AddRegenerable(StatType type, ddouble value = default(ddouble))
		{
			IStat stat = GetOrCreate(type);
			if (stat is IRegenerable regenerable) regenerable.AddValue(value);
			else LogManager.Instance.LogWarning($"Stat {type} is not regenerable.");
		}

		// Getters
		public List<IDepletable> GetDepletableStats()
		{
			return StatMap.Values.OfType<IDepletable>().Prepend(Health).ToList();
		}

		public List<IRegenerable> GetRegenerableStats()
		{
			return StatMap.Values.OfType<IRegenerable>().ToList();
		}

		/// <summary>
		/// Returns the stat's current Value, or 0 if it was never created - never creates it.
		/// </summary>
		public ddouble GetStat(StatType type)
		{
			if (type == StatType.Health) return Health.Value;
			return StatMap.TryGetValue(type, out IStat stat) ? stat.Value : default(ddouble);
		}

		/// <summary>
		/// Returns a stat's raw Base (pre-Passive, pre-mod), or 0 if it was never created - never
		/// creates it. What Skill.ApplyPassive reads before computing its own contribution.
		/// </summary>
		public ddouble GetBase(StatType type)
		{
			if (type == StatType.Health) return Health.Base;
			return StatMap.TryGetValue(type, out IStat stat) ? stat.Base : default(ddouble);
		}

		/// <summary>
		/// Returns a depletable stat's Current value, or 0 if it was never created - never creates it.
		/// </summary>
		public ddouble GetCurrent(StatType type)
		{
			if (type == StatType.Health) return Health.Current;
			if (StatMap.TryGetValue(type, out IStat stat) && stat is IDepletable depletable) return depletable.Current;
			return default(ddouble);
		}

		/// <summary>
		/// Returns a status's current buildup Value, or 0 if it was never created - never creates it.
		/// Named distinctly from Entity.GetStatus, which returns the IResistance object itself.
		/// </summary>
		public ddouble GetStatusValue(StatusType type)
		{
			return StatusMap.TryGetValue(type, out Resistance resistance) ? resistance.Value : default(ddouble);
		}

		public bool IfStatExists(StatType type)
		{
			return type == StatType.Health || StatMap.ContainsKey(type);
		}

		public List<IStat> GetStats()
		{
			return StatMap.Values.Prepend((IStat)Health).ToList();
		}

		// ===== Time =====
		public void Tick(float time)
		{
			foreach (var stat in GetRegenerableStats())
			{
				// stat.Regenerate();
				stat.Tick(time);
			}
		}

		#endregion Methods
	}

	#endregion Blocks
}