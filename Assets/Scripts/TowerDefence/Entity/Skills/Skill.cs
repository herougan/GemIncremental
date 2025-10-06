
using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using Util.Maths;


namespace TowerDefence.Entity.Skills
{
	public interface ISkill : ISource
	{
		// Info
		SkillPlan Plan { get; }
		bool IsPositive { get; }

		// Data
		public ddouble Scale { get; }

		// State
		List<CountdownTimer> Timers { get; }

		// ===== Init =====
		// public void SetSelfInit();

		// ===== Skill Effects =====
		// public void ApplyAction(IEntity source, IEntity target);

		// Events
		public event Action<IEntity> OnLearned; // Applied on

		// Methods
		public void Tick(float t);
	}

	[Serializable]
	public class Skill : ISkill
	{
		// Info
		public ddouble Scale { get; protected set; }


		// Meta
		public bool IsPassive { get; protected set; }
		public bool IsPositive { get; protected set; }
		public bool ForMonster { get; protected set; }
		public bool ForTower { get; protected set; }

		// Events
		public event Action<IEntity> OnLearned;

		// State
		public List<CountdownTimer> Timers { get; protected set; }

		// Main Skill Info

		public SkillPlan Plan { get; protected set; }

		public IEntity Caster { get; protected set; }

		public Skill(SkillPlan plan)
		{
			this.Plan = plan;
		}

		public Skill(SkillPlan plan, ddouble scale = default(ddouble))
		{
			this.Plan = plan;
			this.Scale = scale;
		}

		#region Methods

		public void Tick(float t)
		{
			// Tick timers
			foreach (var timer in Timers)
			{
				timer.Tick(t);
			}
		}

		#endregion Methods
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