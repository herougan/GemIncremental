using System;
using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity.Skills;

namespace TowerDefence.Entity.Events
{
	/// <summary>
	/// Defines an interface for subscribing, unsubscribing, and dispatching entity-related events
	/// based on <see cref="TriggerType"/>.
	/// </summary>
	public interface IEntityRouter
	{
		/// <summary>
		/// Subscribes a listener to a specific trigger type.
		/// </summary>
		/// <param name="type">The type of trigger to subscribe to.</param>
		/// <param name="listener">The action to invoke when the trigger occurs.</param>
		void Subscribe(TriggerType type, Action<TriggerContext> listener);

		/// <summary>
		/// Unsubscribes a listener from a specific trigger type.
		/// </summary>
		/// <param name="type">The type of trigger to unsubscribe from.</param>
		/// <param name="listener">The action to remove from the trigger's invocation list.</param>
		void Unsubscribe(TriggerType type, Action<TriggerContext> listener);

		/// <summary>
		/// Dispatches a trigger event to all subscribed listeners for the trigger type.
		/// </summary>
		/// <param name="context">The context containing trigger information.</param>
		void Dispatch(TriggerContext context);
	}


	/// <summary>
	/// Implements <see cref="IEntityRouter"/> to manage event listeners and dispatch events
	/// for different <see cref="TriggerType"/>s within an entity system.
	/// </summary>
	public class EntityRouter : IEntityRouter
	{
		private readonly Dictionary<TriggerType, Action<TriggerContext>> _listeners = new();

		public void Subscribe(TriggerType type, Action<TriggerContext> listener)
		{
			if (!_listeners.ContainsKey(type))
				_listeners[type] = delegate { };
			_listeners[type] += listener;
		}

		public void Unsubscribe(TriggerType type, Action<TriggerContext> listener)
		{
			if (_listeners.ContainsKey(type))
				_listeners[type] -= listener;
		}

		public void Dispatch(TriggerContext context)
		{
			if (_listeners.TryGetValue(context.TriggerType, out var listeners))
				listeners?.Invoke(context);
		}
	}
}