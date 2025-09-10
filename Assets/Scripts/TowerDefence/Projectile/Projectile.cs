using UnityEngine;

namespace TowerDefence.Projectile
{
	public interface IProjectile
	{
		GameObject ProjectileObject { get; set; }
		GameObject ProjectileTrail { get; set; }
		float Speed { get; set; }
	}

	public class Projectile : IProjectile
	{
		public GameObject ProjectileObject { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public GameObject ProjectileTrail { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public float Speed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
	}
}