using System.Collections.Generic;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Token;
using TowerDefence.Stats;
using Util.Maths;
using Util.Serialisation;

namespace TowerDefence.Entity
{
	/// <summary>
	/// Minimal IEntityPlan for non-combat "entities" that need to exist only so something with a
	/// Buff-granting mechanism can point Caster at them - e.g. WorldProgress.ApplyBuff and Player's
	/// equivalent both need a real IEntity to build a Buff(plan, caster) with, and neither the World nor
	/// the Player is a combat participant with its own real Plan asset. "Easier to code if we create a
	/// dummy entity" per the design discussion - no stats/skills of its own, just an identity to hang a
	/// Caster reference on.
	/// </summary>
	public class DummyEntityPlan : IEntityPlan
	{
		public SerialisableGuid Guid { get; } = new SerialisableGuid(System.Guid.NewGuid());
		public string Name { get; set; } = "Dummy";

		public ddouble StartingHealth => default;
		public List<StatEntry> StatEntries { get; } = new List<StatEntry>();
		public List<StatusEntry> StatusEntries { get; } = new List<StatusEntry>();
		public List<ElementEntry> ElementEntries { get; } = new List<ElementEntry>();
		public ResourceBlock ResourceBlock { get; } = new ResourceBlock();
		public List<IToken> StartingTokens { get; } = new List<IToken>();
		public List<BuffPlan> InitBuffs { get; } = new List<BuffPlan>();
		public List<SkillPlan> InitSkills { get; } = new List<SkillPlan>();
	}
}
