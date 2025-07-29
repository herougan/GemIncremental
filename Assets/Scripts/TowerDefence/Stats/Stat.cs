using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using TowerDefence.Expirable;
using TowerDefence.Resources;
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
		/// Gets or sets the value of the stat.
		/// </summary>
		public ddouble Value { get; set; }

		/// <summary>
		/// Gets or sets the type of the stat.
		/// </summary>
		public StatType Type { get; }

		/// <summary>
		/// Gets or sets the dynamic value of the stat.
		/// This dynamic value can be used to store the calculated values that take in Modifiers and such.
		/// This value is not used internally in classes that implement this interface.
		/// Dynamic divided by Value gives the scaling factor of the stat.
		/// </summary>
		public ddouble Dynamic { get; set; }

		// public ddouble Scale { get; }

		/// <summary>
		/// Occurs when the value of the stat changes.
		/// </summary>
		public event Action<IStat, ddouble> OnValueChanged;

		/// <summary>
		/// Scales the stat value by the specified factor.
		/// </summary>
		/// <param name="scale">The factor by which to scale the stat value.</param>
		public void Recalculate(ddouble scale);
	}

	[Serializable]
	public class Stat : IStat
	{
		// Cooky hacky so that we can maintain using the interface and fields with getters and setters;
		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnValueChanged?.Invoke(this, _Value);
				_Value = value;
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;


		public StatType Type { get { return _Type; } set { _Type = value; } }
		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;
		public ddouble Dynamic { get; set; } // Not serialized - only needed during game
		public ddouble Scale => Dynamic / Value; // Dynamic divided by Value gives the scaling factor of the stat

		public void Recalculate(ddouble scale)
		{
			Value *= scale;
		}

		// Returns the Stat, and the original value
		public event Action<IStat, ddouble> OnValueChanged = delegate { };

		public Stat(StatType type, ddouble value = default(ddouble))
		{
			Type = type;
			Value = value;
		}

		public Stat(Stat original)
		{
			Type = original.Type;
			Value = original.Value;
		}
	}

	[Serializable]
	public class ImmutableStat : IStat
	{
		public ddouble Value // Only Initable
		{
			get { return _Value; }
			set
			{
				if (_Value == 0) _Value = value;
				else { Debug.LogError($"{this} ({_Value}) cannot be mutated to {value}!"); }
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;

		public ddouble Dynamic
		{
			get { return Dynamic; }
			set { Dynamic = value; }
		}

		public StatType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}
		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;

		public ImmutableStat(StatType type, ddouble value = default(ddouble))
		{
			Type = type;
			Value = value;
		}

		public ImmutableStat(ImmutableStat original)
		{
			Type = original.Type;
			Value = original.Value;
		}

		public ImmutableStat(IStat original)
		{
			Type = original.Type;
			Value = original.Value;
		}

		public event Action<IStat, ddouble> OnValueChanged;

		public void Recalculate(ddouble scale)
		{
			// Cannot Scale
			Debug.LogWarning($"Cannot scale ImmutableStat {Type}");
			return;
		}
	}

	public interface IDepletable : IStat
	{
		public ddouble Max { get; }
		public ddouble Current { get; }
		public void SetMax(ddouble value);
		public event Action<IStat, ddouble> OnValueDecreased;
		public event Action<IStat, ddouble> OnMaxValueChanged;
	}

	[Serializable]
	public class DepletableStat : IDepletable
	{
		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnMaxValueChanged?.Invoke(this, _Value);
				_Value = value;
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;

		public ddouble Dynamic { get; set; }

		public StatType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}
		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;

		public ddouble Max => Value;

		public void SetMax(ddouble value)
		{
			Value = value;
		}

		public ddouble Current
		{
			get { return _Current; }
			set
			{
				if (_Current != value) OnValueChanged?.Invoke(this, _Current);
				if (_Current > value) OnValueDecreased?.Invoke(this, _Current - value);
				_Current = value;
			}
		}
		[FormerlySerializedAs("Current")]
		[SerializeField] private ddouble _Current;

		// Events
		public event Action<IStat, ddouble> OnValueChanged = delegate { };
		public event Action<IStat, ddouble> OnValueDecreased = delegate { };
		public event Action<IStat, ddouble> OnMaxValueChanged = delegate { };

		// Constructor
		public DepletableStat(StatType type, ddouble value, ddouble current)
		{
			Type = type;
			Value = value;
			Current = current;
		}

		public DepletableStat(StatType type, ddouble value = default(ddouble))
		{
			Type = type;
			Value = value;
			Current = Value;
		}

		public DepletableStat(DepletableStat original)
		{
			Type = original.Type;
			Value = original.Value;
			Current = original.Current;
		}

		// Methods
		public void Recalculate(ddouble scale)
		{
			Value *= scale;
		}
	}

	[Serializable]
	public class ResourceStat : IDepletable
	{
		// Properties
		public ddouble Max
		{
			get { return _Value; }
			set { _Value = value; }
		}
		[FormerlySerializedAs("Max")]
		[SerializeField] private ddouble _Max;

		public ddouble Current
		{
			get { return _Current; }
			set
			{
				if (_Current != value) OnValueChanged?.Invoke(this, _Current);
				_Current = value;
			}
		}
		[FormerlySerializedAs("Current")]
		[SerializeField] private ddouble _Current;

		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnMaxValueChanged?.Invoke(this, _Value);
				_Value = value;
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;

		public StatType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}
		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;

		public EntityResourceType Resource
		{
			get { return _Resource; }
			set { _Resource = value; }
		}
		[FormerlySerializedAs("Resource")]
		[SerializeField] private EntityResourceType _Resource;

		public ddouble Dynamic { get; set; }

		// Events
		public event Action<IStat, ddouble> OnValueDecreased;
		public event Action<IStat, ddouble> OnMaxValueChanged;
		public event Action<IStat, ddouble> OnValueChanged;

		// Functions
		public bool HasEnough(ddouble cost)
		{
			return Value >= cost;
		}

		public void Deduct(ddouble value)
		{
			Value -= value;
			if (Value < 0) Value = 0;
		}

		public void Add(ddouble value)
		{
			Value += value;
		}

		public void SetMax(ddouble value)
		{
			throw new NotImplementedException();
		}

		public void Recalculate(ddouble scale)
		{
			throw new NotImplementedException();
		}

		// Constructor
		public ResourceStat(EntityResourceType resource, ddouble value, ddouble current = default(ddouble))
		{
			Type = StatType.Resource;
			Resource = resource;
			Value = value;
			Current = current;
		}
	}

	public class CustomResourceStat : ResourceStat
	{
		// Custom
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}
		[FormerlySerializedAs("Type")]
		[SerializeField] private string _Name;
		public int NameHash { get; private set; }
		public void HashName()
		{
			NameHash = GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			return NameHash == ((CustomResourceStat)obj).NameHash;
		}
		public override int GetHashCode()
		{
			return NameHash.GetHashCode();
		}

		public CustomResourceStat(EntityResourceType resource, ddouble value, ddouble current = default(ddouble), string name = "")
			: base(resource, value, current)
		{
			Name = name;
			HashName();
		}

		public CustomResourceStat(CustomResourceStat original)
			: base(original.Resource, original.Value, original.Current)
		{
			Name = original.Name;
			HashName();
		}
	}

	public interface IRegenerable : IDepletable
	{
		public ddouble Regeneration { get; }
		public event Action<IStat, ddouble> OnRegenerate;
		public event Action<IStat, ddouble> OnRegenValueChanged;
		public void Regenerate();
	}

	[Serializable]
	public class RegenerableStat : IRegenerable
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
		[SerializeField] private ddouble _Regeneration;

		public ddouble Current
		{
			get { return _Current; }
			set
			{
				if (_Current != value) OnValueChanged?.Invoke(this, _Current);
				if (_Current > value) OnValueDecreased?.Invoke(this, _Current - value);
				_Current = value;
			}
		}
		[FormerlySerializedAs("Current")]
		[SerializeField] private ddouble _Current;

		public ddouble Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value) OnMaxValueChanged?.Invoke(this, _Value);
				_Value = value;
			}
		}
		[FormerlySerializedAs("Value")]
		[SerializeField] private ddouble _Value;

		public ddouble Dynamic
		{
			get { return Dynamic; }
			set { Dynamic = value; }
		}

		public StatType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}
		[FormerlySerializedAs("Type")]
		[SerializeField] private StatType _Type;

		public ddouble Max
		{
			get { return Value; }
		}
		public void SetMax(ddouble value)
		{
			Value = value;
		}

		// Events
		public event Action<IStat, ddouble> OnValueChanged = delegate { };
		public event Action<IStat, ddouble> OnValueDecreased = delegate { };
		public event Action<IStat, ddouble> OnMaxValueChanged = delegate { };
		public event Action<IStat, ddouble> OnRegenerate = delegate { };
		public event Action<IStat, ddouble> OnRegenValueChanged = delegate { };


		// Constructor
		public RegenerableStat(StatType type, ddouble value, ddouble current, ddouble regeneration)
		{
			Type = type;
			Value = value;
			Current = current;
			Regeneration = regeneration;
		}

		public RegenerableStat(StatType type, ddouble value = default(ddouble))
		{
			Type = type;
			Value = value;
			Current = Value;
			Regeneration = 0;
		}

		public RegenerableStat(RegenerableStat original)
		{
			Type = original.Type;
			Value = original.Value;
			Current = original.Current;
			Regeneration = original.Regeneration;
		}

		// Methods
		public void Regenerate()
		{
			if (Current < Max)
			{
				// Invoke Event
				OnRegenerate?.Invoke(this, Math.Min(Regeneration, Max - Current));
				// Regenerate, Capped at Max health
				Current += Regeneration; if (Current > Max) Current = Value;
			}
		}

		public void Recalculate(ddouble scale)
		{
			Value *= scale;
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
	}

	public enum StatStatusType
	{
		Stun,
		Poison,
		Freeze,
		Slow,
		Blind,
		Knockback,
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

	public enum EntityResourceType
	{
		// Default (Used for Primary/Secondary/Tertiary)
		None,
		// Entity Held Resources
		DarkCore,
		FungiSpores,
		FlowerPollen,
		Capacitance,
	}

	#endregion Enums

	#region Blocks

	[Serializable]
	public class StatBlock
	{
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


		// Resources
		[Header("Resource")]
		public ResourceStat PrimaryResource;
		public ResourceStat SecondaryResource;

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

		// Trackers
		public List<IStat> Stats;
		public List<StatType> StatTypes;

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
			PrimaryResource = new ResourceStat(EntityResourceType.None, 0, 0);
			SecondaryResource = new ResourceStat(EntityResourceType.None, 0, 0);
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
		}

		// Functions
		public List<IDepletable> GetDepletableStats()
		{
			List<IDepletable> depletableStats = new List<IDepletable>();
			foreach (var field in GetType().GetFields())
			{
				if (field.FieldType == typeof(IDepletable))
				{
					depletableStats.Add((IDepletable)field.GetValue(this));
				}
			}
			return depletableStats;
		}

		List<IStat> GetAllStats()
		{
			List<IStat> allStats = new List<IStat>();
			foreach (var field in GetType().GetFields())
			{
				if (field.FieldType == typeof(IStat))
				{
					allStats.Add((IStat)field.GetValue(this));
				}
			}
			return allStats;
		}

		public IStat GetStat(StatType type)
		{
			return Stats.FirstOrDefault(stat => { return stat.Type == type; });
		}

		public void AddStat(IStat stat)
		{
			if (stat == null)
			{
				Debug.LogError("Cannot add null stat!");
				return;
			}
			if (Stats.Any(s => s.Type == stat.Type))
			{
				Debug.LogWarning($"Stat {stat.Type} already exists in StatBlock!");
				return;
			}
			Stats.Add(stat);
			StatTypes.Add(stat.Type); // TODO need to add all kinds of stats, including differentiating elementals
			GetType().GetField(stat.Type.ToString()).SetValue(this, stat);
		}

		public bool IfStatExists(StatType type)
		{
			return Stats.Any(stat => stat.Type == type);
		}

		public List<IStat> GetStats()
		{
			return Stats;
		}

		public void AddStat(ElementStat stat)
		{
			// TODO
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
						Debug.LogError($"Element {field.Name} is null!");
						continue;
					}
					elementStats.Add((IElement)field.GetValue(this));
				}
			}
			return elementStats;
		}
	}
	public class ResourceBlock
	{
		public List<ResourceStat> Resources = new List<ResourceStat>();

		public void AddResource(EntityResourceType type, ddouble value)
		{
			// Check if resource already exists
			foreach (var resource in Resources)
			{
				if (resource.Resource == type)
				{
					resource.Add(value);
					return;
				}
			}

			// If not, create a new resource
			ResourceStat newResource = new ResourceStat(type, value, value);
			Resources.Add(newResource);
		}

		public void DeductResource(EntityResourceType type, ddouble value)
		{
			foreach (var resource in Resources)
			{
				if (resource.Resource == type)
				{
					resource.Deduct(value);
					return;
				}
			}
			Debug.LogError($"Resource {type} not found!");
		}

		public void Recalculate(ddouble scale)
		{
			foreach (var resource in Resources)
			{
				resource.Recalculate(scale);
			}
		}
	}
	public class FlexStatBlock
	{
		public List<IStat> stats = new List<IStat>();
		public List<IResource> resources = new List<IResource>();

		public void AddStat(IStat stat)
		{
			if (stat == null)
			{
				Debug.LogError("Cannot add null stat!");
				return;
			}
			if (stats.Any(s => s.Type == stat.Type))
			{
				Debug.LogWarning($"Stat {stat.Type} already exists in FlexStatBlock!");
				return;
			}
			if (stat.Type == StatType.Resource)
			{
				// resources.Add(stat);
			}
			stats.Add(stat);
		}

		// public void AddResource(IResource resource)
		// {
		// 	if (resource == null)
		// 	{
		// 		Debug.LogError("Cannot add null resource!");
		// 		return;
		// 	}
		// 	if (resources.Any(r => r.ResourceType == resource.ResourceType))
		// 	{
		// 		Debug.LogWarning($"Resource {resource.ResourceType} already exists in FlexStatBlock!");
		// 		return;
		// 	}
		// 	resources.Add(resource);
		// }
	}

	#endregion Blocks
}