using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using UnityEditor;
using Util.Maths;
using Random = UnityEngine.Random;

namespace TowerDefence.Entity.Monster
{
	[Serializable]
	public class Monster : Entity
	{
		#region Preamble
		public MonsterData Plan;

		// Stats

		// Meta

		// ===== Getter/Setter =====

		/// <summary>
		/// Adds a buff to the list. Can trigger OnBuffApplied or OnDebuffApplied depending on buff.IsPositive.
		/// This is a dumb function! There is no game logic.
		/// </summary>
		/// <param name="buff"></param>
		public void AddBuff(IBuff buff)
		{
			Buffs.Add(buff);
			if (buff.IsPositive)
			{
				OnBuffApplied.Invoke(this, buff);
			}
			else
			{
				OnDebuffApplied.Invoke(this, buff);
			}
		}
		public void RemoveBuff(IBuff buff)
		{
			if (Buffs.Remove(buff)) OnBuffExpire.Invoke(this, buff);
		}

		public Monster(MonsterData plan)
		{
			StatBlock = plan.StatBlock;

			this.plan = plan;
			// Add buffs and skill
			foreach (var buff in plan.Buffs)
			{
				// Create Buffs
				IBuff newBuff = new Buff(buff);
				SkillsLib.ApplyBuff(newBuff, this);
				Buffs.Add(newBuff);
			}
			foreach (var skill in plan.Skills)
			{
				// Create Skills
				ISkill newSkill = new Skill(skill);
				SkillsLib.
				Skills.Add(newSkill);
			}

			// Register Callbacks
			RegisterStatCallbacks();
		}

		public override string ToString()
		{
			return $"{plan.name}";
		}

		#endregion Preamble

		#region Game

		/********************************************************************

		Reset on new map or new login.

		********************************************************************/

		// Spawn Time
		public float spawnTime;

		// Game record
		public IEntity LastDamageEntity;
		public ISource LastDamageSource;

		#endregion Game

		#region Event

		// === Generic ===
		public event Action<IEntity, IExpirable> OnBuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffApplied = delegate { };
		public event Action<IEntity, IExpirable> OnBuffExpire = delegate { };
		public event Action<IEntity, IExpirable> OnDebuffExpire = delegate { };
		public event Action<IEntity, IStat, double> OnStatChange = delegate { }; // IEntity, Stat, Amount Changed
		public event Action<IEntity, IDepletable, double> OnStatDecreased = delegate { }; // IEntity, IDepletable, Amount Decreased
		public event Action<IEntity, IRegenerable, double> OnRegenValueChanged = delegate { };
		public event Action<IEntity, IRegenerable> OnRegenerate = delegate { };

		// === Game Space ===
		// These events are triggered by the MonsterController which lives in Game Space.
		public Action<IEntity> OnSpawn = delegate { };
		public Action<IEntity> OnReached = delegate { };
		public Action<IEntity, ISource> OnEnteredRange = delegate { };
		public Action<IEntity, ISource> OnExitRange = delegate { };
		public Action<IEntity> OnEnterAttackRange = delegate { };
		public Action<IEntity> OnExitAttackRange = delegate { };

		// Combat
		public event Action<IEntity, IEntity> OnHit = delegate { };
		public event Action<IEntity, IEntity> OnDOT = delegate { };
		public event Action<IEntity> OnDeath = delegate { }; // If Health <= 0

		// Custom to Monster class
		public event Action<IEntity, IDepletable, double> OnHealthDecreased = delegate { };
		public event Action<IEntity, IDepletable, double> OnManaDecreased = delegate { };


		// Meta
		public event Action<IEntity, ResourceStat> OnEntityResourceChanged = delegate { };


		#endregion Event

		#region Stats
		public StatBlock StatBlock { get; private set; }
		public ResourceBlock ResourceBlock { get; private set; }

		Action<IEntity> IEntity.OnReached => throw new NotImplementedException();

