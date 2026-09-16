using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TowerDefence.Stats;
using Util.Maths;
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
		[field: SerializeField] public string Name { get; set; }
		[field: SerializeField] public bool IsBoss { get; set; }


		//  ===== Basic Stats =====
		// Sparse, Inspector-authored starting stats. An Entity spawned from this Plan builds its own
		// fresh StatBlock/ElementBlock from these (see Entity(IEntityPlan) constructor) - nothing here
		// is shared/mutated at runtime. Only what's listed is ever instantiated.
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

		// ===== Meta =====
		[field: SerializeField] public MonsterType Type { get; set; }
		public MonsterType Hybrid { get; protected set; }
		[field: SerializeField] public Monster.Race Race { get; set; }
		public List<Tag> Tags { get; protected set; }


		// ==== Visuals ====
		public Sprite HeadSprite { get; protected set; }
		public Sprite BaseSprite { get; protected set; }
		public Sprite EvolveSprite { get; protected set; }
		//
		public Texture2D Texture { get; protected set; }

		// The GameObject/MonsterController prefab this Plan spawns as - every other spawn path
		// (EntityWaveManager, MvpDemoSpawner) has always taken a prefab as a *separate* argument from
		// whoever's calling EntityManager.SpawnMonster, which only works when that caller has one handy.
		// SpawnActionHandler (ActionType.Spawn - "summon creatures on death" etc.) doesn't: a Skill effect
		// firing deep in game logic has no scene reference to pass in, so the Plan needs to carry its own,
		// same as ProjectilePlan.ProjectilePrefab/TowerPlan's sprite fields already do.
		[field: SerializeField] public GameObject Prefab { get; set; }


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

			ResourceBlock = new ResourceBlock();

			StartingTokens = new List<IToken>() { };
			InitSkills = new List<SkillPlan>() { }; // was never initialized - Entity.Spawn() iterates this unconditionally
			InitBuffs = new List<BuffPlan>() { };

			Skills = new List<SkillPlan>() { };

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