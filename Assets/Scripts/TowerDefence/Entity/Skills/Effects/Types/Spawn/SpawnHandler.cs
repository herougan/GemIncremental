
using Util.Debug;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills.Effects;

namespace TowerDefence.Game.ActionHandler.Stat
{
	public class SpawnHandler : IActionHandler
	{
		public ActionType Type => ActionType.Spawn;

		public void ApplyAction(GameContext context, TriggerContext trigger, Action effect)
		{
			// // Default implementation for applying effects
			// IEntity entity = trigger.Targets;
			// entity.GetStat(effect.StatType).OperateValue(effect.Value, effect.Operation);

			// if (effect is SpawnAction spawnAction)
			// {
			// 	// Spawn the entity at the specified position
			// 	IEntity spawnedEntity = context.SpawnEntity(spawnAction.EntityType, spawnAction.Position, spawnAction.Rotation);

			// 	// Optionally, apply additional effects or initialization to the spawned entity
			// 	if (spawnAction.InitialStats != null)
			// 	{
			// 		foreach (var stat in spawnAction.InitialStats)
			// 		{
			// 			spawnedEntity.GetStat(stat.Key).SetValue(stat.Value);
			// 		}
			// 	}

			// 	if (trigger is DieTriggerContext dieTriggerContext) { }
			// }
			// else
			// {
			// 	LogManager.Instance.LogError($"SpawnHandler: Unsupported action type {effect.GetType()} for ApplyAction.");
			// }
		}

		public void Rollback(GameContext context, Action effect)
		{
			// Default implementation for rolling back effects

			// Effectively rollsback effects caused by this action
		}

		public void RemoveEffect(GameContext context, Action effect)
		{
			// Default implementation for removing effects

			// Removes attached effect but those not recalculate stats
		}

		public void ApplyAction(in GameContext context, in TriggerContext trigger, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void Rollback(in GameContext context, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveEffect(in GameContext context, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, Action effect)
		{
			throw new System.NotImplementedException();
		}

		public void RegisterCallbacks(IEntity entity, IEntityController controller)
		{

		}
	}
}