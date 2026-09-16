using System;
using TowerDefence.Context;

namespace Util.Events
{
	/// <summary>
	/// STUB - a second bus, separate from EntityEventBus, for Player-meta events (skill learned,
	/// artefact found, achievement unlocked, ...) that aren't tied to a specific combat Entity/hit.
	/// Same shape as EntityEventBus (static, publish/subscribe, no bookkeeping baked in) for the same
	/// reason: no live GameObject/scene needed to exist or to be used.
	///
	/// Reuses TriggerContext as the payload rather than inventing a separate PlayerEventContext - Player
	/// now has a real dummy IEntity (see DummyEntityPlan/Player.PlayerEntity) that can populate
	/// ctx.Entity, so the same shape fits without forcing a parallel context type. Nothing publishes to
	/// this yet - there's no Player-side equivalent of Entity.RaiseEvent that would call Publish.
	/// </summary>
	public static class PlayerEventBus
	{
		public static event Action<TriggerContext> OnAnyEvent = delegate { };

		public static void Publish(TriggerContext ctx) => OnAnyEvent.Invoke(ctx);
	}
}
