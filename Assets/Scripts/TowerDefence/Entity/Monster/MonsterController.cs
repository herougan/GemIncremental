using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity.Skills;
using TowerDefence.Manager;
using TowerDefence.Map;
using TowerDefence.Stats;
using UnityEngine;

namespace TowerDefence.Entity.Monster
{
	public class MonsterController : EntityController
	{
		public Monster monster { get; private set; }

		/// <summary>If Init(Monster) is never called explicitly (e.g. dropped straight into a scene by hand, not spawned via EntityManager/EntityWaveManager), Start builds one from this instead - a prefab that "just works" on its own rather than needing a spawner script for every quick test.</summary>
		[Header("Default (used only if Init isn't called first)")]
		public MonsterPlan DefaultPlan;

		/// <summary>Sets the typed monster reference and runs the base EntityController wiring (RegisterCallbacks + Position Registry) - nothing did either of these before, so a MonsterController on a real GameObject was never actually connected to its Monster.</summary>
		public void Init(Monster monster)
		{
			this.monster = monster;
			Initiate(monster);
		}

		void Start()
		{
			if (monster == null && DefaultPlan != null) Init(EntityManager.SpawnMonster(DefaultPlan));
		}

		#region Movement (follows a path handed in at spawn time - see MapGenerator.Paths)

		List<Node> path;
		int nodeIndex;

		/// <summary>How many Nodes are left before this Monster reaches the Destination - "closest to the end" for Tower.Targetting.distance (see TowerController.Prioritise). int.MaxValue (sorts last, never picked over any real Monster) if no path was ever assigned.</summary>
		public int RemainingNodes => path == null ? int.MaxValue : path.Count - nodeIndex;

		/// <summary>Assigns the path this Monster walks (typically MapGenerator.Paths[sourcePosition] - the whole resolved Source -> Destination route, handed in whole at spawn time rather than looked up per-frame) and snaps to its first Node so it starts exactly on the Source tile rather than wherever it happened to be instantiated.</summary>
		public void SetPath(List<Node> path)
		{
			this.path = path;
			nodeIndex = 0;
			if (path != null && path.Count > 0) transform.position = path[0].WorldPosition;
		}

		// Subscribed for exactly this GameObject's lifetime (OnEnable/OnDisable, not Init/OnDestroy) - a
		// MonsterController with no Path yet (Init not called, or SetPath never called) just no-ops every
		// tick in Move below, so subscribing early is harmless.
		void OnEnable() => TickManager.OnTick += Move;
		void OnDisable() => TickManager.OnTick -= Move;

		/// <summary>
		/// Steps this Monster along `path` by Speed*dt, called from TickManager's fixed-timestep OnTick -
		/// NOT Update - specifically so movement advances by the same fixed distance every logic step
		/// regardless of render framerate (see TickManager's own doc comment for why that's not the same
		/// guarantee plain Time.deltaTime-scaled Update movement gives you).
		/// Consumes the tick's whole movement budget across as many Nodes as it covers in one step
		/// (the `while`, not a single `if`) rather than capping at one Node per tick - otherwise a fast
		/// Monster or a low tick rate could stall against a Node it should have already passed.
		/// </summary>
		void Move(float dt)
		{
			if (path == null || nodeIndex >= path.Count) return;

			float speed = (float)(double)monster.GetStat(StatType.Speed);
			float remaining = speed * dt;

			while (remaining > 0f && nodeIndex < path.Count)
			{
				Vector3 target = path[nodeIndex].WorldPosition;
				Vector3 toTarget = target - transform.position;
				float distance = toTarget.magnitude;

				if (distance <= remaining)
				{
					transform.position = target;
					remaining -= distance;
					nodeIndex++;
				}
				else
				{
					transform.position += toTarget.normalized * remaining;
					remaining = 0f;
				}
			}

			if (nodeIndex >= path.Count) ReachedDestination();
		}

		/// <summary>
		/// TriggerType.OnReached, not OnDeath - reaching the Destination is a distinct outcome from being
		/// killed (EntityWaveManager already decrements MonstersAlive on either). STUB: what actually
		/// happens on a leak (player HP/lives cost, a currency penalty, ...) is undecided - nothing
		/// currently listens for OnReached at all, this just fires it and cleans up the GameObject.
		/// </summary>
		void ReachedDestination()
		{
			path = null;
			monster.RaiseEvent(TriggerType.OnReached, new TriggerContext { Entity = monster });
			Destroy(gameObject);
		}

		#endregion Movement
	}
}
