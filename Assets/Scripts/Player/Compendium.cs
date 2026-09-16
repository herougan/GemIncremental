using System.Collections.Generic;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Stats;
using Util.Maths;

namespace Player
{
	/// <summary>
	/// Bestiary + elemental/type matchup data - e.g. "Fire deals 1.5x against Water creatures" from the
	/// design discussion. A child of Player in the damage-modifier tree (see Player.Children), so its
	/// multiplier stacks underneath Player's own global multiplier for every hit.
	///
	/// "Is the target a Water creature" is read off the target's ElementBlock (ApplyEntry'd from the
	/// target's Plan.ElementEntries at spawn - see Entity's constructor) rather than a new field -
	/// GetElement(Water) existing at all is treated as "this entity is aligned with Water" for matchup
	/// purposes. That's a first-pass interpretation, not a settled rule - revisit once elemental design
	/// is more fleshed out (e.g. a monster might carry an ElementEntry for a *resistance* it has without
	/// being "of" that element).
	/// </summary>
	public class Compendium : IDamageModifier
	{
		readonly Dictionary<(ElementType attack, ElementType defend), ddouble> matchups = new()
		{
			// Placeholder table - only the one example pair from the design discussion is filled in.
			[(ElementType.Fire, ElementType.Water)] = 1.5,
		};

		public void SetMatchup(ElementType attack, ElementType defend, ddouble multiplier) =>
			matchups[(attack, defend)] = multiplier;

		public ddouble GetMultiplier(Damage damage)
		{
			if (damage.Element == ElementType.None || damage.Target == null) return 1;

			foreach (var pair in matchups)
			{
				if (pair.Key.attack == damage.Element && damage.Target.GetElement(pair.Key.defend) != null)
				{
					return pair.Value;
				}
			}
			return 1;
		}

		public IEnumerable<IDamageModifier> Children => System.Array.Empty<IDamageModifier>();
	}
}
