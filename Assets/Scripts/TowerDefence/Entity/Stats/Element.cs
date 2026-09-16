using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Stats
{
	public interface IElement : IStat
	{
		public ElementType Element { get; }
		public ddouble Mastery { get; }
		public ddouble Resist { get; }
		public event Action<IElement, ddouble> OnResistChanged;
		public event Action<IElement, ddouble> OnMasteryChanged;
	}

	[Serializable]
	public class ElementStat : Stat, IElement
	{
		// Cooky hack (see IStat)
		[FormerlySerializedAs("Element")]
		[SerializeField] private ElementType _Element;
		public ElementType Element { get { return _Element; } set { _Element = value; } }

		[FormerlySerializedAs("Mastery")]
		[SerializeField] private ddouble _Mastery;
		public ddouble Mastery { get { return _Mastery; } private set { _Mastery = value; } }

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


		[FormerlySerializedAs("Dynamic")]
		[SerializeField] private double _Dynamic;

		// Events
		public event Action<IElement, ddouble> OnResistChanged = delegate { };
		public event Action<IElement, ddouble> OnMasteryChanged = delegate { };

		public void AddMastery(int n = 1)
		{
			if (n < 0) throw new ArgumentException("Mastery cannot be negative.");
			_Mastery += n;
		}

		public void AddResist(double val)
		{
			if (val < 0) throw new ArgumentException("Resist cannot be negative.");
			Resist += val;
		}

		public ElementStat(ElementType element, ddouble value = default(ddouble), ddouble mastery = default(ddouble), ddouble resist = default(ddouble)) : base(StatType.Element, value)
		{
			_Element = element;
			_Mastery = mastery;
			_Resist = resist;
		}
	}

	/// <summary>
	/// Design-time entry so a Plan (MonsterPlan/TowerPlan) can hand ElementBlock a sparse list of
	/// starting elements without ElementMap itself (a Dictionary) needing to be Unity-serializable.
	/// </summary>
	[Serializable]
	public struct ElementEntry
	{
		public ElementType Type;
		public double Value;
		public double Resist;
		public double Mastery;
	}

	[Serializable]
	public class ElementBlock
	{
		// Elements
		// Note: ElementMap (a Dictionary) is never Unity-serializable - it's built lazily at runtime,
		// element-by-element, only for the elements an entity actually ends up touching. See ElementEntry
		// for how a Plan supplies starting values.
		public Dictionary<ElementType, ElementStat> ElementMap = new Dictionary<ElementType, ElementStat>();
		public List<ElementStat> Elements = new List<ElementStat>();

		public event Action<IElement, ddouble> OnResistChanged;
		public event Action<IElement, ddouble> OnMasteryChanged;

		/// <summary>
		/// No-op by design: AddElement already wires an element's callbacks the moment it's created
		/// (see below), which is the only point new elements ever appear given the lazy pattern. Kept
		/// around so existing call sites (Entity.RegisterElementCallbacks) don't need to change.
		/// </summary>
		public void RegisterCallbacks() { }

		/// <summary>
		/// Returns the underlying element object if it exists, or null - never creates one.
		/// Prefer GetValue/GetResist/GetMastery for plain reads.
		/// </summary>
		public ElementStat GetElement(ElementType type)
		{
			return ElementMap.TryGetValue(type, out var element) ? element : null;
		}

		public ddouble GetValue(ElementType type)
		{
			return ElementMap.TryGetValue(type, out var element) ? element.Value : default(ddouble);
		}

		public ddouble GetResist(ElementType type)
		{
			return ElementMap.TryGetValue(type, out var element) ? element.Resist : default(ddouble);
		}

		public ddouble GetMastery(ElementType type)
		{
			return ElementMap.TryGetValue(type, out var element) ? element.Mastery : default(ddouble);
		}

		public void AddElement(ElementStat element)
		{
			if (ElementMap.ContainsKey(element.Element)) throw new ArgumentException($"Element {element.Element} already exists.");
			ElementMap[element.Element] = element;
			Elements.Add(element);
			element.OnResistChanged += (stat, val) => OnResistChanged?.Invoke(stat, val);
			element.OnMasteryChanged += (stat, val) => OnMasteryChanged?.Invoke(stat, val);
		}

		/// <summary>
		/// Gets the element, creating (and wiring) a fresh one only if it doesn't exist yet.
		/// </summary>
		private ElementStat GetOrCreate(ElementType type)
		{
			var element = GetElement(type);
			if (element == null)
			{
				element = new ElementStat(type);
				AddElement(element);
			}
			return element;
		}

		public void AddValue(ElementType type, ddouble amount) => GetOrCreate(type).AddValue(amount);
		public void AddResist(ElementType type, double amount) => GetOrCreate(type).AddResist(amount);
		public void AddMastery(ElementType type, int amount = 1) => GetOrCreate(type).AddMastery(amount);

		public void ApplyEntry(ElementEntry entry)
		{
			ElementStat stat = GetOrCreate(entry.Type);
			if (entry.Value != 0) stat.SetValue(entry.Value);
			if (entry.Resist != 0) stat.AddResist(entry.Resist);
			if (entry.Mastery != 0) stat.AddMastery((int)entry.Mastery);
		}
	}
	public enum ElementType
	{
		None,
		Fire,
		Earth,
		Water,
		Wind,
		Metal,
		//
		Ice,
		Gold,
		Poison,
		Nature,
		Air,
		Light,
		Dark,
		Electric,
		Toxic,
	}

}