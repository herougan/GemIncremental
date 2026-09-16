using System;
using TowerDefence.Entity;
using UnityEngine;

namespace TowerDefence.Projectile
{
	/// <summary>
	/// Drives one live projectile's flight. Launch() decides whether this shot will actually connect
	/// (once, up front - see ComputeWillHit) rather than discovering it via real collision each frame;
	/// Update() just animates toward whatever outcome was already decided. This is deliberate: the same
	/// ComputeWillHit formula can be called directly (no GameObject/Update loop at all) by a bulk
	/// balance-simulation harness, so "how many shots landed across a billion runs" never needs to
	/// actually simulate flight - only real-time visual play does.
	///
	/// No stored destination Vector3 - a homing shot re-aims at the target's live position every frame
	/// (via EntityManager's Position Registry); a non-homing shot just flies at whatever angle it
	/// launched with (see Launch's initialAngleDegrees) and lives or dies by ComputeWillHit's verdict,
	/// same as real ballistics rather than "fly toward a pre-computed point."
	///
	/// Whoever calls Launch is responsible for the ActionHandler-side follow-through (ApplyDamage,
	/// Attack/GotHit) - see ProjectileActionHandler, which subscribes to OnReach to do exactly that.
	/// This class only knows about flight and its own VFX.
	/// </summary>
	public class ProjectileController : MonoBehaviour
	{
		// Projectile Info
		public GameObject ProjectileObject { get; set; }
		public Projectile Projectile { get; set; }

		// Events
		public event Action<IProjectile, IEntity> OnReach;
		public event Action<IProjectile, IEntity> OnPassthrough;
		public event Action<IProjectile> OnDestroy;
		public event Action<IProjectile, IEntity> OnExpire;

		ProjectilePlan plan;
		IEntity source;
		IEntity target;
		Vector3 direction;
		bool willHit;
		float elapsed;

		/// <summary>
		/// initialAngleDegrees is a flat angle (around Y, in a top-down 2D-on-a-plane setup) - whoever
		/// calls Launch decides how to compute it (typically aimed at the target's position at the
		/// moment of firing - see ProjectileActionHandler), this class doesn't care where it came from.
		/// </summary>
		public void Launch(ProjectilePlan plan, IEntity source, IEntity target, Vector3 sourcePosition, float initialAngleDegrees)
		{
			this.plan = plan;
			this.source = source;
			this.target = target;

			Projectile = new Projectile { ProjectileObject = gameObject, Speed = plan.Speed };
			ProjectileObject = gameObject;

			transform.position = sourcePosition;
			direction = Quaternion.Euler(0, initialAngleDegrees, 0) * Vector3.forward;
			transform.rotation = Quaternion.LookRotation(direction);

			willHit = plan.Homing || ComputeWillHit(source, target, plan);
			elapsed = 0;

			if (plan.ReleaseEffect != null) Instantiate(plan.ReleaseEffect, sourcePosition, Quaternion.identity);
			if (plan.TrailEffect != null) Instantiate(plan.TrailEffect, transform.position, Quaternion.identity, transform);
		}

		void Update()
		{
			if (plan == null) return; // Launch hasn't run yet

			elapsed += Time.deltaTime;
			if (elapsed >= plan.TimeToDie)
			{
				Expire();
				return;
			}

			// Homing re-aims at the target's live position every frame - EntityManager's Position
			// Registry is what makes "live" possible for a plain IEntity. Falls back to the last known
			// direction if nothing's registered (e.g. the target's GameObject was destroyed already).
			if (plan.Homing)
			{
				Transform targetTransform = EntityManager.GetTransform(target);
				if (targetTransform != null) direction = (targetTransform.position - transform.position).normalized;
			}

			transform.position += direction * plan.Speed * Time.deltaTime;

			if (willHit && HasReachedTarget()) Arrive();
		}

		bool HasReachedTarget()
		{
			if (target == null) return false;
			Transform targetTransform = EntityManager.GetTransform(target);
			if (targetTransform == null) return false;
			return Vector3.Distance(transform.position, targetTransform.position) <= plan.Size;
		}

		void Arrive()
		{
			if (plan.HitEffect != null) Instantiate(plan.HitEffect, transform.position, Quaternion.identity);
			OnReach?.Invoke(Projectile, target);
			OnDestroy?.Invoke(Projectile);
			Destroy(gameObject); // no pooling yet - see the pooling discussion; this is the one line that'd change
		}

		void Expire()
		{
			// Either a miss (willHit was false, so HasReachedTarget was never going to fire) or a homing
			// shot whose target died mid-flight - either way, TimeToDie is what cleans it up.
			OnPassthrough?.Invoke(Projectile, target);
			OnExpire?.Invoke(Projectile, target);
			OnDestroy?.Invoke(Projectile);
			Destroy(gameObject);
		}

		/// <summary>
		/// Decides, once, whether this shot connects - combining a physics term (this projectile's
		/// Speed relative to the target's Speed stat: a slow shot against a fast target is more likely
		/// to miss) with a stat term (Accuracy vs DodgeChance). Static and side-effect-free so a bulk
		/// simulator can call this directly, a billion times, with no GameObject/Update loop involved.
		/// The exact curve here is a placeholder - structure, not balance; expect this to be reshaped
		/// once real numbers are in.
		/// </summary>
		public static bool ComputeWillHit(IEntity source, IEntity target, ProjectilePlan plan)
		{
			float targetSpeed = (float)(double)target.GetStat(TowerDefence.Stats.StatType.Speed);
			float speedFactor = Mathf.Clamp01(plan.Speed / Mathf.Max(0.01f, plan.Speed + targetSpeed));

			double accuracy = source.GetStat(TowerDefence.Stats.StatType.Accuracy);
			double dodge = target.GetStat(TowerDefence.Stats.StatType.DodgeChance);
			float statFactor = Mathf.Clamp01((float)((accuracy - dodge) / 100.0));

			float hitChance = speedFactor * statFactor;
			return UnityEngine.Random.value < hitChance;
		}
	}
}
