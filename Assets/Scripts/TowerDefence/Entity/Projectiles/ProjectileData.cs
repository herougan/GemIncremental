using System.Collections.Generic;
using UnityEngine;
using TowerDefence.Stats;
using Util.Serialisation;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;

namespace TowerDefence.Entity.Projectile
{
	[CreateAssetMenu(fileName = "MonsterPlan", menuName = "TowerDefence/Entity/Projectile")]
	public class ProjectileData : ScriptableObject
	{
		#region Preamble

		// Id
		public SerialisableGuid Guid { get; set; }
		public string Name { get; set; }

		//  ===== Basic Stats =====
		public StatBlock StatBlock;
		public ResourceBlock ResourceBlock;

		// Effect
		public List<SkillPlan> Skills;
		public List<BuffPlan> Buffs; // Not sure if should be its own class or not

		// ===== Meta =====
		public ProjectileType Type;
		public ProjectileType Hybrid;
		public List<Tag> Tags;


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

		public void Deserialize()
		{
			throw new System.NotImplementedException();
		}

		public void Serialize()
		{
			throw new System.NotImplementedException();
		}

		#endregion Util
	}
}