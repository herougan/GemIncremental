using TowerDefence.Entity.Skills.Effects;

namespace TowerDefence.Entity.Skills
{
	/// <summary>
	/// Represents an interaction effect in the game. These interaction effects 
	/// are always present, and trigger when certain conditions are met.
	/// For example, water dousing fire, crit-ing a weakened enemy, or applying electrostatics to water from an electrical attack.
	/// (Interactions come from Elemental mixing, ...)
	/// </summary>
	public class Interaction : Effect
	{
		public InteractionType InteractionType { get; protected set; }
		public float Value { get; protected set; }

		public Interaction(InteractionType interactionType, float value)
		{
			InteractionType = interactionType;
			Value = value;
		}
	}

	public class ElementalInteraction : Interaction
	{
		public ElementalInteraction(InteractionType interactionType, float value) : base(interactionType, value)
		{
		}
	}

	public enum InteractionType
	{
		// Physical
		PlasmaSomethingSomething,
		ImpactBreaksArmour, // Impact breaking armour
		LaserBreaksShields,  // Laser breaking shields
							 // Elemental
		WaterDouseFire, // Water dousing fire
		CritWeakEnemy, // Critical hit on a weakened enemy
		ElectrostaticWater, // Applying electrostatics to water from an electrical attack
		FireBreaksIce, // Fire melting ice
		IceFreezesWater, // Ice freezing water
		FireIgnitesWood, // Fire igniting wood
	}
}
