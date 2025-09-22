using System;
using System.Collections.Generic;

namespace Util.Maths
{
	public class CountdownTimer
	{
		#region Preamble

		// Timer properties
		public float countdownTime; // Time in seconds
		private float currentTime;
		public bool Periodic = false;
		bool fired = false;
		bool started = false;
		bool paused = false;


		// Timer events
		public event Action TimerComplete = delegate { };

		public CountdownTimer(float countdownTime)
		{
			this.countdownTime = countdownTime;
		}

		public CountdownTimer(float countdownTime, Action timerComplete)
		{
			this.countdownTime = countdownTime;
			TimerComplete += timerComplete;
		}

		public CountdownTimer SetPeriodic()
		{
			Periodic = true;
			return this;
		}

		#endregion Preamble

		#region Timer

		/// <summary>
		/// Starts the timer. The timer will now count down when Update is called.
		/// </summary>
		public void Start()
		{
			currentTime = countdownTime;
			started = true;
		}

		public void Reset()
		{
			currentTime = countdownTime;
			fired = false;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Resume()
		{
			paused = false;
		}

		public void Complete()
		{
			currentTime = 0;
			fired = true;
			started = false;
		}

		public void ForceInvoke()
		{
			if (Periodic) Reset();
			else Complete();
			TimerComplete.Invoke();
		}

		public void Update(float time)
		{
			if (!started) return; // If not started, do nothing
			if (!Periodic && fired) return; // If not periodic and already fired, do nothing
			if (paused) return; // If paused, do nothing

			// Update the timer
			if (currentTime > 0)
			{
				currentTime -= time;
			}
			else if (currentTime <= 0)
			{
				// Invoke event
				TimerComplete.Invoke();

				// Reset the timer if periodic
				if (Periodic) Reset();
				else Complete();

			}
		}

		#endregion Timer
	}

	public class CountdownCollection
	{
		#region Preamble
		public List<CountdownTimer> timers = new List<CountdownTimer>();
		public event Action<int> TimerComplete = delegate { };
		private int rung;

		public TimerList(List<float> countdownTimes)
		{
			foreach (float countdownTime in countdownTimes)
			{
				AddTimer(countdownTime);
			}
		}

		public TimerList SetPeriodic()
		{
			foreach (var timer in timers)
			{
				timer.SetPeriodic();
			}
			return this;
		}

		#endregion Preamble

		#region Timer
		public void AddTimer(float countdownTime)
		{
			timers.Add(new CountdownTimer(countdownTime));
			timers[timers.Count - 1].TimerComplete += OnRing;
		}

		public void Update(float time)
		{
			foreach (var timer in timers)
			{
				timer.Update(time);
			}
		}

		public void Reset(float time)
		{
			foreach (var timer in timers)
			{
				timer.Reset();
			}
		}

		void OnRing()
		{
			TimerComplete.Invoke(rung);
		}
		#endregion Timer
	}
}