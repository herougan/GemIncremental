
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
		[field: SerializeField] public float Duration { get; set; }
		public BuffStackType StackType { get; private set; }

		// ===== Aura (see Buff.Create/Aura.cs) =====
		// Authoring an Aura is just flagging an ordinary BuffPlan, not a separate asset type - whatever
		// grants this Plan (Entity.Spawn's InitBuffs, or ActionType.ApplyBuff) goes through Buff.Create,
		// which builds a real Aura (not a plain Buff) the moment IsAura is set, no other wiring needed.
		[field: SerializeField] public bool IsAura { get; set; }
		[field: SerializeField] public float AuraRange { get; set; }
		[field: SerializeField] public bool AuraAffectSelf { get; set; }
		[field: SerializeField] public bool AuraAffectOthers { get; set; } = true;
		[field: SerializeField] public bool AuraSpreading { get; set; }
		// 0 = removed the instant an entity leaves Range; >0 = they keep it as a normal expiring Buff
		// for this many more seconds after leaving (see Aura.Leave/IBuff.SetDuration).
		[field: SerializeField] public float AuraLingerDuration { get; set; }

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