// what kind of events does it trigger?

// projectile on reach -> projectile itself <- hooked by Action
// entity on get hit activates
// source on hit activates

// use entity's own functions so that they trigger themselves
// entity subocomponents trigger entity events (registerCallbacks())
// OnBuffExpire is internal as well - an external timerManager loops through their buffs and calls the Entity.BuffTimePassed(float time) method.
// Entity's functions are [RAW]! They do not adjust the values they receive. Therefore, biz logic is above it

using TowerDefence.Projectile;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	public class ProjectileAction : Action
	{
		public ProjectilePlan Plan { get; private set; }
	}
}