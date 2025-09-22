
using System;
using System.Collections.Generic;
using Debug;
using TowerDefence.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Maths;

namespace TowerDefence.Entity.Resources
{
	public interface IResource : IStat
	{
		public ResourceType Resource { get; set; }
		//
		bool HasEnough(ddouble cost);
		void Deduct(ddouble value);
		void Add(ddouble value);
		//
		event Action<IResource, ddouble> OnResourceIncreased;
		event Action<IResource, ddouble> OnResourceDecreased;
	}

	[Serializable]
	public class ResourceStat : Stat, IResource
	{
		// Properties

		public ResourceType Resource
		{
			get { return _Resource; }
			set { _Resource = value; }
		}
		[FormerlySerializedAs("Resource")]
		[SerializeField] private ResourceType _Resource;

		// Events
		public event Action<IResource, ddouble> OnResourceIncreased = delegate { };
		public event Action<IResource, ddouble> OnResourceDecreased = delegate { };

		// Functions
		public bool HasEnough(ddouble cost)
		{
			return Value >= cost;
		}

		public void Deduct(ddouble value)
		{
			if (value <= 0) return;
			OnResourceDecreased?.Invoke(this, value);
			Value -= value;
		}

		public void Add(ddouble value)
		{
			if (value <= 0) return;
			OnResourceIncreased?.Invoke(this, value);
			Value += value;
		}

		// Constructor
		public ResourceStat(ResourceType resource, ddouble value) : base(StatType.Resource, value)
		{
			StatType = StatType.Resource;
			Resource = resource;
			Value = value;
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

		public CustomResourceStat(ResourceType resource, ddouble value, string name = "")
			: base(resource, value)
		{
			Name = name;
			HashName();
		}

		public CustomResourceStat(CustomResourceStat original)
			: base(original.Resource, original.Value)
		{
			Name = original.Name;
			HashName();
		}
	}

	public class ResourceBlock
	{
		#region Resources
		List<ResourceStat> Resources = new List<ResourceStat>();

		#endregion Resources

		#region Events

		// Events
		public event Action<IResource, ddouble> OnResourceIncreased = delegate { };
		public event Action<IResource, ddouble> OnResourceDecreased = delegate { };
		public void RegisterCallbacks()
		{
			foreach (var resource in Resources)
			{
				resource.OnResourceIncreased += (s, v) => OnResourceIncreased?.Invoke(s, v);
				resource.OnResourceDecreased += (s, v) => OnResourceDecreased?.Invoke(s, v);
			}
		}

		#endregion Events

		#region Constructor

		public ResourceBlock()
		{
			Resources = new List<ResourceStat>();
		}

		#endregion Constructor

		#region Methods
		public void GetResource(ResourceType type, out ResourceStat resource)
		{
			resource = null;
			foreach (var res in Resources)
			{
				if (res.Resource == type)
				{
					resource = res;
					return;
				}
			}
		}

		public void AddResource(List<ResourceType> types, List<ddouble> values)
		{
			for (int i = 0; i < Math.Min(types.Count, values.Count); i++)
			{
				AddResource(types[i], values[i]);
			}
		}

		public void AddResource(ResourceType type, ddouble value)
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
			ResourceStat newResource = new ResourceStat(type, value);
			Resources.Add(newResource);
		}

		public void DeductResource(ResourceType type, ddouble value)
		{
			foreach (var resource in Resources)
			{
				if (resource.Resource == type)
				{
					resource.Deduct(value);
					return;
				}
			}
			LogManager.Instance.LogError($"Resource {type} not found!");
		}

		public void Recalculate(ddouble scale)
		{
			foreach (var resource in Resources)
			{
				resource.Recalculate(scale);
			}
		}

		public void Clear()
		{
			Resources.Clear();
		}
		#endregion Methods
	}

	public enum ResourceType
	{
		// Default (Used for Primary/Secondary/Tertiary)
		None,

		// Entity Held Resources
		DarkCore,
		FungiSpores,
		FlowerPollen,
		Capacitance,

		//
		Gold,
		Wood,
		Stone,
		Food,
	}
}