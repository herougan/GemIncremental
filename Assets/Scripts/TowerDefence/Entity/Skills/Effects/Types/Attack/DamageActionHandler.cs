using TowerDefence.Context;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Stats;
using Util.Debug;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	/// <summary>
	/// Handles ActionType.Damage: deals action.Value as flat Physical damage to trigger.Target, falling
	/// back to the acting Entity itself if no Target is set - same fallback StatActionHandler uses for
	/// self-buffs/periodic ticks. Routes through the real Entity.ApplyDamage (DamageCalculator's
	/// modifier tree, then Nullifier/Shield/Health absorb order), not a direct StatBlock.Health.Deplete
	/// - this is the first real caller of that pipeline from inside the Skill/Effect system itself.
	/// </summary>
	public class DamageActionHandler : ActionHandler
	{
		public DamageActionHandler()
		{
			Type = ActionType.Damage;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (action is not Action damageAction)
			{
				LogManager.Instance.LogWarning($"DamageActionHandler received a non-Action action ({action.ActionType}).");
				return;
			}

			IEntity target = trigger?.Target ?? Entity;
			Damage damage = new Damage(StatType.Health, damageAction.Value, Entity);
			damage.SetTarget(target);
			damage.SetElement(damageAction.Element);
			damage.SetSource(trigger?.Source);
			foreach (Tag tag in damageAction.Tags) damage.AddTag(tag);
			target.ApplyDamage(damage);
		}
	}
}
