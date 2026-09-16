using System;
using TowerDefence.Projectile;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	// [Serializable] is required on the CONCRETE type, not just inherited from Action - Unity's
	// [SerializeReference] (see Effect.Actions) checks obj.GetType(), not the base class, and silently
	// fails to round-trip a subclass that's missing it: after a domain reload, a saved ProjectileAction
	// deserializes back as a plain base Action (Type survives - it's on Action itself - but Plan, which
	// only exists here, is gone), which is exactly what made ProjectileActionHandler's `action is not
	// ProjectileAction` guard start firing on content that was fine right after Generate Example Content
	// but broke the next time the Editor reloaded.
	[Serializable]
	public class ProjectileAction : Action
	{
		public ProjectilePlan Plan { get; set; }

		public ProjectileAction()
		{
			Type = ActionType.Projectile;
		}
	}
}
