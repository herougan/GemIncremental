using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Attack.Damage
{


	public interface IDamage
	{
		StatType StatType { get; }
		ddouble Value { get; }
		IEntity Attacker { get; }
		IEntity Target { get; }
	}

	public class Damage : IDamage
	{
		public StatType StatType { get; private set; }
		public ddouble Value { get; private set; }
		public IEntity Attacker { get; private set; }
		public IEntity Target { get; private set; }

		public Damage(StatType statType, ddouble value, IEntity attacker)
		{
			this.StatType = statType;
			this.Value = value;
			this.Attacker = attacker;
		}

		public void SetTarget(IEntity target)
		{
			this.Target = target;
		}
	}
}