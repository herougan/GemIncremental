using System.Collections.Generic;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Passives
{
	public interface IPassive
	{
		StatType Type { get; }
		float Multiplier { get; }
		float Bonus { get; }
		ddouble Gate { get; } // When Entity's Stat(type) crosses threshold

		List<Condition> Conditions { get; }
	}
	public class Passive : IPassive
	{
		public StatType Type { get; protected set; }

		public float Multiplier { get; protected set; }

		public float Bonus { get; protected set; }

		public List<Condition> Conditions { get; protected set; }

		public ddouble Gate { get; protected set; }
	}
}