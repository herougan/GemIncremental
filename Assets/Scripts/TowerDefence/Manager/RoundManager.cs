using UnityEngine;

namespace TowerDefence.Manager
{
	/// <summary>
	/// Owns the timing GameManager sits on top of: how long to wait before the next wave begins, and -
	/// separately - how much longer a respite between whole Rounds (20 Waves each, see WorldProgress)
	/// should be. Watches EntityWaveManager's counts rather than owning any monster bookkeeping itself:
	/// a wave is "defeated" once nothing's left to spawn AND nothing spawned is still alive.
	///
	/// State machine is deliberately just two states - waiting for the current wave to clear, or
	/// counting down to the next one - not a general-purpose FSM (see Entity.EntityState for that
	/// pattern elsewhere); this doesn't need more than two.
	/// </summary>
	public class RoundManager : MonoBehaviour
	{
		public static RoundManager Instance { get; private set; }

		[Header("Timing")]
		public float SecondsBeforeNextWave = 5f;
		public float SecondsRespiteBetweenRounds = 30f;

		bool waitingForWaveClear = true;
		float countdown;

		void Awake()
		{
			if (Instance == null) Instance = this;
			else if (Instance != this) Destroy(this);
		}

		void Update()
		{
			Stages.EntityWaveManager waveManager = Stages.EntityWaveManager.Instance;
			if (waveManager == null || GameManager.Instance == null) return;

			if (waitingForWaveClear)
			{
				if (waveManager.MonstersRemainingToSpawn > 0 || waveManager.MonstersAlive > 0) return;

				// Wave's clear - decide which countdown to start. WorldProgress.Wave hasn't advanced yet
				// (StartNextWave does that after spawning), so "was this the round's last wave" is just
				// checking it against WavesPerRound - 1 directly.
				bool wasLastWaveOfRound = GameManager.Instance.WorldProgress.Wave >= Stages.WorldProgress.WavesPerRound - 1;
				countdown = wasLastWaveOfRound ? SecondsRespiteBetweenRounds : SecondsBeforeNextWave;
				waitingForWaveClear = false;
				return;
			}

			countdown -= Time.deltaTime;
			if (countdown > 0) return;

			waitingForWaveClear = true;
			GameManager.Instance.StartNextWave();
		}
	}
}
