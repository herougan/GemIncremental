using System.Collections.Generic;
using TowerDefence.Entity.Tower;

namespace Player
{
	/// <summary>
	/// The tower-choice reroll - "every wave you get to roll a new set of towers" per the design
	/// discussion. Deliberately just Choices + Reroll: whether a Choice is part of a fusion (see
	/// Util.Game.FusionUtil.FindCombinables) is the UI's job to ask for and render, not this class's job
	/// to know about - Mulligan doesn't reach for a "field" reference or recipe list at all, same
	/// "pure data, orchestration lives in the caller" split as everywhere else (EffectController calling
	/// into EntityUtil, not EntityUtil reaching back into a GameManager).
	///
	/// STUB-ish: no reroll cost/token gating yet (an old prototype's Mulligan had a token-spend gate -
	/// see the legacy-mulligan-reroll-mechanic memory - deliberately not carried over here; "every wave"
	/// per this session's design reads as free/automatic, not token-gated). Reroll pool is whatever the
	/// caller passes in - no runtime TowerPlan registry exists yet (EntityDataWindow's TypeCache scan is
	/// Editor-only), so for now that's just whatever list a scene's demo/UI script hands it.
	/// </summary>
	public class Mulligan
	{
		// 5, not 3 - for testing purposes only (makes hand-only fusion matches - all 3 ingredients drawn
		// into the same hand at once - show up often enough to actually see the "extra pick" button
		// without a dozen rerolls). Dial back down toward 3 once real balance is being tuned.
		public int HandSize = 5;
		public List<TowerPlan> Choices { get; private set; } = new();

		/// <summary>Replaces Choices with HandSize random picks from pool (with replacement - the same TowerPlan can appear twice in one hand). No-op (empty Choices) if pool is null/empty.</summary>
		public void Reroll(IReadOnlyList<TowerPlan> pool, System.Random rng = null)
		{
			Choices = new List<TowerPlan>();
			if (pool == null || pool.Count == 0) return;

			rng ??= new System.Random();
			for (int i = 0; i < HandSize; i++) Choices.Add(pool[rng.Next(pool.Count)]);
		}
	}
}
