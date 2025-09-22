
using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using Util.Maths;


namespace TowerDefence.Entity.Skills
{
	public interface ISkill : ISource
	{
		// Info
		int Level { get; }

		// Meta
		IEntity Caster { get; }

		// Data
		public ddouble Scale { get; }

		// ===== Init =====
		// public void SetSelfInit();

		// ===== Skill Effects =====
		// public void ApplyAction(IEntity source, IEntity target);

		// Events
		public event Action<IEntity> OnApplied; // Applied on
	}

	[Serializable]
	public class Skill : ISkill
	{
		// Info
		public int Level { get; set; }
		public ddouble Scale { get; set; }
		public int Priority { get; set; }

		// Meta
		public bool IsPassive { get; set; }
		public bool IsPositive { get; set; }
		public bool ForMonster { get; set; }
		public bool ForTower { get; set; }

		// Events
		public event Action<IEntity> OnApplied;

		// Main Skill Info

		public SkillPlan Plan { get; set; }

		public IEntity Entity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ISource Source => throw new NotImplementedException();

		public IEntity Affected => throw new NotImplementedException();

		public IEntity Caster => throw new NotImplementedException();

		public Skill(SkillPlan plan)
		{
			this.Plan = plan;
		}

		public Skill(SkillPlan plan, ddouble scale = default(ddouble))
		{
			this.Plan = plan;
			this.Scale = scale;
		}
	}

	// ===== Spirce and Affects =====
	public interface ISource
	{
		public IEntity Caster { get; }
		// public List<IAffect> Affects { get; }
	}

	// Affects point to source. Do source point back to affects? Do affects tell you what they are affecting?!

	public interface IAffect
	{
		public ISource Source { get; }
		public IEntity Affected { get; }
	}
}