using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;

namespace TowerDefence.Context
{
	public interface ITriggerContext : IContext
	{
		TriggerType TriggerType { get; }
		List<IEntity> Targets { get; }
	}

	public interface IPeriodicTriggerContext : ITriggerContext
	{
		float Period { get; }
		// TriggerType TriggerType { get; } = TriggerType.Periodic;
	}

	public interface IEnterRangeTriggerContext : ITriggerContext
	{
		IEntity Target { get; }
		// TriggerType TriggerType { get; } = TriggerType.EnterRange;
	}

	public interface IAttackTriggerContext : ITriggerContext
	{
		IEntity Target { get; }
		// TriggerType TriggerType { get; } = TriggerType.Attack;
	}

	public interface IMultiAttackTriggerContext : ITriggerContext
	{
		List<IEntity> Target { get; }
		// TriggerType TriggerType { get; } = TriggerType.MultiAttack;
	}

	public class TriggerContext : ITriggerContext
	{
		public TriggerType TriggerType => throw new System.NotImplementedException();

		public List<IEntity> Targets => throw new System.NotImplementedException();
	}
}