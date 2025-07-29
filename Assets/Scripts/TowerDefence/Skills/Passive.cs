using System.Collections.Generic;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Skills
{
	public interface IPassive
	{
		StatType Type { get; }
		float Multiplier { get; }
		float Bonus { get; }

		List<Condition> Conditions { get; }
	}
	public class Passive : IPassive
	{
		public StatType Type => throw new System.NotImplementedException();

		public float Multiplier => throw new System.NotImplementedException();

		public float Bonus => throw new System.NotImplementedException();

		public List<Condition> Conditions => throw new System.NotImplementedException();
	}
}