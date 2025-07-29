// Unused.

namespace TowerDefence.Skills
{
	// ===== Interfaces =====
	/*
		An IModification modifiers an Entity's Stat.

		It informs the Entity of the source of the modification
	*/
	public interface IModification
	{
		public ISource Source { get; }
		public IEntity Target { get; }
		public double Value { get; }
		public MathOperation Operation { get; }

		// Apply and Rollback logic should be handled in the handler
		// public void Apply(ref IEntity entity);
		// public void Rollback(ref IEntity entity); 
		public List<IEntityCondition> Conditions { get; }
		public ModificationType ModificationType { get; }
	}

	public enum ModificationType
	{
		Ship,
		StatBlock,
		World,
	}

	public interface IEntityCondition
	{
		public StatType Type { get; set; }
		public MathOperation Comparative { get; set; }
		public double Value { get; set; }
	}

	public interface IStatChanger : IModification
	{
		public StatType Type { get; set; }
	}

	public class StatChanger : IStatChanger
	{
		public StatType Type { get; set; }
		public ISource Source { get; set; }
		public IEntity Target { get; set; }
		public double Value { get; set; }
		public MathOperation Operation { get; set; }

		public List<IEntityCondition> Conditions { get; private set; }

		public ModificationType ModificationType { get; set; }

		public void Apply(ref IEntity entity)
		{
			entity.ApplyModification(this);
		}
	}

	/*
		An IDamager applies damage over time based on certain triggers.

		Current supported triggers are:
			1. OnHit
			2. Periodic
			3. OnTimeExpire,

		(FUTURE) supported triggers are:
			4. OnCriticalHit
			5. OnBuffApplied
			6. OnStackThis
	*/
	public interface IDamager : IModification
	{
		// In a way, its 'StatType' is CurrentHealth
		public double Damage { get; set; }
		public bool IsPeriodic { get; set; }
	}

	public class Damager : IDamager
	{
		public ISource Source { get; set; }
		public IEntity Target { get; set; }
		public double Value { get; set; }
		public MathOperation Operation { get; set; }
		public List<IEntityCondition> Conditions { get; private set; }
		public double Damage { get; set; }
		public bool IsPeriodic { get; set; }

		public ModificationType ModificationType { get; set; }

		public void Apply(ref IEntity entity)
		{
			entity.ApplyDamage(this);
		}
	}

	public interface IModifier
	{
		// Boolean checks
		public bool IsPositive { get; }
		public bool ForMonster { get; }
		public bool ForTower { get; }

		// Modifications
		public List<Effect> Effects { get; } // Conditions lie within
		public List<Skill> Passives { get; set; } // Conditions lie within
		public event Action<ISource> OnApplied;
	}

}