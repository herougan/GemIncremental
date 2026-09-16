using UnityEngine;

namespace TowerDefence.Effects
{
	/// <summary>
	/// Self-destructs after Lifetime seconds - what a spawned-and-forgotten VFX instance (ProjectilePlan.
	/// ReleaseEffect/TrailEffect/HitEffect, all just `Instantiate`d with nothing tracking the result -
	/// see ProjectileController) needs so it doesn't leak forever. Generic, not Fx-specific in name only:
	/// attach to any prefab that should clean itself up a fixed time after spawning.
	/// </summary>
	public class FxAutoDestroy : MonoBehaviour
	{
		public float Lifetime = 1f;

		void Start() => Destroy(gameObject, Lifetime);
	}
}
