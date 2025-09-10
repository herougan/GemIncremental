
using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using Util.Maths;


namespace TowerDefence.Entity.Skills
{
	public interface ISkill : IAffect
	{
		// Data
		public ddouble Scale { get; }

		// ===== Init =====
		// public void SetSelfInit();

		// ===== Skill Effects =====
		// public void ApplyAction(IEntity source, IEntity target);
	}

	[Serializable]
	public class Skill : ISkill
	{
		public int Level;
		public ddouble Scale { get; set; }
		public int priority { get; set; }

		//
		public bool IsPassive { get; set; }
		public bool IsPositive { get; set; }
		public bool ForMonster { get; set; }
		public bool ForTower { get; set; }


		public event Action<ISource> OnApplied;

		// Main Skill Info
		public SkillData Data { get; set; }

		public IEntity Entity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ISource Source => throw new NotImplementedException();

		public IEntity Affected => throw new NotImplementedException();

		public Skill(SkillData data)
		{
			this.Data = data;
		}
	}

	public interface ISource
	{
		public List<IAffect> Affects { get; }
	}

	// Affects point to source. Do source point back to affects? Do affects tell you what they are affecting?!

	public interface IAffect
	{
		public ISource Source { get; }
		public IEntity Affected { get; }
	}
}