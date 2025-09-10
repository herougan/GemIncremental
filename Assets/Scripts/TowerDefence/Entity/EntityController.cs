using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;

namespace TowerDefence.Entity
{

	public interface IEntityController
	{
		// ===== Game State =====
		void ApplyBuff(IEntity source, IBuff buff);

		// ===== Event =====
		void RegisterCallbacks();
		void Died();
		void Buffed(IBuff buff);
		void Debuffed(IBuff duff);
		void Hit(IEntity tower);
		void Reached();
		void HealthDecreased();
		void StatChanged(IStat stat, double value);
	}

}