using System;
using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity.Skills;

namespace Util.Events
{
	/// <summary>
	/// The bus itself: publish/subscribe, nothing else - no bookkeeping baked in, no knowledge of who's
	/// listening. Entity.RaiseEvent is the only publisher; everyone else (EntityManager, GameManager,
	/// EntityWaveManager, BattleTracker, a test) is just one more subscriber, opting in independently.
	///
	/// Two ways to subscribe, same as Entity's own per-instance TriggerType dictionary (GetEvent/
	/// SubscribeEvent) generalized to the global scale:
	///  - Subscribe(type, handler) - only invoked for that exact TriggerType. Use this for anything that
	///    cares about a specific slice (BattleTracker only wants OnHit; EntityWaveManager only wants
	///    OnDeath/OnReached) - no more "runs on every single event in the game, immediately checks
	///    ctx.TriggerType and bails" per subscriber, which is what every one of those used to do.
	///  - OnAnyEvent - genuinely global observers that want *everything*, unfiltered: EntityManager's own
	///    recentEvents/eventCounts bookkeeping and GameManager's event-count tally both deliberately want
	///    "every TriggerType, so I can count them all" - that's not a filtering problem to solve, it's
	///    the actual requirement, so this stays for exactly that case rather than forcing 60-odd
	///    individual Subscribe calls to fake the same thing.
	///
	/// Static, not a MonoBehaviour singleton "launched" by GameManager - a static class needs no live
	/// GameObject/scene to exist at all, which is what let a pure-C# context (EntityEventChainSmokeTest,
	/// no scene/Play Mode involved) publish and subscribe in the first place. A GameManager-owned
	/// instance would reintroduce exactly the Instance-null race that design avoided - GameManager
	/// subscribing its own listener in Start() is still "GameManager using the bus," it just doesn't
	/// need to own the bus's existence to do that.
	/// </summary>
	public static class EntityEventBus
	{
		public static event Action<TriggerContext> OnAnyEvent = delegate { };

		static readonly Dictionary<TriggerType, Action<TriggerContext>> handlers = new();

		/// <summary>The one legal way to attach a type-filtered handler - do not read-modify-write a dictionary entry yourself, same reason Entity.SubscribeEvent exists instead of `GetEvent(type) += handler`.</summary>
		public static void Subscribe(TriggerType type, Action<TriggerContext> handler)
		{
			handlers[type] = handlers.TryGetValue(type, out var existing) ? existing + handler : handler;
		}

		/// <summary>Symmetric detach for Subscribe.</summary>
		public static void Unsubscribe(TriggerType type, Action<TriggerContext> handler)
		{
			if (!handlers.TryGetValue(type, out var existing)) return;
			existing -= handler;
			if (existing == null) handlers.Remove(type);
			else handlers[type] = existing;
		}

		public static void Publish(TriggerContext ctx)
		{
			if (handlers.TryGetValue(ctx.TriggerType, out var handler)) handler.Invoke(ctx);
			OnAnyEvent.Invoke(ctx);
		}
	}
}
