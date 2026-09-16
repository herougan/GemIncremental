using System;
using System.Collections.Generic;
using TowerDefence.Context;
using TowerDefence.Entity.Skills.ActionHandler;
using TowerDefence.Entity.Skills.Buffs;
using TowerDefence.Entity.Skills.Keywords;
using TowerDefence.Stats;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Util.Maths;

namespace TowerDefence.Entity.Skills.Effects
{
	// ===== Action =====
	public interface IAction
	{
		public ActionType ActionType { get; }
		public List<IBoost> Boosts { get; }
		public ddouble Value { get; }
		public void Recalculate(ddouble scale);
		public void ApplyAction(IEntity source, IEntity target);
	}

	/// <summary>
	/// The single, concrete Action data shape - one class, not a type hierarchy. Unity can't serialize a
	/// polymorphic List&lt;IAction&gt; without SerializeReference (fragile, no nice Inspector UI without
	/// a custom drawer), so instead Type is a plain discriminator and each ActionType's extra data gets
	/// its own field below ("pigeon-holes"), read only by that ActionType's own IActionHandler -
	/// everything else stays at its default.
	///
	/// Add a new pigeon-hole (and a new ActionType + IActionHandler) only for a genuinely new action
	/// *mechanism* - not for every spell/flavor. Fireball and Ice Bolt, for instance, are both
	/// ActionType.Projectile, differing only in which data they carry (ProjectilePlan, on-hit status),
	/// not in mechanism. Check in before adding a new ActionType/pigeon-hole.
	/// </summary>
	[Serializable]
	public class Action : IAction
	{
		public ActionType Type;
		public ddouble Value;
		public List<IBoost> Boosts = new List<IBoost>();

		// Author-set, not mechanically derived - free-form categorization (Ranged/Magic/AoE/...) on top
		// of Element below. Read by SkillPlan.GetTags().
		public List<Tag> Tags = new List<Tag>();

		// What element this action deals, if any - distinct from Tags: this drives the actual per-hit
		// elemental-matchup multiplier (Compendium, once a handler sets Damage.Element from this) and
		// SkillPlan.ToDescription's element emoji, where Tags.Elemental is just a display label.
		public ElementType Element;

		// ===== Pigeon-holes: one (or a couple) fields per ActionType, read only by that type's handler =====
		public StatType Stat; // ActionType.Stat
		public BuffPlan BuffToApply; // ActionType.ApplyBuff - see ApplyBuffActionHandler

		ActionType IAction.ActionType => Type;
		List<IBoost> IAction.Boosts => Boosts;
		ddouble IAction.Value => Value;

		public void Recalculate(ddouble scale)
		{
			Value *= scale;
		}

		/// <summary>
		/// Convenience path for direct application (e.g. Buff.ApplyAction checking its own Conditions
		/// before applying). Just builds a TriggerContext and routes through the same
		/// EffectController/IActionHandler dispatch the Trigger-driven path uses, so there's exactly one
		/// real implementation per ActionType, not two.
		/// </summary>
		public void ApplyAction(IEntity source, IEntity target)
		{
			EffectController.ApplyAction(new TriggerContext { Entity = source, Target = target }, source, this);
		}
	}

	public enum ActionType
	{
		// Property change
		Stat,
		Token,
		Damage, // % and flat
		Reflect, // deals a % (read from the acting entity's Reflect/SpellThorns-style stat) of trigger.Damage.FinalValue back at trigger.Target - see ReflectActionHandler
		Revenge, // deals Value% of Entity.DamageReceivedLog's total back at *every* logged attacker, then clears the log - see RevengeActionHandler
		Status,
		// Physics
		Move,
		Teleport,
		TimeSpeed,
		Kinematics,
		// Spawn
		Projectile,
		TimedEffect,
		Spawn,
		// Chain
		ChainEffect,
		ApplyBuff,
	}


	[CustomPropertyDrawer(typeof(Action))]
	public class EffectDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			// Create property container element.
			var container = new VisualElement();

			// TODO: show/hide the relevant pigeon-hole field(s) based on Type, once there's more than one.
			var typeField = new PropertyField(property.FindPropertyRelative("Type"));
			var valueField = new PropertyField(property.FindPropertyRelative("Value"));

			// Add fields to the container.
			container.Add(typeField);
			container.Add(valueField);

			return container;
		}
	}

}
