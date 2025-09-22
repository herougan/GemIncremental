namespace TowerDefence.Entity.Skills.Buffs
{
	public class StatusBuff : Buff
	{
		public ddouble Value { get; private set; }
		public StatusType StatusType { get; private set; }

		public StatusBuff(StatusType statusType, ddouble value = default(ddouble))
		{
			StatusType = statusType;
			Value = value;
		}

		public Add(Resistance resist, ddouble value)
		{

		}
	}

	public enum StatusType
	{
		Stun,
		Poison,
		Freeze,
		Slow,
		Blind,
		Knockback,
		Burn,
		Paralyse,
		Confuse,
		Silence,
		Weak,
		Charm,
		Curse,
	}
}