		Action<IEntity> IEntity.OnSpawn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity, ISource> IEntity.OnExitRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity, ISource> IEntity.OnExitAttackRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity, Tower> IEntity.OnHit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity, Tower> IEntity.OnDOT { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity> IEntity.OnDeath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public List<IAffect> Affects => throw new NotImplementedException();

		// Current
		void RegisterDepletionCallbacks()
		{
			// Register all depletable stats
			foreach (var stat in StatBlock.GetDepletableStats())
			{
				stat.OnValueChanged += (stat, original) =>
				{
					LogManager.Instance.Log($"Stat {stat.Type} changed from {original} to {stat.Value}");
					OnStatChange.Invoke(this, stat, original);
				};
			}
		}

		void RegisterExpirableCallbacks()
		{
		}

		void RegisterStatCallbacks()
		{
			// Register all stats
			foreach (IStat stat in StatBlock.Stats)
			{
				stat.OnValueChanged += (stat, original) => { OnStatChange.Invoke(this, stat, original); };
				if (stat is IDepletable)
				{
					(stat as IDepletable).OnValueDecreased += (stat, change) => OnStatDecreased(this, stat as IDepletable, change);
					(stat as IDepletable).OnMaxValueChanged += (stat, change) => OnStatChange(this, stat as IDepletable, change);
					if (stat.Type == StatType.Health) (stat as IDepletable).OnValueDecreased += (stat, change) => OnHealthDecreased(this, stat as IDepletable, change);
					if (stat.Type == StatType.Mana) (stat as IDepletable).OnValueDecreased += (stat, change) => OnManaDecreased(this, stat as IDepletable, change);
				}
				if (stat is IRegenerable)
				{
					(stat as IRegenerable).OnRegenValueChanged += (stat, original) => OnRegenValueChanged(this, stat as IRegenerable, original);
					(stat as IRegenerable).OnRegenerate += (stat, value) => OnRegenerate(this, stat as IRegenerable);
				}
			}

			// Custom Handling
			// Nopne.
		}

		void DeregisterStatCallbacks()
		{
			// Just Dispose();
		}

		#endregion Stats

		#region Behaviour
		public EntityBehaviourType behaviour { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		IEntityData IEntity.Plan => throw new NotImplementedException();

		public GUID guid => throw new NotImplementedException();

		public List<IBuff> Buffs => throw new NotImplementedException();

		public List<IBuff> TowerAuras => throw new NotImplementedException();

		public List<IBuff> MonsterAuras => throw new NotImplementedException();

		public List<IBuff> AuraInstances => throw new NotImplementedException();

		public EntityBehaviourType Behaviour => throw new NotImplementedException();

		public List<IStatus> Statuses => throw new NotImplementedException();

		Action<IEntity, ISource> IEntity.OnEnteredRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Action<IEntity, ISource> IEntity.OnEnteredAttackRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public List<Tag> Tags => throw new NotImplementedException();
		List<IAffect> ISource.Affects => throw new NotImplementedException();

		#endregion Behaviour

		#region Util

		public float SummariseDefence()
		{
			return 1;
		}

		public float SummarisePower()
		{
			return 1;
		}

		public void TickExpirables(float time)
		{
			// Calculate
			foreach (IExpirable buff in Buffs)
			{
				buff.Tick(time);
			}
		}

		public void RegisterCallbacks()
		{
			throw new NotImplementedException();
		}

		public void EntityDied()
		{
			throw new NotImplementedException();
		}

		public void EntityBuffed(IBuff buff)
		{
			throw new NotImplementedException();
		}

		public void EntityHit(Tower tower)
		{
			throw new NotImplementedException();
		}

		public void EntityReachedDestination()
		{
			throw new NotImplementedException();
		}

		public void HealthChanged()
		{
			throw new NotImplementedException();
		}

		public void ApplyModification(IStatChanger mod)
		{
			throw new NotImplementedException();
		}

		public void ApplyDamage(IDamager damager)
		{
			throw new NotImplementedException();
		}

		#endregion Util

		#region Calculation

		/// <summary>
		/// Returns BASE * SCALED * INTERNAL_MODIFIERS
		/// Some values are dynamically calculated, such as bonus from "fires" or "dark bonds" that change during the game.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public double GetEffectiveStat(StatType type)
		{
			double effective = StatBlock.Stats.FirstOrDefault(s => s.Type == type)?.Value ?? 0;

			// Is the effect on init, editting the statblock? we can try ONLY running this on init
			// WHEN do we scale the buff/skills/ upon scale?

			// Calculate
			foreach (var buff in Buffs)
			{
				// (buff as BuffData).Trigger;
				// if ((buff as Buff).Data.Trigger.Type == TriggerType.None)
				// {
				// 	(buff as Buff).Data.Effect.Scale(effective);
				// }

				foreach (Passive passive in buff.Data.Passives)
				{
					// Check if Passive Stats apply:
					if (SkillsLib.CheckCondition(this, passive.Conditions))
					{
						for (int i = 0; i < passive.Values.Count; ++i)
						{
							if (passive.StatTypes[i] == type)
							{
								effective = MathsLib.Operate(effective, passive.Values[i], passive.Operations[i]);
							}
						}
					}
				}

				// // Assume all are "multiplication!"
				// buff.Modifications.OrderBy(mod => mod.Operation);
				// foreach (IModification mod in buff.Modifications)
				// {
				// 	if (EntityUtil.Check(this, mod.Conditions))
				// 		MathsLib.Operate(effective, mod.Value, mod.Operation);
				// }
			}

			// Store calculated value (BASE * SCALE = VALUE) * (BUFF.MODIFICATIONS.(MODIFIERS * CONDITIONS))
			GetStat(type).Dynamic = effective;
			return effective;
		}

		/// <summary>
		/// Scales the monster's stats, buffs, and skills.
		/// This function is ran on Monster initialistsion, after World & Player are loaded.
		/// You can call this function again to re-scale the monster based on updated World & Player values.
		/// For in-game modifications, use the specific Scale functions. (ScaleByEffects e.g.)
		/// </summary>
		public void Scale()
		{
			//
		}

		public void Scale(StatType type, double scale)
		{
			// Scale Skills

			// Scale Buffs

			// Effects that have left the monster will not be re-scaled.

		}

		public void ScaleEffects(List<Effect> effects)
		{
			foreach (Effect effect in effects)
			{
				ScaleEffect(effect);
			}
		}

		public void ScaleEffect(Effect effect)
		{
			// Scale the effect
			effect.Scale(1.0);
		}

		public void ScaleByEffects(List<Effect> effects)
		{
			// Scale Skills
			foreach (var effect in effects)
			{
				ScaleByEffect(effect);
			}
		}

		public void ScaleByEffect(Effect effect)
		{
			// Apply skill effect's ApplyAction
		}

		public void Scale(ElementType element, double scale)
		{

		}

		// public void Scale(List<StatType> types, List<double> values)
		// {
		// 	//;
		// 	StatBlock.Level.Value = 0;

		// 	foreach (var buff in Buffs)
		// 	{
		// 		if ((buff as Buff).Data.Effect.StatType in Types) {
		// 			buff.Scale(types, values);
		// 		}
		// 	}
		// 	foreach (var skill in Skills)
		// 	{
		// 		skill.Scale(types, values);
		// 	}
		// }

		public IStat GetStat(StatType type)
		{
			foreach (var stat in StatBlock.Stats)
			{
				if (stat.Type == type)
				{
					return stat;
				}
			}

			throw new Exception($"Stat {type} not found");
		}

		public void Scale(List<StatType> types, List<double> values)
		{
			throw new NotImplementedException();
		}

		public void RegisterSkillTrigger(ISkill Skill)
		{
			throw new NotImplementedException();
		}

		public float GetKinematics(KinematicsType type)
		{
			throw new NotImplementedException();
		}

		public float GetMileage(MileageType type)
		{
			throw new NotImplementedException();
		}

		public int GetResource(ResourceType type)
		{
			throw new NotImplementedException();
		}

		public int GetCounter(CounterType type)
		{
			throw new NotImplementedException();
		}

		public int GetInventory(ItemType type)
		{
			throw new NotImplementedException();
		}

		public ddouble GetStatus(StatusType type)
		{
			throw new NotImplementedException();
		}

		public float GetKinematics(KinematicsType type)
		{
			throw new NotImplementedException();
		}

		public int GetCounter(CounterType type)
		{
			throw new NotImplementedException();
		}

		public void AddBuff(IBuff buff)
		{
			throw new NotImplementedException();
		}

		public void RemoveBuff(IBuff buff)
		{
			throw new NotImplementedException();
		}

		public void ApplyModification(Skills.Action action)
		{
			throw new NotImplementedException();
		}

		public void ApplyDamage(ddouble damager)
		{
			throw new NotImplementedException();
		}

		public bool ContainStatus(StatusType statusType)
		{
			throw new NotImplementedException();
		}

		public StatusStat GetStatus(StatusType statusType)
		{
			throw new NotImplementedException();
		}

		public void RegisterSkillTrigger(ISkill Skill)
		{
			throw new NotImplementedException();
		}

		public void AddTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public void RemoveTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public bool HasTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		public bool CheckMeta(MetaType metaType, string data)
		{
			throw new NotImplementedException();
		}

		public int GetResource(ResourceType type)
		{
			throw new NotImplementedException();
		}

		public int GetResource(ResourceType type)
		{
			throw new NotImplementedException();
		}

		#endregion Calculation

		#region Enum

		public enum Race
		{
			Slime,
			Beast,
			Undead,
			Elf,
			Ork,
			Dwarf,
			Human,
			Kobold,
			Gnome,
			Spirit,
			Robot,
			Mollusc,
		}
		public enum Type
		{
			None,
			Bloodo,
			Bubbling,
			Dewy,
			Drippie,
			Ferro,
			Gooey,
			Grimeo,
			Melta,
			Muddy,
			Oily,
			Plasma,
			Riverling,
			Rotto,
			Slimey,
			Splashy,
			Sticky,
			Sweetie,
			Toxa,
			// Beast
			Bat,
			DireWolf,
			Felid,
			Werewolf,
			// Spirit
			Incarnation,
			Ghost,
			Avatar,
			// Robot
			RobotWalker,
			// None,
			// // Forest(s)
			// Slime,
			// Bat,
			// // Cave(s)
			// Undead,
			Skeleton,
			Pumpkin,
			Ghoul,
			// Milkbool,
			// // Techno
			// Electromite,
			// Mollusc
			Octopus,
		}
		public enum Category
		{
			/* Basic */
			Common,
			Uncommon,

			/* Rare */
			Rare,
			SuperRare,
			Epic,

			/* Individual */
			Heroic,
			Legendary,
			Mythic,
		}

		#endregion Enum
	}
}