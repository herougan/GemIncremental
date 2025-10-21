using TowerDefence.Context;
using TowerDefence.Projectile;
using UnityEngine;

namespace TowerDefence.Entity.Skills.Effects.Types.Attack
{
	public class ProjectileActionHandler : ActionHandler
	{
		public void ApplyAction(GameContext context, TriggerContext triggerContext, IAction action)
		{
			// if (action is ProjectileAction pjAction)
			// {

			// }
			// GameObject projectile = EntityManager.Instance.SpawnProjectile(pjAction.Plan, triggerContext.GetSource(), triggerContext.GetTarget(), pjAction.ProjectileParams);

			// // Hook events
			// projectile.GetComponent<ProjectileController>().OnReach += (proj) =>
			// {
			// 	// Handle projectile reach event
			// 	// This is where you can trigger any additional effects or actions when the projectile reaches its target
			// 	// For example, you might want to apply damage or trigger an explosion effect
			// 	Debug.Log($"Projectile {proj.name} reached its target.");
			// };

			// Step 1: Create Projectile
			// (EntityManager creates projectile with pool) // Wrong, Entities wire themselves on creation. // Raw triggers done in entity itself
			// e.g. buff expire or on buff applied!!!
			// (EntityManager wires Entity with itself)
			// Step 2: Wire new external functions to Projectile's inner events

			// GameManager: 
			// 1. EntManager creates Entity
			// 2. Wires Entity with itself
			// 3. Check triggers
			// 4. 
		}

		public void ApplyAction(GameContext context, IAction action)
		{
			// This method is called to apply the action in the context of the game
			// You can implement any additional logic needed for applying the action here
			// LogManager.Instance.Log($"Applying action: {action.Type} with data: {action.Data}");
			// ApplyAction();
			// Spawn projectile

			// Wire event

			// ...
		}

		public void ApplyAction(GameContext context, Effect effect)
		{
			// This method is called to apply the effect in the context of the game
			// You can implement any additional logic needed for applying the effect here
			// LogManager.Instance.Log($"Applying effect with actions: {effect.Actions.Count}");
			foreach (IAction action in effect.Actions)
			{
				ApplyAction(context, action);
			}
		}

		public ProjectileActionHandler() : base()
		{
			Type = ActionType.Projectile;
		}
	}
}