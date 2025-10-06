using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Attack.Damage;
using TowerDefence.Entity.Resources;
using TowerDefence.Entity.Skills;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Token;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Context
{
	public interface ITriggerContext : IContext
	{
		// Source
		TriggerType TriggerType { get; }
		IEntity Entity { get; }
		ISource Source { get; }

		// Target
		List<IEntity> Targets { get; }
		IEntity Target { get; }
		IStat Stat { get; }
		IRegenerable Regenerable { get; }
		IDepletable Depletable { get; }
		IExpirable Expirable { get; }
		IResource Resource { get; }
		IElement Element { get; }

		// Token
		IToken Token { get; }
		IToken NewToken { get; }

		// Property
		float Period { get; }
		ddouble Value { get; }
		IDamage Damage { get; }
		IStatMod Mod { get; }

	}

	public class TriggerContext : ITriggerContext
	{
		// Source
		public TriggerType TriggerType { get; set; }

		public IEntity Entity { get; set; }

		public ISource Source { get; set; }

		// Target

		public List<IEntity> Targets { get; set; }

		public IEntity Target { get; set; }

		public IStat Stat { get; set; }

		public IRegenerable Regenerable { get; set; }

		public IDepletable Depletable { get; set; }

		public IExpirable Expirable { get; set; }

		public IResource Resource { get; set; }

		public IElement Element { get; set; }

		// Token
		public IToken Token { get; set; }
		public IToken NewToken { get; set; }


		// Property

		public float Period { get; set; }

		public ddouble Value { get; set; }

		public IDamage Damage { get; set; }

		public IStatMod Mod { get; set; }
	}
}

// public interface IPeriodicTriggerContext : ITriggerContext
// {
// 	float Period { get; }
// 	// TriggerType TriggerType { get; } = TriggerType.Periodic;
// }

// public interface IEnterRangeTriggerContext : ITriggerContext
// {
// 	IEntity Target { get; }
// 	// TriggerType TriggerType { get; } = TriggerType.EnterRange;
// }

// public interface IAttackTriggerContext : ITriggerContext
// {
// 	IEntity Target { get; }
// 	// TriggerType TriggerType { get; } = TriggerType.Attack;
// }

// public interface IMultiAttackTriggerContext : ITriggerContext
// {
// 	List<IEntity> Target { get; }
// 	// TriggerType TriggerType { get; } = TriggerType.MultiAttack;
// }