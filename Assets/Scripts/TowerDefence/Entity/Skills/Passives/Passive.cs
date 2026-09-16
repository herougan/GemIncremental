using System.Collections.Generic;
using TowerDefence.Stats;

namespace TowerDefence.Entity.Skills.Passives
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
		public StatType Type { get; protected set; }

		// Defaults to 1 (neutral), not 0 - see Skill.ApplyPassive, which does
		// `value = value * Multiplier + Bonus`. A never-configured Multiplier defaulting to 0 would
		// zero out the stat entirely the moment this Passive is active, which is never what's intended.
		public float Multiplier { get; protected set; } = 1;

		public float Bonus { get; protected set; }

		public List<Condition> Conditions { get; protected set; } = new List<Condition>();

		public Passive(StatType type, float multiplier = 1, float bonus = 0, List<Condition> conditions = null)
		{
			Type = type;
			Multiplier = multiplier;
			Bonus = bonus;
			Conditions = conditions ?? new List<Condition>();
		}
	}
}
