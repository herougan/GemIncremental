using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TowerDefence.Stats;
using Util.Serialisation;
using TowerDefence.Entity.Resources;
using Unity.VisualScripting;
using TowerDefence.Entity.Skills;

namespace TowerDefence.Entity.Projectile
{
	[CreateAssetMenu(fileName = "MonsterData", menuName = "TowerDefence/Entity/Projectile")]
	public class MonsterData : ScriptableObject, IEntityData
	{
		#region Preamble

		// Id
		public SerialisableGuid Guid { get; set; }
		public string Name { get; set; }

		//  ===== Basic Stats =====
		public StatBlock StatBlock;
		public ResourceBlock ResourceBlock;

		// Effect
		public List<SkillData> Skills;
		public List<BuffData> Buffs; // Not sure if should be its own class or not

		// ===== Meta =====
		public Projectile.Type Type;
		public Projectile.Type Hybrid;
		public Projectile.Race Race;
		public List<Projectile.Tag> Tags;


		// ==== Visuals ====
		public Sprite HeadSprite;
		public Sprite BaseSprite;
		public Sprite EvolveSprite;
		//
		public Texture2D MonsterTexture;


		// ==== Audio ====
		public AudioClip SpawnSound;
		public AudioClip AttackSound;
		public AudioClip RoarSounds;
		public AudioClip DeathSound;

		#endregion Preamble

		#region Util

		public GUID id => throw new System.NotImplementedException();

		public GUID guid => throw new System.NotImplementedException();

		StatBlock IEntityData.StatBlock => throw new System.NotImplementedException();

		GUID IEntityData.Guid => throw new System.NotImplementedException();

		ResourceBlock IEntityData.ResourceBlock => throw new System.NotImplementedException();

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
	}

	[CustomEditor(typeof(MonsterData))]
	public class MonsterDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			MonsterData script = (MonsterData)target;
			if (GUILayout.Button("Generate new GUID"))
			{
				script.Guid = new SerialisableGuid(System.Guid.NewGuid());
			}
		}
	}
}