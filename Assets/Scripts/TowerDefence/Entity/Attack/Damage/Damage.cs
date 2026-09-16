using System.Collections.Generic;
using TowerDefence.Entity.Skills;
using TowerDefence.Stats;
using Util.Maths;

namespace TowerDefence.Entity.Attack.Damage
{
	public interface IDamage
	{
		StatType StatType { get; }
		ddouble Value { get; }
		IEntity Attacker { get; }
		IEntity Target { get; }
		ElementType Element { get; }
		// Physical is any untagged damage - there's no explicit Physical tag. Tags compose freely
		// (Magic Fire, Physical Ranged Fire, Armor Piercing Ice, ...), which is why this replaced the
		// earlier single DamageSchool enum (Physical/Magical) - that couldn't express more than one
		// dimension at once. See Tag.cs.
		List<Tag> Tags { get; }
		bool HasTag(Tag tag);
		ddouble FinalValue { get; }
		// Which Skill/Buff actually dealt this hit, if known - what a battle-analysis report (see
		// Util.Game.BattleTracker) attributes damage to. Populated from TriggerContext.Source, which
		// WrappedAction.Invoke fills in with the Skill it belongs to - see WrappedAction.cs.
		ISource Source { get; }
	}

	public class Damage : IDamage
	{
		public StatType StatType { get; private set; }
		public ddouble Value { get; private set; }
		public IEntity Attacker { get; private set; }
		public IEntity Target { get; private set; }
		// What element this hit deals, if any - read by Compendium (see DamageModifier.cs) to look up
		// type-matchup bonuses. None means "no element" (a plain physical hit), not "unset".
		public ElementType Element { get; private set; } = ElementType.None;
		public List<Tag> Tags { get; } = new List<Tag>();
		// What actually landed on Health after the full modifier tree (DamageCalculator) AND the
		// Nullifier/Shield absorb order - set once by Entity.ApplyDamage, 0 before then (and 0 after,
		// if Nullifier/Shield absorbed all of it). This, not Value, is "how much this hit actually
		// hurt" - what a reflect/mirror effect (see ReflectActionHandler) should read.
		public ddouble FinalValue { get; private set; }
		public ISource Source { get; private set; }

		public Damage(StatType statType, ddouble value, IEntity attacker)
		{
			this.StatType = statType;
			this.Value = value;
			this.Attacker = attacker;
		}

		public void SetTarget(IEntity target)
		{
			this.Target = target;
		}

		public void SetElement(ElementType element)
		{
			this.Element = element;
		}

		public void AddTag(Tag tag)
		{
			if (!Tags.Contains(tag)) Tags.Add(tag);
		}

		public bool HasTag(Tag tag) => Tags.Contains(tag);

		public void SetFinalValue(ddouble value)
		{
			this.FinalValue = value;
		}

		public void SetSource(ISource source)
		{
			this.Source = source;
		}
	}
}
