using System.Collections.Generic;
using TowerDefence.Entity;
using TowerDefence.Entity.Monster;
using UnityEngine;

namespace TowerDefence.Stages
{
	[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 1)]
	public class WaveData : ScriptableObject
	{
		// public SerialisableGuid Guid; // Unique identifier for this wave data
		public MonsterPlan monster; // The type of monster to spawn
		public int quantity; // Number of monsters to spawn
		public float period; // Time between spawns
		public float offset; // Initial delay before spawning starts
		public List<Tag> tags; // Additional tags for the monsters

		// Constructor
		public WaveData(MonsterPlan monster, int quantity, float period, float offset, List<Tag> tags = null)
		{
			this.monster = monster;
			this.quantity = quantity;
			this.period = period;
			this.offset = offset;
			this.tags = tags;
		}
	}
}