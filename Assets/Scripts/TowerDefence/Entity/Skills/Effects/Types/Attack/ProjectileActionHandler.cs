using TowerDefence.Context;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Projectile;
using TowerDefence.Stats;
using Util.Debug;
using UnityEngine;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	/// <summary>
	/// Handles ActionType.Projectile: instantiates action.Plan.ProjectilePrefab, launches it at
	/// trigger.Target, and wires its OnReach so that whatever the projectile actually hits gets the
	/// real ApplyDamage -> Attack/GotHit sequence - not this handler's caller, the projectile's own
	/// arrival.
	///
	/// Position resolution prefers the *live* Transform (EntityManager's Position Registry) over the
	/// raw TriggerContext.SourcePosition/TargetPosition snapshot, falling back to the snapshot only if
	/// nothing's registered - "sometimes I want the raw positions, sometimes the dynamic
	/// source.position/target.position" per the design discussion. ProjectileController itself no
	/// longer needs a TargetPosition at all once launched: homing re-queries the live Transform every
	/// frame, a straight shot just flies at the angle computed here once.
	/// </summary>
	public class ProjectileActionHandler : ActionHandler
	{
		public ProjectileActionHandler()
		{
			Type = ActionType.Projectile;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (action is not ProjectileAction projectileAction || projectileAction.Plan == null)
			{
				LogManager.Instance.LogWarning("ProjectileActionHandler received an action with no ProjectilePlan.");
				return;
			}

			IEntity target = trigger?.Target;
			if (target == null)
			{
				LogManager.Instance.LogWarning("ProjectileActionHandler has no Target to fire at.");
				return;
			}

			ProjectilePlan plan = projectileAction.Plan;
			if (plan.ProjectilePrefab == null)
			{
				LogManager.Instance.LogWarning($"ProjectilePlan {plan.name} has no ProjectilePrefab.");
				return;
			}

			Vector3 sourcePosition = EntityManager.GetTransform(Entity)?.position ?? trigger.SourcePosition;
			Vector3 targetPosition = EntityManager.GetTransform(target)?.position ?? trigger.TargetPosition;
			float initialAngle = Vector3.SignedAngle(Vector3.forward, targetPosition - sourcePosition, Vector3.up);

			GameObject instance = Object.Instantiate(plan.ProjectilePrefab, sourcePosition, Quaternion.identity);
			ProjectileController controller = instance.GetComponent<ProjectileController>();
			if (controller == null)
			{
				LogManager.Instance.LogWarning($"{plan.ProjectilePrefab.name} has no ProjectileController - destroying.");
				Object.Destroy(instance);
				return;
			}

			IEntity source = Entity;
			// Copied to a local before the closure below - `trigger` is an `in` parameter, which C#
			// won't let a lambda capture directly (same restriction as ref/out).
			ISource attackSource = trigger?.Source;
			controller.OnReach += (proj, hitEntity) =>
			{
				Damage damage = new Damage(StatType.Health, plan.Damage, source);
				damage.SetTarget(hitEntity);
				damage.SetSource(attackSource);
				damage.SetElement(projectileAction.Element);
				damage.AddTag(Tag.Ranged); // every projectile is Ranged - see Tag.cs
				foreach (Tag tag in projectileAction.Tags) damage.AddTag(tag);
				hitEntity.ApplyDamage(damage);
				source.Attack(hitEntity, damage);
				hitEntity.GotHit(source, damage);
			};

			controller.Launch(plan, source, target, sourcePosition, initialAngle);
		}
	}
}
