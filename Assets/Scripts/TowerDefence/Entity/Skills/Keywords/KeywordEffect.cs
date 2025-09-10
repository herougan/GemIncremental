namespace TowerDefence.Entity.Skills.Keywords
{
	public interface IKeywordEffect
	{

	}

	public class KeywordEffect : IKeywordEffect
	{
		// Implementation of keyword effect logic
		public KeywordType Type { get; private set; }
		public float Value { get; private set; }

		public KeywordEffect(KeywordType type, float value)
		{
			Type = type;
			Value = value;
		}
	}

	public enum KeywordType
	{

	}
}