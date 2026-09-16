using TowerDefence.Entity;
using UnityEngine;

namespace TowerDefence.Manager
{
	/// <summary>
	/// The global tick loop: drives every registered Entity's Tick(dt) (EntityManager.GetAllEntities -
	/// the Position Registry, Monsters and Towers alike) plus anything else that needs the exact same
	/// per-step dt, via OnTick, from FixedUpdate rather than Update.
	///
	/// Why FixedUpdate: it's Unity's own fixed-timestep callback (Time.fixedDeltaTime, default 0.02s -
	/// Project Settings > Time), which is exactly the "decouple simulation steps from render framerate"
	/// accumulator pattern this needed, for free, with no hand-rolled accumulator - normally reserved for
	/// physics, but nothing here touches Rigidbodies/PhysX (see CLAUDE.md: no physics system exists), so
	/// there's no competing use for it. Every machine runs the same number of equal-size steps per second
	/// of game time no matter how fast/slow it renders - this is what makes Buff/Status expiry, StatMod
	/// periodic ticks, AND Monster movement (MonsterController subscribes to OnTick - see SetPath/Move)
	/// all framerate-independent: a monster crosses the same distance in the same number of ticks whether
	/// the game renders at 30fps or 300fps, not just "on average" the way scaling movement by Time.
	/// deltaTime in Update alone would (deltaTime-scaled Update movement keeps the *average* speed
	/// constant, but the exact step boundaries - and therefore exactly when a waypoint is reached - still
	/// shift with framerate; a fixed step size doesn't have that wobble). It also means a future "fast
	/// forward" (run N ticks back-to-back with no frame render in between, for the "simulate a billion
	/// battles in minutes" idea from earlier) is just calling the same Tick method in a loop - the
	/// simulation was never coupled to wall-clock rendering to begin with.
	/// This is the "sync strategy" the TickManager was waiting on - see the memory this resolves.
	/// </summary>
	public class TickManager : MonoBehaviour
	{
		public static TickManager Instance { get; private set; }

		void Awake()
		{
			if (Instance == null) Instance = this;
			else if (Instance != this) Destroy(this);
		}

		/// <summary>Fires every fixed step, after every Entity's own Tick - for anything (MonsterController movement, a future fast-forward driver, ...) that wants the same synced dt without going through the Entity/Tick pipeline.</summary>
		public static event System.Action<float> OnTick = delegate { };

		void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;
			foreach (IEntity entity in EntityManager.GetAllEntities()) entity.Tick(dt);
			OnTick.Invoke(dt);
		}
	}
}
