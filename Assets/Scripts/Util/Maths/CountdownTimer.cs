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
		public Action<CountdownTimer> OnRing;

		public CountdownTimer(float countdownTime, bool periodic = false)
		{
			this.countdownTime = countdownTime;
			this.Periodic = periodic;
		}

		public CountdownTimer(float countdownTime, Action onRing)
		{
			this.countdownTime = countdownTime;
			OnRing += (timer) => onRing();
		}

		public CountdownTimer(float countdownTime, Action<CountdownTimer> onRing)
		{
			this.countdownTime = countdownTime;
			OnRing += onRing;
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
			OnRing.Invoke(this);
		}

		/// <summary>
		/// Updates the timer. Call this method in your game loop with the time elapsed since
		/// the last update (in seconds).
		/// </summary>
		public void Tick(float time)
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
				OnRing.Invoke(this);

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
		public event Action<CountdownCollection, CountdownTimer> OnRing;

		public CountdownCollection(List<float> countdownTimes)
		{
			foreach (float countdownTime in countdownTimes)
			{
				AddTimer(countdownTime);
			}
		}

		public CountdownCollection SetPeriodic()
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
			timers[timers.Count - 1].OnRing += (CountdownTimer timer) => OnRing.Invoke(this, timer);
		}

		public void Tick(float time)
		{
			foreach (var timer in timers)
			{
				timer.Tick(time);
			}
		}

		public void Reset(float time)
		{
			foreach (var timer in timers)
			{
				timer.Reset();
			}
		}
		#endregion Timer
	}
}