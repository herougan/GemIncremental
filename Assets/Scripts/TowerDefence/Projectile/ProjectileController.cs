using System;
using TowerDefence.Entity;
using UnityEngine;

namespace TowerDefence.Projectile
{
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


	}
}