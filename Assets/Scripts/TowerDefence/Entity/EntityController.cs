namespace TowerDefence.Entity
{
	public interface IEntityController
	{
		void RegisterCallbacks(IEntity entity /*, EntityController controller */);
	}

	public class EntityController : IEntityController
	{

	}
}