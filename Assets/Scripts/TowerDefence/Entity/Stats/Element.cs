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

	public class ElementBlock
	{
		// Elements
		public Dictionary<ElementType, ElementStat> ElementMap = new Dictionary<ElementType, ElementStat>();
		public List<ElementStat> Elements = new List<ElementStat>();

		public event Action<IElement, ddouble> OnResistChanged;
		public event Action<IElement, ddouble> OnMasteryChanged;

		public void RegisterCallbacks()
		{
			foreach (var element in Elements)
			{
				element.OnResistChanged += (stat, val) => OnResistChanged?.Invoke(stat, val);
				element.OnMasteryChanged += (stat, val) => OnMasteryChanged?.Invoke(stat, val);
			}
		}

		public ElementStat GetElement(ElementType type)
		{
			if (ElementMap.ContainsKey(type)) return ElementMap[type];
			return null;
		}

		public void AddElement(ElementStat element)
		{
			if (ElementMap.ContainsKey(element.Element)) throw new ArgumentException($"Element {element.Element} already exists.");
			ElementMap[element.Element] = element;
			Elements.Add(element);
			element.OnResistChanged += (stat, val) => OnResistChanged?.Invoke(stat, val);
			element.OnMasteryChanged += (stat, val) => OnMasteryChanged?.Invoke(stat, val);
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