

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TowerDefence.Stats;
using Util.Maths;
using Util.Serialisation;
using TowerDefence.Entity.Skills.Effects;
using TowerDefence.Entity.Resources;

namespace TowerDefence.Entity.Skills
{
	#region Class and Enums
	[CreateAssetMenu(fileName = "Skill", menuName = "TowerDefence/Skills/Skill")]
	public class SkillData : ScriptableObject
	{
		[Header("Basic Information")]
		public string Name;
		public SerialisableGuid guid;
		public string Description;
		public int Level;
		public int MaxLevel;
		public float Cooldown;
		public float Duration;
		public float Range;
		public ddouble Damage;
		public float Radius;
		public float ChannelingTime;

		[Header("Gameplay")]
		public List<DepletableStat> Costs;
		public List<ResourceStat> ResourceCosts;

		[SerializeField]
		public List<Effect> Effects { get; set; }
		public List<IPassive> Passives { get; set; }

		// Ancestry
		public SkillData Predecessor;
		public SkillData Successor;

		public bool IsPositive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool ForMonster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool ForTower { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public double Cost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


		// public event Action<ISource, IModifier> OnApplied;

		public void Recalculate(ddouble scale)
		{
			foreach (Effect effect in Effects)
			{
				effect.Recalculate(scale);
			}
		}
	}
	#endregion Class and Enums

	#region Editor


#if UNITY_EDITOR
	[CustomEditor(typeof(SkillData))]
	public class SkillDataEditor : Editor
	{
		public enum SkillType
		{

		}
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			// MonsterData script = (MonsterData)target;
			if (GUILayout.Button("Generate new GUID"))
			{
				// script.Guid = new SerialisableGuid(System.Guid.NewGuid());
			}
		}
	}

#endif // UNITY_EDITOR
	#endregion Editor
}