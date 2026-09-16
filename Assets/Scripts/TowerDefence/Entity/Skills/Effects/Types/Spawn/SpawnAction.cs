using System;
using TowerDefence.Entity.Monster;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects.Types.Spawn
{
	/// <summary>
	/// ActionType.Spawn's pigeon-hole data - "spawn Quantity of Monster" (e.g. a Skeleton King's
	/// on-death Skill: Monster = SkeletonPlan, Quantity = 2, Trigger = OnDeath). One mechanism, reused by
	/// however many SkillPlans want a spawn-on-X effect, each just carrying different data here - same
	/// relationship ProjectilePlan has to ProjectileAction (Firebolt vs IceShard, one Projectile
	/// mechanism): no new ActionType/handler is needed per spawning skill, only per genuinely new spawn
	/// *mechanism* (e.g. "spawn in a ring around the target" later would still be this same ActionType,
	/// just another field here).
	///
	/// SpawnSelf covers the case a fixed Monster reference can't: "splits into mini versions of itself,"
	/// where the thing to spawn is whichever Entity died, not one specific hardcoded MonsterPlan - so the
	/// *same* SkillPlan asset (e.g. "SplitOnDeath") can be dropped into any monster's InitSkills
	/// unchanged, Slime and Slug alike, rather than needing one SkillPlan per species. When true, Monster
	/// is ignored and SpawnActionHandler spawns a copy of the dying Entity's own Plan instead - see
	/// CounterType.SplitDepth for how it's kept from splitting forever.
	/// </summary>
	// [Serializable] required on the concrete type - see ProjectileAction's own doc comment for why
	// (the same [SerializeReference]-on-a-subclass gotcha applies here identically).
	[Serializable]
	public class SpawnAction : Action
	{
		public MonsterPlan Monster;
		public bool SpawnSelf;
		public int Quantity = 1;

		/// <summary>Permanent multiplier (Health/Attack/Defence) applied to each spawned copy - 1 means no scaling. Only meaningful with SpawnSelf (a "mini version" splitting weaker each generation); a fixed Monster reference is already however strong its own Plan says.</summary>
		public ddouble ScalePerDepth = 1;

		public SpawnAction()
		{
			Type = ActionType.Spawn;
		}
	}
}
