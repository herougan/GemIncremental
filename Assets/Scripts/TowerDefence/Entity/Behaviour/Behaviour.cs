using System.Collections.Generic;

namespace TowerDefence.Entity.Behaviour
{
	public interface IBehaviour
	{
		EntityBehaviourType Type { get; }
		bool Evaluate(IEntity entity);
	}

	public class Behaviour : IBehaviour
	{
		public IEntity Target { get; set; }
		public List<IEntity> WithinRange { get; set; }

		public EntityBehaviourType Type { get; set; }

		public bool Evaluate(IEntity entity)
		{
			// Implement evaluation logic here
			return false;
		}
	}

	public enum EntityBehaviourType
	{
		HasBuff,
		HasDebuff,
		Attacking,
		Channeling,
		Fight,
	}
}