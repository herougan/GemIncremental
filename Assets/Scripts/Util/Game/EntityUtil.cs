using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using TowerDefence.Stats;
using Util.Maths;

namespace Util.Game
{
	public static class EntityUtil
	{
		public static bool Check(IEntity entity, IEnumerable<ICondition> conditions)
		{
			foreach (ICondition condition in conditions)
			{
				if (!Check(entity, condition))
					return false;
			}
			return true;
		}

		public static bool Check(IEntity Entity, ICondition condition)
		{
			switch (condition.ConditionType)
			{
				case ConditionType.Stat:
					// Load Condition
					var statCondition = condition as StatCondition;
					if (statCondition == null)
					{
						LogManager.Instance.LogWarning($"StatCondition {condition} is null or failed to cast in EntityUtil.Check");
						return false;
					}

					// Fetch value
					var stat = Entity.StatBlock.GetStat(statCondition.StatType);
					var value = stat.Value;
					if (statCondition.CheckCurrent && stat is IDepletable)
					{
						value = (stat as IDepletable).Current;
					}

					// Compare
					return MathsLib.Compare(value, statCondition.Value, statCondition.Comparative);

				case ConditionType.Kinematics:
					// Load Condition
					var kinCondition = condition as KinematicsCondition;
					if (kinCondition == null) return false;

					// Fetch value
					var kinValue = Entity.Kinematics.Get(kinCondition.Param);

					// Compare
					return MathsLib.Compare(kinValue, kinCondition.Value, kinCondition.Comparative);

				// Implement other condition types as needed
				default:
					return false;
			}
		}
	}
}