using TowerDefence.Skills;

namespace TowerDefence.Game.EffectHandler
{
	public interface IEffectHandler
	{
		public ActionType Type { get; }

		public void ApplyEffect(GameContext context, Action effect);
		public void Rollback(GameContext context, Action effect);
		public void RemoveEffect(GameContext context, Action effect);
	}

	public class StatEffectHandler : IEffectHandler
	{
		public ActionType Type => ActionType.Stat;

		public void ApplyEffect(GameContext context, Action effect)
		{
			// Default implementation for applying effects
		}

		public void Rollback(GameContext context, Action effect)
		{
			// Default implementation for rolling back effects

			// Effectively rollsback effects caused by this action
		}

		public void RemoveEffect(GameContext context, Action effect)
		{
			// Default implementation for removing effects

			// Removes attached effect but those not recalculate stats
		}
	}
}