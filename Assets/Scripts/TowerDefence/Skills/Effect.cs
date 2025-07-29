using System;
using System.Collections.Generic;

namespace TowerDefence.Skills
{
	public interface IEffect
	{
		// When
		public List<ITrigger> Triggers { get; }
		// If
		public List<ICondition> Conditions { get; }
		public List<ICondition> TargetConditions { get; }
		// Do
		public List<IAction> Actions { get; }
		// And
		public List<IBoost> Boosts { get; }
	}

	/// <summary>
	/// Effect is a data object containing triggers, conditions, and actions.
	/// EffectHandler manages the logic of subscribing actions based on trigger(s). Upon any trigger,
	/// it checks all condition(s), and if they are met, executes the action(s).
	/// </summary>
	[Serializable]
	public class Effect : IEffect
	{
		public List<ITrigger> Triggers { get; private set; }
		public List<ICondition> Conditions { get; private set; }
		public List<IAction> Actions { get; private set; }

		public override string ToString()
		{
			return base.ToString();
		}
	}

	public class EffectBundle
	{
		public List<Effect> Effects { get; set; } = new List<Effect>();

		public EffectBundle() { }

		public EffectBundle(List<Effect> effects)
		{
			Effects = effects;
		}

		public void AddEffect(Effect effect)
		{
			if (effect != null)
			{
				Effects.Add(effect);
			}
		}

		public void GetTriggers(int index)
		{
			if (index < 0 || index >= Effects.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
			}

			var effect = Effects[index];
			if (effect != null)
			{
				foreach (var trigger in effect.Triggers)
				{
					// Process each trigger
				}
			}
			else
			{
				throw new NullReferenceException("Effect at the specified index is null.");
			}
		}
	}

	// [CustomPropertyDrawer(typeof(SkillEffect))]
	// public class EffectDrawer : PropertyDrawer
	// {
	// 	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	// 	{
	// 		// Create property container element.
	// 		var container = new VisualElement();

	// 		// SkillTrigger trigger = (SkillTrigger)target; // TODO
	// 		// switch (trigger.Type)
	// 		// {
	// 		// 	case TriggerType.None:
	// 		// 		EditorGUILayout.LabelField("No trigger selected.");
	// 		// 		break;
	// 		// 	case TriggerType.Permanent:
	// 		// 		EditorGUILayout.LabelField("Permanent trigger.");
	// 		// 		break;
	// 		// 	case TriggerType.OnCast:
	// 		// 		EditorGUILayout.LabelField("Trigger on cast.");
	// 		// 		break;
	// 		// 	// Add other cases as needed
	// 		// 	default:
	// 		// 		EditorGUILayout.LabelField("Unknown trigger type.");
	// 		// 		break;
	// 		// }

	// 		// Create property fields.
	// 		var typeField = new PropertyField(property.FindPropertyRelative("Type"));
	// 		var valueField = new PropertyField(property.FindPropertyRelative("Value"));
	// 		// new PropertyField(,)

	// 		// Add fields to the container.
	// 		container.Add(typeField);
	// 		container.Add(valueField);

	// 		return container;
	// 	}
	// }

}