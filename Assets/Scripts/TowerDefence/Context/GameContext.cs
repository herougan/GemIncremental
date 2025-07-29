namespace TowerDefence.Game
{
	public class GameContext
	{


		// Float, ddouble
		public void Resolve(string s)
		{

		}
	}

	public class GameContextBuilder
	{
		public GameContext Build()
		{
			return new GameContext();
		}

		public GameContextBuilder WithSomeSetting(string setting)
		{
			// Set some context settings
			return this;
		}
	}
}