using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity.Skills;

namespace Player
{
	/// <summary>
	/// A skill the player has personally learned - distinct from Skill (TowerDefence/Entity/Skills/
	/// Skill.cs), which is a *runtime instance* of a skill live on a specific Monster/Tower right now.
	/// PlayerSkill is "does the player have access to this at all, and how upgraded is it" (persistent,
	/// cross-run meta state); Skill is "this Effect/Trigger is currently wired onto this Entity"
	/// (per-match runtime state). A PlayerSkill still points at the same SkillPlan data (Effects/
	/// Triggers/Actions) - only the ownership/progression layer is new here.
	///
	/// How a PlayerSkill actually gets onto a Tower at run start (presumably alongside/instead of
	/// TowerPlan.InitSkills) isn't decided yet - that's the next open question once this exists.
	/// </summary>
	public class PlayerSkill
	{
		public SkillPlan Plan { get; }
		public int Level { get; set; } = 1;

		public PlayerSkill(SkillPlan plan)
		{
			Plan = plan;
		}
	}

	/// <summary>
	/// Skills the player has learned, and which of those are slotted into the loadout they're bringing
	/// into their next TD run. Learned/Loadout are deliberately separate lists (not e.g. an "IsEquipped"
	/// flag on PlayerSkill) - loadout slot limits/rules are still open design, so this keeps that
	/// decision from leaking into PlayerSkill's own shape.
	/// </summary>
	public class PlayerSkills
	{
		public List<PlayerSkill> Learned { get; } = new();
		public List<PlayerSkill> Loadout { get; } = new();

		public bool HasLearned(SkillPlan plan) => Learned.Any(s => s.Plan == plan);

		public PlayerSkill Learn(SkillPlan plan)
		{
			PlayerSkill existing = Learned.FirstOrDefault(s => s.Plan == plan);
			if (existing != null) return existing;

			PlayerSkill learned = new PlayerSkill(plan);
			Learned.Add(learned);
			return learned;
		}

		public void AddToLoadout(PlayerSkill skill)
		{
			if (Learned.Contains(skill) && !Loadout.Contains(skill)) Loadout.Add(skill);
		}

		public void RemoveFromLoadout(PlayerSkill skill) => Loadout.Remove(skill);
	}
}
