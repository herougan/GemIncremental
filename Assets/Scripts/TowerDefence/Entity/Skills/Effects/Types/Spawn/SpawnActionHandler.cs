using TowerDefence.Context;
using TowerDefence.Entity.Monster;
using TowerDefence.Stats;
using UnityEngine;
using Util.Debug;
// A separate alias for the Monster class itself, same reason as EntityManager.cs's own MonsterEntity
// alias: this file's namespace (TowerDefence.Entity.Skills.Effects.Types.Spawn) has TowerDefence.Entity
// as an enclosing namespace, which also has a *nested* namespace called Monster - enclosing-namespace
// lookup finds that nested namespace before any using-directive gets considered, so a bare `Monster`
// here always resolves to the namespace (CS0118), never the class.
using MonsterEntity = TowerDefence.Entity.Monster.Monster;

namespace TowerDefence.Entity.Skills.Effects.Types.Spawn
{
	/// <summary>
	/// Handles ActionType.Spawn: builds Quantity fresh Monsters from either action.Monster (a fixed Plan)
	/// or, if SpawnSelf, a copy of whichever Entity's own Plan just triggered this (EntityManager.
	/// SpawnMonster - the plain-C# Entity side), then, if that Plan has a Prefab assigned, instantiates it
	/// and wires it up with MonsterController.Init - same "plain Entity first, GameObject wrapper second"
	/// two-step every other spawn path in the codebase uses (see MvpDemoSpawner, EntityWaveManager).
	///
	/// SpawnSelf spawns are stamped with CounterType.SplitDepth = the dying Entity's own SplitDepth + 1,
	/// and scaled down by ScalePerDepth (compounding across generations, since each spawned copy's stats
	/// are already the previous generation's scaled numbers) - a SkillPlan gates further splitting with a
	/// CounterCondition(SplitDepth, maxDepth, LessThan) in its own Effect.Conditions (checked by
	/// EffectController before Actions run at all), not this handler - the handler only ever does what
	/// it's told, it doesn't decide when splitting should stop.
	///
	/// Position resolution mirrors ProjectileActionHandler: prefers the *live* Transform of whatever
	/// triggered this (EntityManager's Position Registry - e.g. the dying Entity, for an OnDeath spawn),
	/// falling back to the TriggerContext's snapshot position if nothing's registered.
	/// </summary>
	public class SpawnActionHandler : ActionHandler
	{
		public SpawnActionHandler()
		{
			Type = ActionType.Spawn;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (action is not SpawnAction spawnAction)
			{
				LogManager.Instance.LogWarning("SpawnActionHandler received a non-Spawn action.");
				return;
			}

			MonsterPlan plan = spawnAction.SpawnSelf ? Entity.Plan as MonsterPlan : spawnAction.Monster;
			if (plan == null)
			{
				LogManager.Instance.LogWarning(spawnAction.SpawnSelf
					? $"SpawnActionHandler: SpawnSelf on {Entity} (Plan is not a MonsterPlan - only Monsters can split)."
					: "SpawnActionHandler received an action with no MonsterPlan.");
				return;
			}

			int depth = Entity.GetCounter(CounterType.SplitDepth) + 1;
			Vector3 position = EntityManager.GetTransform(Entity)?.position ?? trigger.SourcePosition;

			for (int i = 0; i < spawnAction.Quantity; i++)
			{
				MonsterEntity spawned = EntityManager.SpawnMonster(plan);
				spawned.SetCounter(CounterType.SplitDepth, depth);

				if ((double)spawnAction.ScalePerDepth != 1)
				{
					EntityManager.ScaleStat(spawned, StatType.Health, spawnAction.ScalePerDepth);
					EntityManager.ScaleStat(spawned, StatType.Attack, spawnAction.ScalePerDepth);
					EntityManager.ScaleStat(spawned, StatType.Defence, spawnAction.ScalePerDepth);
				}

				if (plan.Prefab == null)
				{
					LogManager.Instance.LogWarning($"MonsterPlan {plan.name} has no Prefab - spawned as data only, no GameObject/visual.");
					continue;
				}

				GameObject instance = Object.Instantiate(plan.Prefab, position, Quaternion.identity);
				MonsterController controller = instance.GetComponent<MonsterController>();
				if (controller == null)
				{
					LogManager.Instance.LogWarning($"{plan.Prefab.name} has no MonsterController - destroying.");
					Object.Destroy(instance);
					continue;
				}
				controller.Init(spawned);
				// No Path assigned (see MonsterController.SetPath) - a spawned-in monster doesn't
				// automatically inherit whatever route its spawner was following. STUB: revisit once
				// there's a real "should on-death spawns keep walking the same path" design answer.
			}
		}
	}
}
