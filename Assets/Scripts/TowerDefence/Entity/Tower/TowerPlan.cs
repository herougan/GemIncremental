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
using Util.Maths;
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
		[field: SerializeField] public string Name { get; set; }
		public bool IsCrown { get; protected set; }

		// ===== Basic Stats =====
		// Sparse, Inspector-authored starting stats - see MonsterPlan for the rationale (Entity builds
		// its own fresh StatBlock/ElementBlock from these; nothing here is shared/mutated at runtime).
		[field: SerializeField] public ddouble StartingHealth { get; set; } = 1;
		[field: SerializeField] public List<StatEntry> StatEntries { get; protected set; } = new List<StatEntry>();
		[field: SerializeField] public List<StatusEntry> StatusEntries { get; protected set; } = new List<StatusEntry>();
		[field: SerializeField] public List<ElementEntry> ElementEntries { get; protected set; } = new List<ElementEntry>();
		public ResourceBlock ResourceBlock { get; protected set; }
		public List<IToken> StartingTokens { get; protected set; }
		[field: SerializeField] public List<SkillPlan> InitSkills { get; protected set; }
		[field: SerializeField] public List<BuffPlan> InitBuffs { get; protected set; }

		// Abilities
		public List<SkillPlan> Skills { get; protected set; }

		// Behavioural
		[field: SerializeField] public Tower.BulletCurve BulletType { get; protected set; }
		// Which of the Monsters currently in Range (see TowerAttackController) gets shot - health
		// (lowest first), percentageHealth, debuffed, distance (closest to the end of the path),
		// proximity (closest to this Tower). Wasn't actually serialized before this ([field:
		// SerializeField] was missing) - always silently defaulted to Targetting.health.
		[field: SerializeField] public Tower.Targetting Targetting { get; set; }
		// Only Monsters carrying at least one of these are valid targets at all - empty means no
		// restriction. "flying only" from the design discussion.
		[field: SerializeField] public List<Tag> TargetingTags { get; protected set; } = new List<Tag>();

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

		#region Upgrades

		/// <summary>One Skill the Upgrade Centre can offer for purchase on a Tower spawned from this Plan - see Util.Game.TowerUpgradeUtil. Threshold is measured against Tower.UpgradePurchases' total (every stat purchase counts, not a specific stat) - "unlock skills at some thresholds - these skills themselves have to be bought again," per the design discussion: reaching Threshold only makes it purchasable, it doesn't grant it for free.</summary>
		[Serializable]
		public struct SkillUnlock
		{
			public int Threshold;
			public SkillPlan Skill;
			public ddouble Cost;
		}

		[field: SerializeField] public List<SkillUnlock> SkillUnlocks { get; protected set; } = new();

		#endregion Upgrades

		#region Summative stats

		public int kills;
		public int count;


		#endregion Summative stats

		#region Init

		public TowerPlan()
		{
			Guid = new SerialisableGuid(System.Guid.NewGuid());
			Name = "New Tower";

			ResourceBlock = new ResourceBlock();
			StartingTokens = new List<IToken>() { };
			InitSkills = new List<SkillPlan>() { };
			InitBuffs = new List<BuffPlan>() { };
		}

		#endregion Init

		#region Util
		public static TowerPlan RankUp(TowerPlan plan, int rank)
		{
			TowerPlan _new = Instantiate(plan);
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