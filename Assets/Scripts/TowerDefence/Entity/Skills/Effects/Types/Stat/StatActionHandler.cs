using TowerDefence.Context;
using TowerDefence.Stats;
using Util.Debug;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects.Types.Stat
{
	/// <summary>
	/// Handles ActionType.Stat: registers a permanent Add IStatMod (action.Value onto action.Stat) on
	/// the trigger's Target, falling back to the acting Entity itself if no Target is set (e.g. a
	/// self-buff) - via Entity.RegisterStatMod, same registry Skill.ApplyPassive/Foundry use, so it
	/// composes correctly with everything else touching the same stat instead of a separate direct
	/// StatBlock mutation.
	///
	/// Permanent, not owned by anything that expires it: an ActionType.Stat mod is "permanent until
	/// reset" by design - if you want a stat change that goes away on its own, that's what
	/// ActionType.ApplyBuff + a Passive containing the actual StatMod is for (the Buff's own
	/// expiry/cleanse deregisters it - see Buff's Duration timer). This handler never deregisters what
	/// it registers.
	/// </summary>
	public class StatActionHandler : ActionHandler
	{
		public StatActionHandler()
		{
			Type = ActionType.Stat;
		}

		public override void ApplyAction(in GameContext context, in TriggerContext trigger, IEntity Entity, IAction action)
		{
			if (action is not Action statAction)
			{
				LogManager.Instance.LogWarning($"StatActionHandler received a non-Action action ({action.ActionType}).");
				return;
			}
			IEntity target = trigger?.Target ?? Entity;
			IStatMod mod = new StatMod(statAction.Value, statAction.Stat, MathOperation.Add, trigger?.Source);
			target.RegisterStatMod(mod);
		}
	}
}
