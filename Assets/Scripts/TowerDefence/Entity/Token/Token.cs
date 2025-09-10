namespace TowerDefence.Entity.Token
{
	public interface IToken
	{
		int Number { get; }
		TokenType Type { get; }
	}

	public class Token : IToken
	{
		public int Number { get; private set; }
		public TokenType Type { get; private set; }
	}

	public enum TokenType
	{

	}
}