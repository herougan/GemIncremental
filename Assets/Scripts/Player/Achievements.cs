using System.Collections.Generic;

namespace Player
{
	/// <summary>Empty for now, same as Condition.cs's CounterType/TagType/MetaType - filled in once real achievements are designed.</summary>
	public enum AchievementType
	{
	}

	public class Achievements
	{
		public HashSet<AchievementType> Unlocked { get; } = new();

		public bool IsUnlocked(AchievementType type) => Unlocked.Contains(type);

		public void Unlock(AchievementType type) => Unlocked.Add(type);
	}
}
