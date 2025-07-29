using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TowerDefence.Skills
{
	public interface IAction
	{

	}

	public class Action : IAction
	{
		// Implementation of trigger logic
	}

	public enum ActionType
	{
		// Property change
		Stat,
		Token,
		Damage, // % and flat
		Status,
		// Physics
		Move,
		Teleport,
		TimeSpeed,
		Kinematics,
		// Spawn
		Projectile,
		TimedEffect,
		Spawn,
		// Chain
		ChainEffect,
		ApplyBuff,
	}


	[CustomPropertyDrawer(typeof(Action))]
	public class EffectDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			// Create property container element.
			var container = new VisualElement();

			// SkillTrigger trigger = (SkillTrigger)target; // TODO
			// switch (trigger.Type)
			// {
			// 	case TriggerType.None:
			// 		EditorGUILayout.LabelField("No trigger selected.");
			// 		break;
			// 	case TriggerType.Permanent:
			// 		EditorGUILayout.LabelField("Permanent trigger.");
			// 		break;
			// 	case TriggerType.OnCast:
			// 		EditorGUILayout.LabelField("Trigger on cast.");
			// 		break;
			// 	// Add other cases as needed
			// 	default:
			// 		EditorGUILayout.LabelField("Unknown trigger type.");
			// 		break;
			// }

			// Create property fields.
			var typeField = new PropertyField(property.FindPropertyRelative("Type"));
			var valueField = new PropertyField(property.FindPropertyRelative("Value"));
			// new PropertyField(,)

			// Add fields to the container.
			container.Add(typeField);
			container.Add(valueField);

			return container;
		}
	}

}