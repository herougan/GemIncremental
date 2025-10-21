
using System.Collections.Generic;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Skills.Passives;
using TowerDefence.Stats;
using UnityEditor;
using UnityEngine;
using Util.Serialisation;

namespace TowerDefence.Entity.Skills.Buffs
{
	#region Class and Enums

	[CreateAssetMenu(fileName = "Skill", menuName = "TowerDefence/Skills/Buff")]
	public class BuffPlan : SkillPlan
	{
		// ===== Buff Specific =====
		public float Duration { get; private set; }
		public BuffStackType StackType { get; private set; }


		// Ancestry
		public new BuffPlan Predecessor;
		public new BuffPlan Successor;


		public void ApplyEffect(IEntity source, IEntity target)
		{
			throw new System.NotImplementedException();
		}

		public void ApplyDamage()
		{

		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(BuffPlan))]
	public class BuffPlanEditor : Editor
	{
		public TriggerType TriggerType;

		public override void OnInspectorGUI()
		{
			BuffPlan script = (BuffPlan)target;
			// TriggerType = script.Trigger.Type;

			// Draw hidden components based on the TriggerType
			switch (TriggerType)
			{
				case TriggerType.OnCast:

					break;
				case TriggerType.OnHit:

					break;
				case TriggerType.OnKill:

					break;
				case TriggerType.OnDeath:

					break;
				case TriggerType.OnAttack:

					break;
				case TriggerType.OnAttacked:


					break;
				case TriggerType.OnPeriodic:
					break;

				case TriggerType.OnEnteredRange:
					break;

				case TriggerType.OnExitRange:
					break;

					// case TriggerType.OnStatusIn:
					// 	break;

					// case TriggerType.OnStatusOut:
					// 	break;


					// case TriggerType.OnSkillUse:
					// 	break;
			}


			// Draw the rest
			DrawDefaultInspector();


			// Copy skill
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Copy From Existing SkillPlan", EditorStyles.boldLabel);

			// Field to select an existing SkillPlan asset
			SkillPlan sourceSkillPlan = (SkillPlan)EditorGUILayout.ObjectField(
				"Source SkillPlan",
				null,
				typeof(SkillPlan),
				false
			);

			if (sourceSkillPlan != null)
			{
				if (GUILayout.Button("Copy Properties"))
				{
					// Copy properties from sourceSkillPlan to script (BuffPlan)
					script.Name = sourceSkillPlan.Name;
					var type = typeof(SkillPlan);
					var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
					foreach (var field in fields)
					{
						if (field.IsInitOnly) continue; // skip readonly fields
						field.SetValue(script, field.GetValue(sourceSkillPlan));
					}



					// Copy other relevant properties as needed

					// Mark as dirty so Unity saves changes
					EditorUtility.SetDirty(script);
				}
			}
		}
	}


#endif

	#endregion Class and Enums

}