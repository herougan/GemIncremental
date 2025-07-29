using System;
using TowerDefence.Calculation;
using TowerDefence.Entity;
using TowerDefence.Skills;
using Incremental.Currency;

namespace Towerdefence.Entity.Items
{
	public interface IItem
	{
		// Define properties or methods that an Item should have
		string Name { get; }
		string Description { get; }
		Currency Cost { get; }
		ItemType ItemType { get; }
	}

	/// <summary>
	/// Items are fungible resources held by entities.
	/// </summary>
	public class Item : IItem
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public Currency Cost { get; private set; }
		public ItemType ItemType { get; private set; }

		public Item(string name, string description = "", Currency cost = default(Currency), ItemType itemType = ItemType.DefaultBerry)
		{
			ItemType = itemType;
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Item name cannot be null or empty", nameof(name));
			}
			if (cost == null)
			{
				throw new ArgumentNullException(nameof(cost), "Cost cannot be null");
			}
			Name = name;
			Description = description;
			Cost = cost;
		}
		public override string ToString()
		{
			return $"{Name}: {Description} (Cost: {Cost})";
		}
	}

	public class Consumable : Item
	{
		public Effect Effect { get; private set; }
		private bool Used { get; set; } = false;

		public Consumable(string name, string description = "", Currency cost = default(Currency), ItemType itemType = ItemType.DefaultBerry)
			: base(name, description, cost)
		{
			// Additional properties or methods specific to consumables can be added here
		}

		public override string ToString()
		{
			return $"{base.ToString()} (Consumable)";
		}

		private bool CanConsume(IEntity entity)
		{
			return SkillsLib.CheckCondition(Effect, entity);
		}

		public bool Use(IEntity entity)
		{
			// Logic for using the consumable item
			// For example, applying effects or removing it from inventory
			if (CanConsume(entity))
			{
				Used = true;
				// Apply effects or perform actions associated with the consumable
				return true; // Indicate successful use
			}
			return false; // Indicate failure to use
		}
	}

	public class Berry : Consumable
	{
		public Berry(string name, string description = "", Currency cost = default(Currency), ItemType itemType = ItemType.DefaultBerry)
			: base(name, description, cost)
		{
			// Additional properties or methods specific to berries can be added here
		}

		public override string ToString()
		{
			return $"{base.ToString()} (Berry)";
		}
	}

	public enum ItemType
	{
		DefaultBerry,
		Gorgiberry,
	}


}