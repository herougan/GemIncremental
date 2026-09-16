using UnityEngine;

namespace TowerDefence.Projectile
{
	/// <summary>
	/// Plain data view of a live projectile - what ProjectileController.OnReach/OnPassthrough/OnExpire
	/// hand out, so a listener (see ProjectileActionHandler) doesn't need the whole MonoBehaviour.
	/// </summary>
	public interface IProjectile
	{
		GameObject ProjectileObject { get; set; }
		GameObject ProjectileTrail { get; set; }
		float Speed { get; set; }
	}

	public class Projectile : IProjectile
	{
		public GameObject ProjectileObject { get; set; }
		public GameObject ProjectileTrail { get; set; }
		public float Speed { get; set; }
	}
}
