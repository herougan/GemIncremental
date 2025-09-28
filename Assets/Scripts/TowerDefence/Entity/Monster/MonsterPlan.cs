using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TowerDefence.Stats;
using Util.Serialisation;
using TowerDefence.Entity.Resources;
using Unity.VisualScripting;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Token;
using TowerDefence.Entity.Skills.Buffs;

namespace TowerDefence.Entity.Monster
{
	[CreateAssetMenu(fileName = "MonsterPlan", menuName = "TowerDefence/Entity/Monster")]
	public class MonsterPlan : ScriptableObject, IEntityPlan
	{
		#region Preamble

		// ===== Meta =====
		public SerialisableGuid Guid { get; protected set; }
		public string Name { get; protected set; }
		public bool IsBoss { get; protected set; }


		//  ===== Basic Stats =====
		public StatBlock StatBlock { get; protected set; }
		public ResourceBlock ResourceBlock { get; protected set; }
		public List<IToken> StartingTokens { get; protected set; }
		public List<SkillPlan> InitSkills { get; protected set; }
		public List<BuffPlan> InitBuffs { get; protected set; }

		// Abilities
		public List<SkillPlan> Skills { get; protected set; }

		// ===== Meta =====
		public MonsterType Type { get; protected set; }
		public MonsterType Hybrid { get; protected set; }
		public Monster.Race Race { get; protected set; }
		public List<Tag> Tags { get; protected set; }


		// ==== Visuals ====
		public Sprite HeadSprite { get; protected set; }
		public Sprite BaseSprite { get; protected set; }
		public Sprite EvolveSprite { get; protected set; }
		//
		public Texture2D Texture { get; protected set; }


		// ==== Audio ====
		public AudioClip SpawnSound;
		public AudioClip AttackSound;
		public AudioClip RoarSounds;
		public AudioClip DeathSound;

		#endregion Preamble

		#region Util

		public void Deserialize()
		{
			throw new System.NotImplementedException();
		}

		public void Serialize()
		{
			throw new System.NotImplementedException();
		}

		// public void Deserialize()
		// {
		// 	throw new System.NotImplementedException();
		// }

		// public void Serialize()
		// {
		// 	throw new System.NotImplementedException();
		// }

		#endregion Util

		#region Init

		public MonsterPlan()
		{
			// Init default values
			Guid = new SerialisableGuid(System.Guid.NewGuid());
			Name = "New Monster";

			StatBlock = new StatBlock();
			ResourceBlock = new ResourceBlock();

			StartingTokens = new List<IToken>() { };
			InitBuffs = new List<BuffPlan>() { };

			Skills = new List<SkillPlan>() { };
			InitBuffs = new List<BuffPlan>() { };

			Type = MonsterType.None;
		}

		#endregion Inits
	}

	[CustomEditor(typeof(MonsterPlan))]
	public class MonsterDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			// MonsterPlan script = (MonsterPlan)target;
			// if (GUILayout.Button("Generate new GUID"))
			// {
			// 	script.Guid = new SerialisableGuid(System.Guid.NewGuid());
			// }
		}
	}
}