namespace TowerDefence.Entity.Skills
{
	public interface ITrigger
	{
		public TriggerType Type { get; }
	}

	/// <summary>
	/// Trigger is just a data object, as part of an Effect. ActionHandler manages the logic of subscribing actions.
	/// </summary>
	public class Trigger : ITrigger
	{
		// Implementation of trigger logic
		public TriggerType Type { get; private set; }
		public Trigger(TriggerType type)
		{
			Type = type;
		}
	}

	// [CustomPropertyDrawer(typeof(Trigger))]
	// public class TriggerDrawer : PropertyDrawer
	// {
	// 	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	// 	{
	// 		EditorGUI.BeginProperty(position, label, property);

	// 		// Draw foldout
	// 		property.isExpanded = EditorGUI.Foldout(
	// 			new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
	// 			property.isExpanded, label, true);

	// 		if (property.isExpanded)
	// 		{
	// 			EditorGUI.indentLevel++;
	// 			float y = position.y + EditorGUIUtility.singleLineHeight + 2;

	// 			SerializedProperty typeProp = property.FindPropertyRelative("Type");
	// 			EditorGUI.PropertyField(
	// 				new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
	// 				typeProp);

	// 			y += EditorGUIUtility.singleLineHeight + 2;

	// 			// Draw other fields based on SkillTrigger fields
	// 			SerializedProperty paramProp = property.FindPropertyRelative("Parameter");
	// 			if (paramProp != null)
	// 			{
	// 				EditorGUI.PropertyField(
	// 					new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
	// 					paramProp);
	// 				y += EditorGUIUtility.singleLineHeight + 2;
	// 			}

	// 			// Add more fields as needed

	// 			EditorGUI.indentLevel--;
	// 		}

	// 		EditorGUI.EndProperty();
	// 	}

	// 	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	// 	{
	// 		float height = EditorGUIUtility.singleLineHeight;
	// 		if (property.isExpanded)
	// 		{
	// 			height += EditorGUIUtility.singleLineHeight + 2; // Type
	// 			SerializedProperty paramProp = property.FindPropertyRelative("Parameter");
	// 			if (paramProp != null)
	// 				height += EditorGUIUtility.singleLineHeight + 2;
	// 			// Add more fields as needed
	// 		}
	// 		return height;
	// 	}
	// }

	// [CustomPropertyDrawer(typeof(Trigger))]
	// public class TriggerDrawer2 : PropertyDrawer
	// {
	// 	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	// 	{
	// 		// Create property container element.
	// 		var container = new VisualElement();

	// 		// Create property fields.
	// 		var typeField = new PropertyField(property.FindPropertyRelative("Type"));
	// 		var valueField = new PropertyField(property.FindPropertyRelative("Value"));
	// 		// var triggerType = property.FindPropertyRelative("Type").GetValue(property)±±—;

	// 		var objectClassType = property.GetType();
	// 		var field = objectClassType.GetField(property.propertyPath);
	// 		if (field != null)
	// 		{
	// 			var value = field.GetValue(property);
	// 			LogManager.Instance.Log($"Testing TriggerDrawer {value}");
	// 		}

	// 		// 	        public static object GetTargetObjectOfProperty(SerializedProperty prop)
	// 		// {
	// 		//     if (prop == null) return null;

	// 		//     var path = prop.propertyPath.Replace(".Array.data[", "[");
	// 		//     object obj = prop.serializedObject.targetObject;
	// 		//     var elements = path.Split('.');
	// 		//     foreach (var element in elements)
	// 		//     {
	// 		//         if (element.Contains("["))
	// 		//         {
	// 		//             var elementName = element.Substring(0, element.IndexOf("["));
	// 		//             var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
	// 		//             obj = GetValue_Imp(obj, elementName, index);
	// 		//         }
	// 		//         else
	// 		//         {
	// 		//             obj = GetValue_Imp(obj, element);
	// 		//         }
	// 		//     }
	// 		//     return obj;
	// 		// }

	// 		// EditorGUILayout.LabelField("Trigger Type");
	// 		// EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), typeField, GUIContent.none);

	// 		// typeField.GetType().GetField("m_EnumNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
	// 		// 	.SetValue(typeField, new string[] { "None", "Permanent", "OnCast" }); // Add other enum names as needed

	// 		//
	// 		// switch (typeField.value.enumValueIndex)
	// 		// {
	// 		// 	case (int)TriggerType.None:
	// 		// 		EditorGUILayout.LabelField("No trigger selected.");
	// 		// 		break;
	// 		// 	case (int)TriggerType.Permanent:
	// 		// 		EditorGUILayout.LabelField("Permanent trigger.");
	// 		// 		break;
	// 		// 	case (int)TriggerType.OnCast:
	// 		// 		EditorGUILayout.LabelField("Trigger on cast.");
	// 		// 		break;
	// 		// 	// Add other cases as needed
	// 		// 	default:
	// 		// 		EditorGUILayout.LabelField("Unknown trigger type.");
	// 		// 		break;
	// 		// }

	// 		// Add fields to the container.
	// 		container.Add(typeField);
	// 		container.Add(valueField);

	// 		return container;
	// 	}
	// }

	public enum TriggerType
	{
		None,
		Permanent,
		OnCast,
		OnHit,
		OnKill,
		OnAttack,
		OnAttacked,
		OnPeriodic,
		OnSkilled,
		OnStatused,
		OnElement,
		OnSpawn,
		OnDeath,
		OnEnterRange,
		OnExitRange,
		OnStatusInflict,
		OnStatusInflicted,
		OnSkillUse,
		OnSkillUsed,
		//
		EnterRange,
		Attack,
		MultiAttack,
		Periodic,
	}

}