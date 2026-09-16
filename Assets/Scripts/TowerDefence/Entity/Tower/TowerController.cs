using System.Linq;
using TowerDefence.Context;
using TowerDefence.Entity.Skills.ActionHandler;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Effects.Types.Attack;
using TowerDefence.Stats;
using UnityEngine;
using Util.Maths;

namespace TowerDefence.Entity.Tower
{
	/// <summary>
	/// The Unity-side wrapper for a Tower (see EntityController) - AND its targeting/attack timing loop.
	/// Previously split into TowerController (wiring/lifecycle) + a separate TowerAttackController
	/// component; merged into one, since nothing ever reached into TowerAttackController independently
	/// (`[RequireComponent]` already tied them 1:1, always-together, forever) and MonsterController
	/// already sets the precedent of a Controller owning its own core per-frame behaviour directly
	/// (movement, in that case) rather than splitting it into a second component.
	///
	/// The attack timer starts the moment Init runs (i.e. "the moment the Tower is spawned") and runs
	/// continuously, with one deliberate exception: if it rings with no valid target in range, it doesn't
	/// reset and keep counting down to the next scheduled tick - it pauses right there, so the instant a
	/// target becomes available the Tower fires immediately rather than waiting out a full idle period.
	/// This is genuinely different from the generic OnPeriodic/WrappedAction pipeline (which always ticks
	/// on schedule regardless of targets), so this owns its own CountdownTimer rather than reusing that one.
	///
	/// Firing calls EffectController.ApplyAction directly against the Tower's own first ProjectileAction
	/// (found by walking its Skills), rather than going through a Trigger - there's no TriggerType that
	/// means "the attack timer just rang and picked a target," and OnAttack is reserved for when a shot
	/// actually *lands* (see ProjectileActionHandler's OnReach, which raises OnAttack/OnHit itself).
	///
	/// Targeting itself is O(every registered Entity) per Tower per frame (see
	/// EntityManager.GetEntitiesInRange) - fine at small scale, a real spatial-partitioning pass is a
	/// follow-up once there are enough simultaneous Towers/Monsters for it to matter.
	/// </summary>
	public class TowerController : EntityController
	{
		public Tower tower { get; private set; }

		/// <summary>If Init(Tower) is never called explicitly, Start builds one from this instead - see MonsterController.DefaultPlan for the same reasoning.</summary>
		[Header("Default (used only if Init isn't called first)")]
		public TowerPlan DefaultPlan;

		CountdownTimer attackTimer;
		bool paused;

		/// <summary>Sets the typed tower reference, runs the base EntityController wiring (Position Registry), and starts the attack timer - the three things a real Tower GameObject needs to actually target/fire, none of which anything wired up before either of these classes existed.</summary>
		public void Init(Tower tower)
		{
			this.tower = tower;
			Initiate(tower);

			float attackSpeed = (float)(double)tower.GetStat(StatType.AttackSpeed);
			float interval = 1f / Mathf.Max(0.01f, attackSpeed);

			attackTimer = new CountdownTimer(interval, true);
			attackTimer.OnRing += OnRing;
			attackTimer.Start();
		}

		void Start()
		{
			if (tower == null && DefaultPlan != null)
			{
				Tower defaultTower = new Tower(DefaultPlan);
				defaultTower.Spawn();
				Init(defaultTower);
			}
		}

		void Update()
		{
			if (attackTimer == null) return; // Init hasn't run yet

			IEntity target = FindTarget();

			if (target == null)
			{
				if (!paused)
				{
					attackTimer.Pause();
					paused = true;
				}
				return;
			}

			if (paused)
			{
				attackTimer.Resume();
				paused = false;
			}

			attackTimer.Tick(Time.deltaTime);
		}

		void OnRing(CountdownTimer timer)
		{
			IEntity target = FindTarget();
			if (target == null) return; // shouldn't normally happen (Update pauses first) - defensive

			Fire(target);
		}

		IEntity FindTarget()
		{
			TowerPlan plan = (TowerPlan)tower.Plan;
			float range = (float)(double)tower.GetStat(StatType.Range);

			var candidates = EntityManager.GetEntitiesInRange(transform.position, range)
				.Where(e => e != (IEntity)tower)
				.Where(e => e is Monster.Monster)
				.Where(e => plan.TargetingTags.Count == 0 || plan.TargetingTags.Any(e.HasTag))
				.ToList();

			return Prioritise(candidates, plan.Targetting);
		}

		IEntity Prioritise(System.Collections.Generic.List<IEntity> candidates, Tower.Targetting targetting)
		{
			if (candidates.Count == 0) return null;

			switch (targetting)
			{
				case Tower.Targetting.health:
					return candidates.OrderBy(e => (double)e.GetStat(StatType.Health)).First();

				case Tower.Targetting.percentageHealth:
					return candidates.OrderBy(e => PercentHealth(e)).First();

				case Tower.Targetting.debuffed:
					return candidates.FirstOrDefault(e => e.Buffs.Any(b => !b.IsPositive)) ?? candidates.First();

				case Tower.Targetting.distance:
					// "Closest to the end [of its path]" per the design discussion - real now that
					// MonsterController tracks its own path progress (see RemainingNodes).
					return candidates.OrderBy(RemainingNodes).First();

				case Tower.Targetting.proximity:
				default:
					return candidates.OrderBy(e => Vector3.Distance(EntityManager.GetTransform(e).position, transform.position)).First();
			}
		}

		double PercentHealth(IEntity entity)
		{
			double max = (double)entity.GetStat(StatType.Health);
			if (max <= 0) return 0;
			return (double)entity.StatBlock.GetCurrent(StatType.Health) / max;
		}

		/// <summary>Nodes left on entity's own path (MonsterController.RemainingNodes) - int.MaxValue (sorts last) if it has no MonsterController/Transform/path, so a malformed candidate never wins "closest to the end" by default.</summary>
		int RemainingNodes(IEntity entity)
		{
			Transform entityTransform = EntityManager.GetTransform(entity);
			Monster.MonsterController controller = entityTransform != null ? entityTransform.GetComponent<Monster.MonsterController>() : null;
			return controller != null ? controller.RemainingNodes : int.MaxValue;
		}

		void Fire(IEntity target)
		{
			IAction attackAction = FindAttackAction();
			if (attackAction == null) return;

			Vector3 sourcePosition = transform.position;
			Vector3 targetPosition = EntityManager.GetTransform(target)?.position ?? sourcePosition;

			TriggerContext ctx = new TriggerContext
			{
				Entity = tower,
				Target = target,
				SourcePosition = sourcePosition,
				TargetPosition = targetPosition,
			};
			EffectController.ApplyAction(ctx, tower, attackAction);
		}

		/// <summary>First ProjectileAction found across every Skill this Tower knows - STUB: a Tower with multiple attack skills has no way to pick "the" basic attack yet, this just grabs whichever comes first.</summary>
		IAction FindAttackAction()
		{
			foreach (Skills.ISkill skill in tower.Skills)
			{
				foreach (IEffect effect in skill.Plan.Effects)
				{
					foreach (IAction action in effect.Actions)
					{
						if (action.ActionType == ActionType.Projectile) return action;
					}
				}
			}
			return null;
		}
	}
}
