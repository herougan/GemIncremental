using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity;
using TowerDefence.Manager;
using UnityEngine;

namespace TowerDefence.Entity.Skills.Buffs
{
	/// <summary>
	/// The "responsible party" for spatial Aura propagation - NOT every Entity in the game polling its
	/// own surroundings for nearby Auras, and NOT one EntityManager.GetEntitiesInRange scan per Aura.
	/// Only entities that actually own a live (non-instance) Aura are ever visited (see Register/
	/// Unregister, called from Aura.RegisterCallbacks/Deregister - the same "only place a Skill goes
	/// live/dies" hook everything else in this codebase uses), and each visited source gets exactly ONE
	/// spatial query per tick interval covering every Aura it owns at once (sized to the widest Range
	/// among them) - "1 round of checking to satisfy all of them," per the design discussion. Each
	/// individual Aura then filters that shared candidate list down to its own (possibly smaller) Range
	/// and does its own conditional-grant/enter/exit bookkeeping (see Aura.Propagate) - this class only
	/// ever does the spatial batching, never the grant/remove decision itself.
	///
	/// Ticked on its own slower interval (TickInterval), not every TickManager.OnTick step - aura range
	/// doesn't need per-fixed-tick precision, and this is exactly the kind of O(sources * range) work
	/// worth throttling independently of the main simulation tick.
	/// </summary>
	public static class AuraPropagationService
	{
		public static float TickInterval = 0.25f;
		static float accumulator;

		// Reference-counted, not a plain HashSet - a source with 2+ Auras must stay registered as long
		// as ANY of them is still alive, so one Aura expiring can't evict a source that still has
		// another one active.
		static readonly Dictionary<IEntity, int> sourceCounts = new();

		static AuraPropagationService()
		{
			TickManager.OnTick += Tick;
		}

		public static void Register(IEntity source)
		{
			if (source == null) return;
			sourceCounts[source] = sourceCounts.GetValueOrDefault(source) + 1;
		}

		public static void Unregister(IEntity source)
		{
			if (source == null || !sourceCounts.TryGetValue(source, out int count)) return;
			if (count <= 1) sourceCounts.Remove(source);
			else sourceCounts[source] = count - 1;
		}

		static void Tick(float dt)
		{
			accumulator += dt;
			if (accumulator < TickInterval) return;
			accumulator = 0f;

			// Snapshot - a Propagate call below can cause an Aura to expire/Deregister mid-loop (Grant/
			// Leave can fire OnExpired synchronously, same reentrancy concern Entity.Tick's own Buffs
			// ToList() already guards against), which would otherwise mutate sourceCounts.Keys while
			// still being enumerated.
			foreach (IEntity source in sourceCounts.Keys.ToList())
			{
				PropagateFrom(source);
			}
		}

		static void PropagateFrom(IEntity source)
		{
			List<IAura> auras = source.Buffs.OfType<IAura>().Where(a => !a.IsInstance).ToList();
			if (auras.Count == 0) return; // Every Aura this source had has since expired - Unregister just hasn't caught up (or the count is still >0 from a race); nothing to do either way.

			Transform sourceTransform = EntityManager.GetTransform(source);
			if (sourceTransform == null) return; // No live Transform (not spawned as a real GameObject, or already destroyed) - can't do a spatial query.

			float maxRange = auras.Max(a => a.Range);
			List<IEntity> candidates = EntityManager.GetEntitiesInRange(sourceTransform.position, maxRange)
				.Where(e => e != source)
				.ToList();

			foreach (IAura aura in auras)
			{
				List<IEntity> inRange = candidates.Where(e => WithinRange(e, sourceTransform.position, aura.Range)).ToList();
				aura.Propagate(source, inRange);
			}
		}

		static bool WithinRange(IEntity entity, Vector3 sourcePosition, float range)
		{
			Transform entityTransform = EntityManager.GetTransform(entity);
			return entityTransform != null && Vector3.Distance(entityTransform.position, sourcePosition) <= range;
		}
	}
}
