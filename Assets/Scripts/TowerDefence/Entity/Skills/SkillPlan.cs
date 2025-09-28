
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

		public string Name { get; set; }
		public SerialisableGuid Guid { get; set; }
		public string Description { get; set; }
		public int Level { get; set; }
		public int MaxLevel { get; set; }
		public float Cooldown { get; set; }
		public float Range { get; set; }
		public ddouble Damage { get; set; }
		public float Radius { get; set; }
		public float ChannelingTime { get; set; }

		[Header("Meta")]
		public bool IsPositive { get; set; }
		public bool ForMonster { get; set; }
		public bool ForTower { get; set; }

		[Header("Gameplay")]
		public double Cost { get; set; }
		public List<DepletableStat> StatCosts { get; set; }
		public List<ResourceStat> ResourceCosts { get; set; }

		[SerializeField]
		public List<IEffect> Effects { get; set; }
		public List<IPassive> Passives { get; set; }

		// Ancestry
		public SkillPlan Predecessor { get; set; }
		public SkillPlan Successor { get; set; }


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