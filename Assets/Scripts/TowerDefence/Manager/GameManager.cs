using System;
using TowerDefence.Context;
using UnityEngine;

namespace TowerDefence.Manager
{
	public class GameManager : MonoBehaviour
	{
		#region Preamble
		public static GameManager Instance { get; private set; }
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}
		}

		public GameContext GameContext { get; private set; }

		#endregion Preamble

		#region Events
		public event Action<TriggerContext> OnMonsterDeath = delegate { };
		public event Action<TriggerContext> OnTowerDeath = delegate { };
		public event Action<TriggerContext> OnMonsterReached = delegate { };
		public event Action<TriggerContext> OnNumberOfMonstersChanged = delegate { };

		#endregion Events
	}
}