using System;
using System.Collections.Generic;
using Util.Debug;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Token;
using TowerDefence.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Serialisation;

namespace TowerDefence.Entity.Tower
{
	[CreateAssetMenu(fileName = "TowerPlan", menuName = "TowerDefence/Entity/Tower")]
	public class TowerPlan : ScriptableObject, IEntityPlan
	{
		#region Basic

		// Meta
		// public SerialisableGuid Guid
		// {
		// 	get { return _Guid; }
		// 	set
		// 	{
		// 		if (_Guid.IsEmpty())
		// 		{
		// 			_Guid = value;
		// 		}
		// 		else
		// 		{
		// 			LogManager.Instance.LogWarning("Attempting to set a GUID that is already set.");
		// 		}
		// 	}
		// }
		// Creates a new serailisableGuid on init
		// [FormerlySerializedAs("Guid")][SerializeField] private SerialisableGuid _Guid = new SerialisableGuid(System.Guid.NewGuid());
		public SerialisableGuid Guid { get; protected set; }
		public string Name { get; protected set; }
		public bool IsCrown { get; protected set; }

		// ===== Basic Stats =====
		public StatBlock StatBlock { get; protected set; }
		public ResourceBlock ResourceBlock { get; protected set; }
		public List<IToken> StartingTokens { get; protected set; }
		public List<SkillPlan> InitSkills { get; protected set; }
		public List<BuffPlan> InitBuffs { get; protected set; }

		// Abilities
		public List<SkillPlan> Skills { get; protected set; }

		// Behavioural
		public Tower.BulletCurve BulletType { get; protected set; }
		public Tower.Targetting Targetting { get; protected set; }

		// Game Speed
		public float reeling = 0;

		// Meta
		public ElementStat Element;
		public TowerType type = TowerType.None; // Identifies the tower
		public List<Tag> tags = new List<Tag>() { };
		public int rank = 0;

		// === Sprites ===
		public Sprite baseSprite;
		public Sprite turretSprite;
		public Sprite bulletSprite;
		//
		public Texture2D turretTexture;

		// === Audio ===
		public AudioClip attackSound;
		public AudioClip idleSound;
		public AudioClip buildSound;


		#endregion Basic

		#region Skills

		// === Periodic ===

		// === Trigger ===

		// === Channeling ===

		public float channelingTime = 0;

		#endregion Skills

		#region Lineage

		public TowerPlan NextRank;
		public TowerPlan PreviousRank;

		#endregion Lineage

		#region Ingredients

		public List<TowerPlan> ingredients = new List<TowerPlan>();
		public bool oneShot = false;
		public bool combinable = false;

		#endregion Ingredients

		#region Summative stats

		public int kills;
		public int count;


		#endregion Summative stats

		#region Util
		public static TowerPlan RankUp(TowerPlan plan, int rank)
		{
			TowerPlan _new = (TowerPlan)plan.MemberwiseClone();
			_new.rank += rank;
			return _new;
		}

		public void Deserialize()
		{
			throw new NotImplementedException();
		}

		public void Serialize()
		{
			throw new NotImplementedException();
		}

		// public void Deserialize()
		// {
		// 	throw new NotImplementedException();
		// }

		// public void Serialize()
		// {
		// 	throw new NotImplementedException();
		// }

		#endregion Util
	}

	[CustomEditor(typeof(TowerPlan))]
	public class TowerDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			// TowerPlan script = (TowerPlan)target;
			// if (GUILayout.Button("Generate new GUID"))
			// {
			// 	script.Guid = new SerialisableGuid(Guid.NewGuid());
			// }
		}
	}
}