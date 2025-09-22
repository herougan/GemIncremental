using System;
using TowerDefence.Library;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Buffs
{
	public class StatusBuff : Buff
	{
		public StatusType StatusType { get; private set; }

		public StatusBuff(StatusType statusType, ddouble scale = default(ddouble)) : base(SkillsLib.StatusPlans[statusType], scale)
		{
			StatusType = statusType;
			BuffStackType = SkillsLib.StatusStackTypes[statusType];
		}

		public void Add(Resistance resist, ddouble value)
		{
			throw new NotImplementedException();
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