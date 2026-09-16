using TowerDefence.Entity.Skills;
using Util.Maths;

namespace TowerDefence.Stats
{
	// ===== Interfaces =====
	/*
		An IStatMod modifies an Entity's Stat.
		It informs the Entity of the source of the modification - many sources can each contribute their
		own StatMods to the same stat, and because each one remembers where it came from, adding/removing
		a specific source's contribution (a Skill learned/unlearned, a Buff applied/cleansed, a Foundry
		upgrade granted) doesn't disturb anyone else's. See Entity's StatType -> List<IStatMod> registry.

		Application order is Operation-driven, not insertion-order-driven: Add mods first, then Multiply,
		then Exponent (see EntityUtil.ApplyStatMods) - that's what makes the result independent of what
		order mods happened to be registered/deregistered in.
	*/
	public interface IStatMod
	{
		public ddouble Value { get; }
		public StatType StatType { get; }
		public MathOperation Operation { get; }
		public bool IsPositive { get; }
		public ISource Source { get; }
	}

	public class StatMod : IStatMod
	{
		public ddouble Value { get; }
		public StatType StatType { get; }
		public MathOperation Operation { get; }
		public bool IsPositive { get; }
		public ISource Source { get; }

		public StatMod(ddouble value, StatType statType, MathOperation op, ISource source = null)
		{
			Value = value;
			StatType = statType;
			Operation = op;
			IsPositive = (double)value > 0 && MathsLib.IsPositive(op);
			Source = source;
		}
	}
}
