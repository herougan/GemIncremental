
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
	public class BuffPlan : ScriptableObject, IPlan
	{
		[Header("Basic Information")]
		public string Name { get; private set; }
		public SerialisableGuid Guid { get; private set; }
		public string Description { get; private set; }
		public int Level { get; private set; }
		public int MaxLevel { get; private set; }
		public int Rank { get; private set; }
		public SkillPlan Basis { get; private set; } // Basis for Buffplan

		// Overwritten with custom editor
		[HideInInspector] public float Cooldown { get; private set; }
		[HideInInspector] public float Duration { get; private set; }
		[HideInInspector] public float Range { get; private set; }
		[HideInInspector] public float Damage { get; private set; }
		[HideInInspector] public double Radius { get; private set; }

		[Header("Gameplay")]
		public List<DepletableStat> Costs { get; private set; }
		public List<ResourceStat> ResourceCosts { get; private set; }

		[SerializeField]
		public List<Effect> Effects { get; private set; }
		public List<Passive> Passives { get; private set; }

		public BuffStackType StackType { get; private set; }
		// public List<IModification> Modifications = new List<IModification>();

		// effects

		// bools (in buff)

		// Ancestry
		public BuffPlan Predecessor;
		public BuffPlan Successor;


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
				case TriggerType.None:

					break;
				case TriggerType.Permanent:
					break;
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

				case TriggerType.OnEnterRange:
					break;

				case TriggerType.OnExitRange:
					break;

				// case TriggerType.OnStatusIn:
				// 	break;

				// case TriggerType.OnStatusOut:
				// 	break;

				case TriggerType.OnSkillUse:
					break;

					// case TriggerType.OnSkillUse:
					// 	break;
			}


			// Draw the rest
			DrawDefaultInspector();
		}
	}


#endif

	#endregion Class and Enums

}