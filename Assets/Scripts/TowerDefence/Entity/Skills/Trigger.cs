namespace TowerDefence.Entity.Skills
{
	public interface ITrigger
	{
		public TriggerType Type { get; }
		public float Parameter { get; }
	}

	/// <summary>
	/// Trigger is just a data object, as part of an Effect. ActionHandler manages the logic of subscribing actions.
	/// </summary>
	[System.Serializable]
	public class Trigger : ITrigger
	{
		// Implementation of trigger logic
		public TriggerType Type { get; private set; }
		public float Parameter { get; private set; }

		public Trigger(TriggerType type, float parameter = 0)
		{
			Type = type;
			Parameter = parameter;
		}

		public static bool IsGameTrigger(TriggerType type)
		{
			return false;
		}
	}

	public enum TriggerType
	{
		OnReached,
		OnSpawn,
		OnPushback,
		OnHidden,
		OnWaypoint,
		//
		OnFirstHurt,
		OnFirstBuff,
		OnTargetted,
		OnUntargetted,
		OnCast,
		//
		OnAllySpawn,
		OnAllyDeath,
		OnAllyHurt,
		OnAllyHeal,
		OnAllyBuff,
		OnAllyStatus,
		OnAllySpendResources,
		//
		OnSkillLearned,
		OnSkillUnlearned,
		OnSkillActivate, // A skill's effect actually fired (incl. periodic ticks) - see WrappedAction
		//
		OnEnteredRange,
		OnExitRange,
		OnEnteredAttackRange,
		OnExitAttackRange,
		OnIsolated,
		OnNotIsolated,
		//
		OnAttack,
		OnHit,
		OnAttacked,
		OnDOT,
		OnDeath,
		OnKill,
		OnHurt,
		OnHeal,
		OnVamp,
		OnBloodied,
		OnDying,
		OnRage,
		OnRange,
		OnManaDry,
		OnRest,
		OnEntityStateChange,
		//
		OnPeriodic,
		//
		OnResourceIncreased,
		OnResourceDecreased,
		OnResourceSpent,
		//
		OnValueChanged,
		OnValueDecreased,
		OnValueIncreased,
		OnMaxValueChanged,
		OnCurrentValueDecreased,
		OnCurrentValueIncreased,
		OnStatBonusAdded,
		OnStatNerfAdded,
		OnRegenerate,
		OnRegenValueChanged,
		OnRegenRateChanged,
		//
		OnStatusResistChanged,
		OnStatusMasteryChanged,
		OnMasterChanged,
		// 
		OnThresholdChanged,
		OnThresholdCrossed,
		//
		OnTokenChanged,
		OnTokenTransmute,
		OnTokenExchange,
		OnNewToken,
		//
		OnEleResistChanged,
		OnEleMasteryChanged,
		//
		OnBuffApplied,
		OnDebuffApplied,
		OnBuffRemoved,
		OnDebuffCleansed,
		OnBuffExpired,
		OnDebuffExpired,
		OnBuffStacked,
	}

}