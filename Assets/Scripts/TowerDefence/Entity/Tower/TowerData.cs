using System;
using System.Collections.Generic;
using Debug;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Serialisation;

namespace TowerDefence.Entity.Tower
{
	[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefence/Entity/Tower")]
	public class TowerData : ScriptableObject, IEntityData
	{
		#region Basic

		// Id
		public SerialisableGuid Guid
		{
			get { return _Guid; }
			set
			{
				if (_Guid.IsEmpty())
				{
					_Guid = value;
				}
				else
				{
					LogManager.Instance.LogWarning("Attempting to set a GUID that is already set.");
				}
			}
		}
		[FormerlySerializedAs("Guid")][SerializeField] private SerialisableGuid _Guid = new SerialisableGuid(System.Guid.NewGuid());
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		GUID IEntityData.Guid => throw new NotImplementedException();

		StatBlock IEntityData.StatBlock => throw new NotImplementedException();

		ResourceBlock IEntityData.ResourceBlock => throw new NotImplementedException();

		[FormerlySerializedAs("Name")][SerializeField] private string _Name;

		// Basic Stats
		public StatBlock StatBlock;
		public ResourceBlock ResourceBlock;

		// Behavioural
		public Tower.BulletCurve bulletType = Tower.BulletCurve.linear;
		public Tower.Targetting targetting = Tower.Targetting.distance;

		// Game Speed
		public float reeling = 0;

		// Skills
		public List<ISkill> skills;

		// Meta
		public ElementStat Element;
		public Tower.Type type = Tower.Type.None; // Identifies the tower
		public List<Tower.Tag> tags = new List<Tower.Tag>() { };
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

		public TowerData NextRank;
		public TowerData PreviousRank;

		#endregion Lineage

		#region Ingredients

		public List<TowerData> ingredients = new List<TowerData>();
		public bool oneShot = false;
		public bool combinable = false;

		#endregion Ingredients

		#region Summative stats

		public int kills;
		public int count;


		#endregion Summative stats

		#region Util
		public static TowerData RankUp(TowerData plan, int rank)
		{
			TowerData _new = (TowerData)plan.MemberwiseClone();
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

	[CustomEditor(typeof(TowerData))]
	public class TowerDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			TowerData script = (TowerData)target;
			if (GUILayout.Button("Generate new GUID"))
			{
				script.Guid = new SerialisableGuid(Guid.NewGuid());
			}
		}
	}
}