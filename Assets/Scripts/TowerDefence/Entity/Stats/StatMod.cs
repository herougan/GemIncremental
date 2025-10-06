using Util.Maths;

namespace TowerDefence.Stats
{
	// ===== Interfaces =====
	/*
		An IModification modifiers an Entity's Stat.
		It informs the Entity of the source of the modification.

		StatMod ensures this single change of magnitude is reversible. However, since the order in which StatMods are applied matters,
		StatMods are not sufficient for keyboard playback.
	*/
	public interface IStatMod
	{
		public double Value { get; }
		public StatType StatType { get; }
		public MathOperation Operation { get; }
		public bool IsPositive { get; }
	}

	public class StatMod : IStatMod
	{
		public double Value { get; }
		public StatType StatType { get; }
		public MathOperation Operation { get; }
		public bool IsPositive { get; }

		public StatMod(double value, StatType statType, MathOperation op)
		{
			Value = value;
			StatType = statType;
			Operation = op;
			IsPositive = (value > 0) && MathsLib.IsPositive(op) /*? 1 : 0*/;
		}
	}
}
