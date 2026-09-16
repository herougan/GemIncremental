using UnityEngine;
using Util.Maths;

namespace TowerDefence.Projectile
{
	/// <summary>
	/// Design-time data for a projectile, authored as an asset like MonsterPlan/SkillPlan/TowerPlan -
	/// referenced from a ProjectileAction (see ProjectileAction.cs) to say what a given Skill's shot
	/// looks/behaves like. ProjectileController is the runtime side that actually reads this.
	///
	/// Physics fields (Speed/Size/TimeToDie) are float - they drive real Vector3/Transform math on
	/// ProjectileController. Damage is ddouble, like every other game-balance number in the codebase -
	/// see MathsLib.ddouble. Don't mix the two: a projectile's flight is physics, what it does on
	/// arrival is game logic.
	/// </summary>
	[CreateAssetMenu(fileName = "ProjectilePlan", menuName = "TowerDefence/Projectile/ProjectilePlan")]
	public class ProjectilePlan : ScriptableObject
	{
		[Header("Physics")]
		public float Speed;
		public float Size; // arrival/hit-radius threshold, not a visual scale
		public float TimeToDie; // destroys itself after this many seconds regardless of whether it hit anything - no Range field; max travel distance is just Speed * TimeToDie if you need it for display
		// If true, ProjectileController continuously steers toward the target's live position every
		// frame (via EntityManager's Position Registry) and always connects (barring the target dying
		// first - no "miss" concept for a homing shot). If false, it just flies straight at whatever
		// angle it launched at - see ProjectileActionHandler for how that angle gets picked, and
		// ProjectileController.ComputeWillHit for how a straight shot decides whether it'll actually
		// connect before it ever moves.
		public bool Homing;

		[Header("Damage")]
		public ddouble Damage;

		[Header("VFX")]
		public GameObject ReleaseEffect; // spawned at the source's position when the projectile launches
		public GameObject TrailEffect;   // spawned attached to the projectile, follows it in flight
		public GameObject HitEffect;     // spawned at the point of impact (or the miss point, if it misses)

		[Header("Prefab")]
		// Must have a ProjectileController component - ProjectileActionHandler instantiates this and
		// calls Launch on it.
		public GameObject ProjectilePrefab;
	}
}
