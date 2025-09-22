using Debug;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Projectile;
using UnityEngine;

namespace TowerDefence.Entity
{
	public class EntityManager : MonoBehaviour
	{
		public static EntityManager Instance { get; private set; }
		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			LogManager.Instance.Log($"EntityManager started. {System.DateTime.Now}");
		}

		#region Methods

		public static void SpawnMonster()
		{

		}

		public static void SpawnTower()
		{
		}

		/// <summary>
		/// Note, projectiles are not entities
		/// </summary>
		public static void SpawnProjectile(ProjectilePlan plan, IEntity source, IEntity target, IActionData actionData)
		{


			// Wire



		}

		#endregion Methods
	}
}