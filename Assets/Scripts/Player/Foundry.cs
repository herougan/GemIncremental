using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Stats;

namespace Player
{
	/// <summary>
	/// Permanent player-driven stat growth - "Foundry can be for perma stat increases of the Player,
	/// which are then affected onto the Tower/Monsters" per the design discussion. IEnhancementSystem's
	/// first implementation - registers each mod into the target Entity's own StatType -> IStatMod
	/// registry (see Entity.RegisterStatMod) - the same mechanism a Skill's Passives use - rather than
	/// StatBlock.ModifyStat directly, so Foundry's contributions compose correctly with everything else
	/// touching the same stat instead of being a second, independent mechanism.
	///
	/// AddPermaMod calls EntityManager.MarkEnhancementsDirty - see EntityManager's Enhancement Systems
	/// region for why every mutation site needs to do that (the cached aggregate otherwise has no way
	/// to know it's stale).
	/// </summary>
	public class Foundry : IEnhancementSystem
	{
		public List<IStatMod> PermaMods { get; } = new();

		public void AddPermaMod(IStatMod mod)
		{
			PermaMods.Add(mod);
			TowerDefence.Entity.EntityManager.MarkEnhancementsDirty();
		}

		public void ApplyModTo(IEntity entity)
		{
			foreach (IStatMod mod in PermaMods)
			{
				entity.RegisterStatMod(mod);
			}
		}

		public IEnumerable<IStatMod> GetMods() => PermaMods;
	}
}
