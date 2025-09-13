using System;
using System.Collections.Generic;
using System.Linq;
using Debug;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Keywords;
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
			Value *= scale;
		}

		public void SetValue(ddouble value)
		{
			Value = value;
		}

		/// <summary>
		/// Triggers with (itself, and value it changed to)
		/// </summary>
		public event Action<IStat, ddouble> OnValueChanged = delegate { };

		public Stat(StatType statType, ddouble value = default(ddouble))
		{
			StatType = statType;
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
		public event Action<IStat, ddouble> OnCurrentValueDecreased;
		public event Action<IStat, ddouble> OnCurrentValueIncreased;
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
		public event Action<IStat, ddouble> OnCurrentValueDecreased = delegate { };
		public event Action<IStat, ddouble> OnCurrentValueIncreased = delegate { };

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
		public event Action<IStat, ddouble> OnRegenerate;
		public event Action<IStat, ddouble> OnRegenValueChanged;
		public event Action<IStat, float> OnRegenRateChanged;
		//
		public void Regenerate();
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

		public event Action<IStat, ddouble> OnRegenerate = delegate { };
		public event Action<IStat, ddouble> OnRegenValueChanged = delegate { };
		public event Action<IStat, float> OnRegenRateChanged = delegate { };


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

	#region Blocks

	[Serializable]
	public class StatBlock
	{
		#region Stats

		// Dict
		public Dictionary<StatType, IStat> StatMap { get; protected set; } = new Dictionary<StatType, IStat>();


		// Trackers
		public List<IStat> Stats;

		[Header("Basic Stats")]
		public DepletableStat Health; // Health is NOT inherently regenerable
		public DepletableStat Mana;
		public Stat Attack;
		public Stat MagicAttack;
		public Stat Defence;
		public Stat MagicResist;
		public RegenerableStat Shield;
		public DepletableStat Energy;
		// Defensive
		[Header("Defensive Stats")]
		public DepletableStat ArmourHealth;
		public Stat ArmourDefence;
		public Stat CritResist;
		public Stat DodgeChance;
		public Stat BlockChance;
		public Stat BlockDefence;
		public Stat SpellCounter;
		public Stat SpellCounterResist;
		public Stat Thorns;
		public Stat SpellThorns;
		public Stat Reflect;

		// Anti-Defensive
		[Header("Anti-Defensive Stats")]
		public Stat IgnoreBlock;
		public Stat DefenceReduction;
		public Stat MagicResistReduction;
		public Stat ArmourPenetration;
		public Stat ShieldPenetration;
		public Stat AttackReduction;

		// Offensive
		[Header("Offensive Stats")]
		public Stat AttackSpeed;
		public Stat CritChance;
		public Stat CritDamage;
		public Stat Accuracy;
		public Stat FinalAttack;
		public Stat ExtraAttackChance;

		// Advantage
		[Header("Advantage Stats")]
		public Stat Lifesteal;
		public Stat SkillVamp;
		public Stat ManaCostReduction;
		public Stat EnergyCostReduction;
		public Stat CooldownReduction;
		public Stat AbilityPower;

		// Physics
		[Header("Sim/Physics Stats")]
		public Stat TurnSpeed;
		public Stat Speed;
		public Stat BulletSpeed;
		public Stat Range;
		public Stat RangeCloak;
		public Stat AoEResist;
		public Stat ArcOfFire;

		// Meta
		[Header("Meta Stats")]
		public ImmutableStat Level;
		public ImmutableStat Difficulty;
		public Stat Reward;
		public Stat Cost;

		#endregion Stats

		#region Status & Element

		// Status (Resist & Apply)
		[Header("Status")]
		public StatusStat Stun;
		public StatusStat Poison;
		public StatusStat Freeze;
		public StatusStat Slow;
		public StatusStat Blind;


		// Ele (Resist & Mastery)
		[Header("Element")]
		public ElementStat Fire;
		public ElementStat Water;
		public ElementStat Earth;
		public ElementStat Air;
		public ElementStat Wind;
		public ElementStat Metal;
		public ElementStat Gold;
		public ElementStat Nature;
		public ElementStat Light;
		public ElementStat Dark;
		public ElementStat Electric;
		public ElementStat Ice;
		public ElementStat Toxic;

		#endregion Status & Element

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
		public event Action<IStat, ddouble> OnMaxValueChanged = delegate { };
		public event Action<IStat, ddouble> OnCurrentValueDecreased = delegate { };
		public event Action<IStat, ddouble> OnCurrentValueIncreased = delegate { };
		// Bonus
		public event Action<IStat, IStatMod> OnStatBonusAdded = delegate { };
		public event Action<IStat, IStatMod> OnStatNerfAdded = delegate { };
		// Regenerable
		public event Action<IStat, ddouble> OnRegenerate = delegate { };
		public event Action<IStat, ddouble> OnRegenValueChanged = delegate { };
		public event Action<IStat, ddouble> OnRegenRateChanged = delegate { };

		public void RegisterCallbacks()
		{
			foreach (var stat in Stats)
			{
				stat.OnValueChanged += (s, v) =>
				{
					OnValueChanged?.Invoke(s, v);
					if (v < s.Value) OnValueDecreased?.Invoke(s, v);
					if (v > s.Value) OnValueIncreased?.Invoke(s, v);
				};
				if (stat is IDepletable depletable)
				{
					depletable.OnValueChanged += (s, v) => OnMaxValueChanged?.Invoke(s, v); // On Val change triggers On Max Val change
				}
				if (stat is IRegenerable regenerable)
				{
					regenerable.OnRegenerate += (s, v) => OnRegenerate?.Invoke(s, v);
					regenerable.OnRegenValueChanged += (s, v) => OnRegenValueChanged?.Invoke(s, v);
					regenerable.OnCurrentValueDecreased += (s, v) => OnCurrentValueDecreased?.Invoke(s, v);
					regenerable.OnCurrentValueIncreased += (s, v) => OnCurrentValueIncreased?.Invoke(s, v);
					regenerable.OnRegenRateChanged += (s, v) => { OnRegenRateChanged?.Invoke(s, v); };
				}
			}
		}

		#endregion Events

		#region Constructor

		// Constructor
		public StatBlock()
		{
			Health = new DepletableStat(StatType.Health);
			Mana = new DepletableStat(StatType.Mana);
			Attack = new Stat(StatType.Attack);
			MagicAttack = new Stat(StatType.MagicAttack);
			Defence = new Stat(StatType.Defence);
			MagicResist = new Stat(StatType.MagicResist);
			Shield = new RegenerableStat(StatType.Shield);
			Energy = new DepletableStat(StatType.Energy);
			//
			ArmourHealth = new DepletableStat(StatType.ArmourHealth);
			ArmourDefence = new Stat(StatType.ArmourDefence);
			CritResist = new Stat(StatType.CritResist);
			DodgeChance = new Stat(StatType.DodgeChance);
			BlockChance = new Stat(StatType.BlockChance);
			BlockDefence = new Stat(StatType.BlockDefence);
			SpellCounter = new Stat(StatType.SpellCounter);
			SpellCounterResist = new Stat(StatType.SpellCounterResist);
			Thorns = new Stat(StatType.Thorns);
			SpellThorns = new Stat(StatType.SpellThorns);
			Reflect = new Stat(StatType.Reflect);
			//
			IgnoreBlock = new Stat(StatType.IgnoreBlock);
			DefenceReduction = new Stat(StatType.DefenceReduction);
			MagicResistReduction = new Stat(StatType.MagicResistReduction);
			ArmourPenetration = new Stat(StatType.ArmourPenetration);
			ShieldPenetration = new Stat(StatType.ShieldPenetration);
			AttackReduction = new Stat(StatType.AttackReduction);
			AttackSpeed = new Stat(StatType.AttackSpeed);
			//
			CritChance = new Stat(StatType.CritChance);
			CritDamage = new Stat(StatType.CritDamage);
			Accuracy = new Stat(StatType.Accuracy);
			FinalAttack = new Stat(StatType.FinalAttack);
			ExtraAttackChance = new Stat(StatType.ExtraAttackChance);
			//
			Lifesteal = new Stat(StatType.Lifesteal);
			SkillVamp = new Stat(StatType.SkillVamp);
			ManaCostReduction = new Stat(StatType.ManaCostReduction);
			EnergyCostReduction = new Stat(StatType.EnergyCostReduction);
			CooldownReduction = new Stat(StatType.CooldownReduction);
			AbilityPower = new Stat(StatType.AbilityPower);
			//
			TurnSpeed = new Stat(StatType.TurnSpeed);
			Speed = new Stat(StatType.Speed);
			BulletSpeed = new Stat(StatType.BulletSpeed);
			RangeCloak = new Stat(StatType.RangeCloak);
			Range = new Stat(StatType.Range);
			AoEResist = new Stat(StatType.AoEResist);
			ArcOfFire = new Stat(StatType.ArcOfFire);
			//
			Level = new ImmutableStat(StatType.Level);
			Difficulty = new ImmutableStat(StatType.Difficulty);
			Reward = new Stat(StatType.Reward);
			Cost = new Stat(StatType.Cost);
			//
			Stun = new StatusStat(StatusType.Stun);
			Poison = new StatusStat(StatusType.Poison);
			Freeze = new StatusStat(StatusType.Freeze);
			Slow = new StatusStat(StatusType.Slow);
			Blind = new StatusStat(StatusType.Blind);
			//
			Fire = new ElementStat(ElementType.Fire);
			Water = new ElementStat(ElementType.Water);
			Earth = new ElementStat(ElementType.Earth);
			Air = new ElementStat(ElementType.Air);
			Wind = new ElementStat(ElementType.Wind);
			Metal = new ElementStat(ElementType.Metal);
			Gold = new ElementStat(ElementType.Gold);
			Nature = new ElementStat(ElementType.Nature);
			Light = new ElementStat(ElementType.Light);
			Dark = new ElementStat(ElementType.Dark);
			Electric = new ElementStat(ElementType.Electric);
			Ice = new ElementStat(ElementType.Ice);
			Toxic = new ElementStat(ElementType.Toxic);

			Stats = GetAllStats();
			RegisterCallbacks();
		}

		#endregion Constructor

		#region Logic

		public void Regenerate()
		{
			foreach (IRegenerable stat in GetRegenerableStats())
			{
				stat.Regenerate();
			}
		}

		#endregion Logic

		#region Methods

		// ===== Functions ======
		// Construction
		List<IDepletable> Depletables;
		public void ConstructDepletableStats()
		{
			List<IDepletable> depletableStats = new List<IDepletable>();
			foreach (var field in GetType().GetFields())
			{
				if (field.FieldType == typeof(IDepletable))
				{
					depletableStats.Add((IDepletable)field.GetValue(this));
				}
			}
		}

		List<IRegenerable> Regenerables;
		public void ConstructRegenerableStats()
		{
			List<IRegenerable> regenerableStats = new List<IRegenerable>();
			foreach (var field in GetType().GetFields())
			{
				if (typeof(IRegenerable).IsAssignableFrom(field.FieldType))
				{
					var value = field.GetValue(this) as IRegenerable;
					if (value != null)
						regenerableStats.Add(value);
				}
			}
		}

		// Adders
		public void AddStat(IStat stat)
		{
			if (stat == null)
			{
				LogManager.Instance.LogError("Cannot add null stat!");
				return;
			}
			if (Stats.Any(s => s.StatType == stat.StatType))
			{
				LogManager.Instance.LogWarning($"Stat {stat.StatType} already exists in StatBlock!");
				GetType().GetField(stat.StatType.ToString()).SetValue(this, stat);
				return;
			}
			Stats.Add(stat);
			StatMap[stat.StatType] = stat;
		}

		public void ModifyStat(StatType type, IStatMod mod)
		{
			IStat stat = GetStat(type);
			stat.SetValue(MathsLib.Operate(stat.Value, mod.Value, mod.Operation));
			if (mod.IsPositive)
				OnStatBonusAdded?.Invoke(stat, mod);
			if (!mod.IsPositive)
				OnStatNerfAdded?.Invoke(stat, mod);
		}

		public void RemoveStat(IStat stat)
		{
			if (stat == null)
			{
				LogManager.Instance.LogError("Cannot remove null stat!");
				return;
			}
			if (!Stats.Any(s => s.StatType == stat.StatType))
			{
				LogManager.Instance.LogWarning($"Stat {stat.StatType} does not exist in StatBlock!");
				return;
			}
			Stats.Remove(stat);
			StatMap.Remove(stat.StatType);

			// TODO why so many ways to search? Since StatMap exists, no use for StatTypes
		}

		// Getters
		public List<IDepletable> GetDepletableStats()
		{
			if (Depletables == null)
				ConstructDepletableStats();
			return Depletables;
		}

		public List<IRegenerable> GetRegenerableStats()
		{
			if (Regenerables == null)
				ConstructRegenerableStats();
			return Regenerables;
		}

		List<IStat> GetAllStats()
		{
			return Stats;
		}

		public IStat GetStat(StatType type)
		{
			return StatMap.ContainsKey(type) ? StatMap[type] : null;
		}

		public bool IfStatExists(StatType type)
		{
			return Stats.Any(stat => stat.StatType == type);
		}

		public List<IStat> GetStats()
		{
			return Stats;
		}

		public List<IElement> GetElementStats()
		{
			List<IElement> elementStats = new List<IElement>();
			foreach (var field in GetType().GetFields())
			{
				if (field.FieldType == typeof(IElement))
				{
					if (field.GetValue(this) == null)
					{
						LogManager.Instance.LogError($"Element {field.Name} is null!");
						continue;
					}
					elementStats.Add((IElement)field.GetValue(this));
				}
			}
			return elementStats;
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