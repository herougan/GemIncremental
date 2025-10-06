using System;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using UnityEngine.UIElements;

namespace Util.Events
{
	public static class DelegateAdapter
	{
		// // Wraps an Action<T1>
		// public static WrappedAction Wrap<T1>(Action<T1> action, Action<T1> trigger, ISkill skill)
		// {
		// 	return new WrappedAction(trigger, args => action((T1)args[0]), skill);
		// }

		// // Wraps an Action<T1, T2>
		// public static WrappedAction Wrap<T1, T2>(Action<T1, T2> action, Action<T1, T2> trigger, ISkill skill)
		// {
		// 	return new WrappedAction(trigger, args => action((T1)args[0], (T2)args[1]), skill);
		// }

		// // Wraps an Action<T1, T2, T3>
		// public static WrappedAction Wrap<T1, T2, T3>(Action<T1, T2, T3> action, Action<T1, T2, T3> trigger, ISkill skill)
		// {
		// 	return new WrappedAction(trigger, args => action((T1)args[0], (T2)args[1], (T3)args[2]), skill);
		// }

		// // Wraps an Action<T1, T2, T3, T4>
		// public static WrappedAction Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, Action<T1, T2, T3, T4> trigger, ISkill skill)
		// {
		// 	return new WrappedAction(trigger, args => action((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), skill);
		// }
	}
}