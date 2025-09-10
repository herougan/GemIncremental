using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects
{
	/// <summary>
	/// Accounts for all IDepletable reductions.
	/// </summary>
	public class Damage
	{
		public ddouble DamageDone { get; set; }
		public IDepletable Stat { get; set; }
	}
}