using UnityEngine;

namespace TowerDefence.Entity
{
	public interface IEntityController
	{
		Entity Entity { get; }
		// void RegisterCallbacks(IEntity entity /*, EntityController controller */);
	}

	/// <summary>
	/// MonoBehaviour, not a plain class - MonsterController/TowerController are meant to be Components
	/// on a spawned GameObject (elsewhere in the codebase, commented-out spawn code does
	/// `monsterObject.GetComponent&lt;MonsterController&gt;()`), which only works if this is one. Also
	/// registers/unregisters this Entity's Transform with EntityManager on Initiate/OnDestroy - the one
	/// place a plain-C# Entity gets linked to a real world position, for anything (e.g.
	/// ProjectileActionHandler's homing target) that needs to ask "where is this Entity right now."
	/// </summary>
	public class EntityController : MonoBehaviour, IEntityController
	{
		public Entity Entity { get; private set; }

		public void Initiate(Entity entity)
		{
			Entity = entity;
			entity.RegisterCallbacks();
			EntityManager.RegisterTransform(entity, transform);
		}

		void OnDestroy()
		{
			if (Entity != null) EntityManager.UnregisterTransform(Entity);
		}
	}
}
