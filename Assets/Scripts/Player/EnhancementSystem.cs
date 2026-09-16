using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Stats;

namespace Player
{
	/// <summary>
	/// Shared contract for Player-owned systems that permanently modify an Entity when it's
	/// created/spawned - Foundry today, and whatever future systems join it (Achievements, Compendium,
	/// Forge, Collection, Research Centre, ...), per the design discussion. Deliberately separate from
	/// the dynamic in-combat IStatMod stacking (Skill.ApplyPassive, Entity.RegisterStatMod): ApplyModTo
	/// runs once, at spawn - it's initial setup, not something that gets re-applied/removed as combat
	/// state changes.
	///
	/// Two methods, not one, because EntityManager needs to *aggregate* every system's mods into one
	/// cached list (see EntityManager's Enhancement Systems region) without applying them to any entity
	/// yet - GetMods is the query side of that (pure data, no target), ApplyModTo is the per-system
	/// convenience for applying just this one system directly. Both exist on IStatMod-backed systems
	/// (Foundry) trivially - GetMods returns the same list ApplyModTo would loop over.
	/// </summary>
	public interface IEnhancementSystem
	{
		void ApplyModTo(IEntity entity);
		IEnumerable<IStatMod> GetMods();
	}
}
