using System.Collections.Generic;
using TowerDefence.Manager;
using Util.Maths;

namespace TowerDefence.Entity.Attack.Damage
{
	/// <summary>
	/// One node in the damage-modifier tree. GetMultiplier is this node's own contribution only
	/// (1 = no effect) - Children are walked and multiplied in underneath it, so a node's total effect
	/// is itself times everything beneath it. E.g. Player (2x global) with Compendium as a child
	/// (1.5x Fire-vs-Water) contributes 3x total when both apply to a given hit.
	///
	/// Deliberately separate from StatBlock's IStatMod/ModifyStat - that system is for *permanent*
	/// stat changes (see Foundry, which does use it). This is a *per-interaction* calculation: nothing
	/// here is stored on an Entity, it's recomputed from scratch for every single hit by
	/// DamageCalculator - performance is not a concern for this pass, correctness/tidiness is.
	/// </summary>
	public interface IDamageModifier
	{
		ddouble GetMultiplier(Damage damage);
		IEnumerable<IDamageModifier> Children { get; }
	}

	/// <summary>
	/// Walks every root IDamageModifier's tree for a given hit and multiplies in whatever each node
	/// decides applies - "hitting the top node of the tree (Player, Map, Game, Boss) then they
	/// promulgate and total the effects from there," per the design discussion.
	///
	/// Map and Boss aren't systems that exist yet, so they're not registered as roots below - add them
	/// to GetRoots the moment they do; nothing else in this pipeline needs to change, since Walk
	/// recurses through whatever Children a root reports.
	/// </summary>
	public static class DamageCalculator
	{
		/// <summary>
		/// Applies every applicable modifier to damage.Value and returns the final number - does not
		/// mutate damage or deplete anything itself. `roots` defaults to the real GameManager/Player
		/// tree (GetRoots) - pass it explicitly to exercise specific nodes without needing a live
		/// GameManager singleton/scene, e.g. from a plain-C# test.
		/// </summary>
		public static ddouble Calculate(Damage damage, IEnumerable<IDamageModifier> roots = null)
		{
			ddouble multiplier = 1;
			foreach (IDamageModifier root in roots ?? GetRoots())
			{
				multiplier *= Walk(root, damage);
			}
			return damage.Value * multiplier;
		}

		static ddouble Walk(IDamageModifier node, Damage damage)
		{
			ddouble total = node.GetMultiplier(damage);
			foreach (IDamageModifier child in node.Children)
			{
				total *= Walk(child, damage);
			}
			return total;
		}

		static IEnumerable<IDamageModifier> GetRoots()
		{
			if (GameManager.Instance == null) yield break;

			if (GameManager.Instance.Player != null) yield return GameManager.Instance.Player;
			yield return GameManager.Instance; // the "Game" root - see GameManager's IDamageModifier implementation
		}
	}
}
