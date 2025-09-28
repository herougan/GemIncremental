using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Context;

namespace TowerDefence.Entity.Skills.Effects
{
	public interface IActionHandler
	{
		ActionType Type { get; }

		void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, Action effect);
		void Rollback(in GameContext context, Action effect);
		void RemoveEffect(in GameContext context, Action effect);
	}

	public class ActionHandler : IActionHandler
	{
		public ActionType Type { get; protected set; }

		public ActionHandler()
		{
		}

		public virtual void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, Action effect)
		{
			// Default implementation does nothing
		}

		/// <summary>
		/// Note: Most actions cannot be rolled back.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="effect"></param>
		public virtual void Rollback(in GameContext context, Action effect)
		{
			// Default implementation does nothing

			// Most actions cannot be rolledback.
		}

		public virtual void RemoveEffect(in GameContext context, Action effect)
		{
			// Default implementation does nothing
		}
	}
}