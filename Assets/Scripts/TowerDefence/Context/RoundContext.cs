using System.Collections.Generic;
using Util.Game;

namespace TowerDefence.Context
{

	public interface IRoundContext : IContext
	{

	}

	public class RoundContext : IRoundContext
	{
		public int MonsterCount { get; set; }
		public List<SpawnChain> SpawnChains { get; set; }

		// Float, ddouble
		public void Resolve(string s)
		{

		}
	}
}