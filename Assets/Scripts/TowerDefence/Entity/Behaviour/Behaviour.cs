namespace TowerDefence.Entity.Behaviour
{
	public class Behaviour
	{
		public IEntity Target;
		public List<IEntity> WithinRange;
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