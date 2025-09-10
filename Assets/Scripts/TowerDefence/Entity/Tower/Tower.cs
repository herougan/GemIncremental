using System;
using System.Collections.Generic;
using Towerdefence.Entity.Items;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Stats;
using UnityEngine;
using Util.Maths;
using Util.Serialisation;

namespace TowerDefence.Entity.Tower
{
	[System.Serializable]
	public class Tower : Entity
	{
		#region Preamble

		[SerializeField]
		// Config and Mutators
		public TowerData Plan;
		// public TowerUpgrade Upgrade;

		// Identity
		public UnityEditor.GUID guid;
		public string Nickname;

		// Stats
		// public List<IBuff> Buffs { get; }
		public StatBlock StatBlock { get; }
		// public ResourceBlock ResourceBlock { get; }

		public Tower(TowerData plan)
		{
			StatBlock = plan.StatBlock;

			this.guid = UnityEditor.GUID.Generate();
			this.Plan = plan;
		}

		public Tower Fuse(List<Tower> children)
		{
			this.Children = children;
			this.DateCreated = DateTime.Now;
			this.guid = UnityEditor.GUID.Generate();
			return this;
		}

		public static Tower Blank()
		{
			return new Tower(ResourceAllocater.Instance.TowerDict[Tower.Type.None]);
		}

		public override string ToString()
		{
			return $"{Plan.name} ({coord.Item1}, {coord.Item2})";
		}

		// ===== Lineage =====
		public List<Tower> Children { get; private set; }// Set on fusion
		public DateTime DateCreated { get; private set; }


		#endregion Preamble

		#region Stats

		public double attack; // to be derived from TowerData or...
		public double maxHealth;
		public float range;
		public double defence;
		//
		public double armourDefence;
		public double armourHealth;

		//
		public double cooldown;
		public double cost;

		//
		public float turnSpeed;
		public float arcOfFire;
		public float reeling;


		#endregion Stats

		#region Game State

		public double health;
		public (int, int) coord;

		#endregion Game State

		#region Cumulative State

		public int kills = 0;
		public int time = 0;
		public int mvp_attacker = 0;
		public int mvp_status = 0;

		#endregion Cumulative State

		#region Meta
		public DateTime datePurchased;
		public DateTime lastDatePurchased;
		public DateTime dateFused;
		public DateTime dateMetamorphised;

		public event Action<IEntity, IExpirable> OnBuffApplied;
		public event Action<IEntity, IExpirable> OnDebuffApplied;
		public event Action<IEntity, IExpirable> OnBuffExpire;
		public event Action<IEntity, IExpirable> OnDebuffExpire;
		public event Action<IEntity, IStat, double> OnStatChange;
		public event Action<IEntity, IDepletable, double> OnStatDecreased;
		public event Action<IEntity, IRegenerable, double> OnRegenValueChanged;
		public event Action<IEntity, IRegenerable> OnRegenerate;
		public event Action<IEntity> OnReached;
		public event Action<IEntity> OnSpawn;
		public event Action<IEntity, Tower> OnHit;
		public event Action<IEntity, Tower> OnDOT;
		public event Action<IEntity> OnDeath;
		public event Action<IEntity, IStat, ddouble> OnValueChanged;
		public event Action<IEntity, IStat, ddouble> OnValueDecreased;
		public event Action<IEntity, IStat, ddouble> OnValueIncreased;
		public event Action<IEntity, IStat, ddouble> OnMaxValueChanged;
		public event Action<IEntity, IStat, ddouble> OnCurrentValueDecreased;
		public event Action<IEntity, IStat, ddouble> OnCurrentValueIncreased;
		public event Action<IEntity, IStat, IStatMod> OnStatBonusAdded;
		public event Action<IEntity, IStat, IStatMod> OnStatNerfAdded;
		public event Action<IEntity, IStat, ddouble> OnRegenRateChanged;
		public event Action<IEntity, IElement, ddouble> OnResistChanged;
		public event Action<IEntity, IElement, ddouble> OnMasteryChanged;
		public event Action<IEntity, ISource> OnHurt;
		public event Action<IEntity, ISource> OnHeal;

		public void RegisterStatCallbacks()
		{
			foreach (IStat stat in StatBlock.Stats)
			{
				stat.OnValueChanged += (stat, delta) => OnStatChange(this, stat, delta);
			}

			foreach (IStat stat in StatBlock.GetDepletableStats())
			{

			}
		}

		public void RegisterExpirableCallbacks()
		{

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

		public int GetResource(Resources.ResourceType type)
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

		public void ApplyDamage(ddouble damager)
		{
			throw new NotImplementedException();
		}

		public void Hurt(ddouble value)
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

		public void AddTag(TowerDefence.Entity.Tag tag)
		{
			throw new NotImplementedException();
		}

		public void RemoveTag(TowerDefence.Entity.Tag tag)
		{
			throw new NotImplementedException();
		}

		public bool HasTag(TowerDefence.Entity.Tag tag)
		{
			throw new NotImplementedException();
		}

		public bool CheckMeta(MetaType metaType, string data)
		{
			throw new NotImplementedException();
		}


		#endregion Meta

		#region Enum
		// ===== Enum =====
		public enum Targetting
		{
			health,
			percentageHealth,
			debuffed,
			distance,
			proximity,
		}
		public enum BulletCurve
		{
			linear,
			slerp,
			ease,
		}
		public enum Type
		{
			None,
			Amethyst,
			Emerald,
			Diamond,
			Ruby,
			Opal,
			Sapphire,
			Peridot,
			Topaz,
			Lapis,
			Alexandrite,
			Agate,
			Spinel,
			Beryl,
			Morganite,
			Onyx,
			Garnet,
			Citrine,
			Aquamarine,
			Mozanite,
			Moonstone,
			Tanzanite,
			Jasper,
			Jade,
			Tourmaline,
			Pyrite,
			Oricahclum,
			Socerorium,
			Philosophine,
			Seraphium,
			DarkCrystallum,
			Glass,
			Urite,
			SeaGlass,
			Amberina,
		}
		public enum Colour
		{


		}
		public enum Rank
		{

		}
		public enum Tag
		{
			Turret,
			Trap,

		}

		#endregion Enum
	}
}