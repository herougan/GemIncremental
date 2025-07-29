using System;
using TowerDefence.Expirable;

namespace TowerDefence.Skills
{
	public interface IAura
	{
		// Define properties or methods that an Aura should have

		public SkillPlan Data { get; }
		public float Range { get; }
		public bool IsAffectSelf { get; }
	}

	public class Aura : IAura
	{
		// Implementation of aura logic
		public float Range => throw new System.NotImplementedException();

		public bool IsAffectSelf => throw new System.NotImplementedException();

		SkillPlan IAura.Data => throw new System.NotImplementedException();
	}

	public class AuraInstance
	{
		public Aura SourceAura { get; set; }


		public event Action<AuraInstance> OnLostAura;
	}
}