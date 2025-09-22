

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TowerDefence.Stats;
using Util.Maths;
using Util.Serialisation;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills.Passives;

namespace TowerDefence.Entity.Skills
{
	#region Class and Enums
	[CreateAssetMenu(fileName = "Skill", menuName = "TowerDefence/Skills/Skill")]
	public class SkillPlan : ScriptableObject, IPlan
	{
		[Header("Basic Information")]

		public string Name { get; protected set; }
		public SerialisableGuid Guid { get; protected set; }
		public string Description { get; protected set; }
		public int Level { get; protected set; }
		public int MaxLevel { get; protected set; }
		public float Cooldown { get; protected set; }
		public float Duration { get; protected set; }
		public float Range { get; protected set; }
		public ddouble Damage { get; protected set; }
		public float Radius { get; protected set; }
		public float ChannelingTime { get; protected set; }

		[Header("Meta")]
		public bool IsPositive { get; protected set; }
		public bool ForMonster { get; protected set; }
		public bool ForTower { get; protected set; }
		public double Cost { get; protected set; }

		[Header("Gameplay")]
		public List<DepletableStat> StatCosts { get; protected set; }
		public List<ResourceStat> ResourceCosts { get; protected set; }

		[SerializeField]
		public List<IEffect> Effects { get; protected set; }
		public List<IPassive> Passives { get; protected set; }

		// Ancestry
		public SkillPlan Predecessor { get; protected set; }
		public SkillPlan Successor { get; protected set; }


		// public event Action<ISource, IModifier> OnApplied;

		public void Recalculate(ddouble scale)
		{
			foreach (IEffect effect in Effects)
			{
				effect.Recalculate(scale);
			}
		}
	}
	#endregion Class and Enums

	#region Editor


#if UNITY_EDITOR
	[CustomEditor(typeof(SkillPlan))]
	public class SkillDataEditor : Editor
	{
		public enum SkillType
		{

		}
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			// MonsterPlan script = (MonsterPlan)target;
			if (GUILayout.Button("Generate new GUID"))
			{
				// script.Guid = new SerialisableGuid(System.Guid.NewGuid());
			}
		}
	}

#endif // UNITY_EDITOR
	#endregion Editor
}