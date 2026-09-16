using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity.Tower;

namespace Util.Game
{
	/// <summary>
	/// One recipe that could be completed right now - TowerPlan.ingredients (a multiset: duplicate
	/// entries mean "needs more than one of this") matched against a hand + field pool. Recipe is the
	/// fusion result; FromHand is which hand items it would consume; FromField is one entry PER
	/// ingredient slot not covered by hand, listing every field Tower that could fill that slot - not a
	/// single pre-chosen one. Which specific duplicate actually gets consumed (if more than one field
	/// Tower could fill a slot) is deliberately left to whoever commits the fusion, not decided here -
	/// see FindCombinables' own doc comment.
	/// </summary>
	public class FusionMatch
	{
		public TowerPlan Recipe;
		public List<TowerPlan> FromHand = new();
		public List<List<Tower>> FromField = new();

		/// <summary>True if every ingredient was covered by the hand alone - no field Towers needed/consumed.</summary>
		public bool IsHandOnly => FromField.Count == 0;
	}

	/// <summary>
	/// Pure calculation, no game-knowledge (no GameManager/EntityManager singleton reaches, everything
	/// comes in as parameters) - same spirit as EntityUtil, just Tower/fusion-shaped instead of
	/// Entity/skill-shaped. Detection only: this never mutates hand or field, and never decides which
	/// specific duplicate field Tower gets consumed for a hand+field match - that's a decision for
	/// whoever actually commits a fusion (not built yet - this is the "highlight what's possible" half
	/// only, per the design discussion).
	/// </summary>
	public static class FusionUtil
	{
		/// <summary>
		/// Every recipe (from `allRecipes`) that hand+field together could complete right now.
		/// TowerPlan.ingredients is matched as a multiset against hand first, then field, tower-plan by
		/// tower-plan - a recipe needing [A3, A3, B3] with A3 in hand and two A3s on the field consumes
		/// the hand A3 first (hand is "free" - it's already yours to take - then only one field A3 is
		/// reserved against the second A3 slot, not both). A recipe with no ingredients (a plain,
		/// non-fusion TowerPlan) never matches.
		/// </summary>
		public static List<FusionMatch> FindCombinables(List<TowerPlan> hand, List<Tower> field, IEnumerable<TowerPlan> allRecipes)
		{
			List<FusionMatch> matches = new();
			foreach (TowerPlan recipe in allRecipes)
			{
				FusionMatch match = TryMatch(recipe, hand, field);
				if (match != null) matches.Add(match);
			}
			return matches;
		}

		static FusionMatch TryMatch(TowerPlan recipe, List<TowerPlan> hand, List<Tower> field)
		{
			if (recipe.ingredients == null || recipe.ingredients.Count == 0) return null;

			List<TowerPlan> remainingHand = new(hand);
			List<Tower> remainingField = new(field);
			FusionMatch match = new FusionMatch { Recipe = recipe };

			foreach (TowerPlan ingredient in recipe.ingredients)
			{
				int handIndex = remainingHand.IndexOf(ingredient);
				if (handIndex >= 0)
				{
					match.FromHand.Add(ingredient);
					remainingHand.RemoveAt(handIndex);
					continue;
				}

				List<Tower> candidates = remainingField.Where(t => t.Plan == ingredient).ToList();
				if (candidates.Count == 0) return null; // Ingredient satisfiable by neither hand nor field - no match.

				match.FromField.Add(candidates);
				remainingField.Remove(candidates[0]); // Provisionally reserve one so a later identical-ingredient slot doesn't double-count it.
			}

			return match;
		}

		/// <summary>Convenience for "is THIS specific hand item part of any combinable recipe" (e.g. per-button highlighting in a mulligan UI) - just FindCombinables filtered down to matches that actually used it.</summary>
		public static List<FusionMatch> FindCombinablesFor(TowerPlan handItem, List<TowerPlan> hand, List<Tower> field, IEnumerable<TowerPlan> allRecipes)
		{
			return FindCombinables(hand, field, allRecipes).Where(m => m.FromHand.Contains(handItem)).ToList();
		}
	}
}
