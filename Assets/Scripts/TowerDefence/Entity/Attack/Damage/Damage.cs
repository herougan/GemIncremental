using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Attack.Damage
{


	public interface IDamage
	{
		StatType StatType { get; }
		ddouble Value { get; }
	}

	public class Damage : IDamage
	{
		public StatType StatType { get; private set; }
		public ddouble Value { get; private set; }
	}
}