using System;

namespace TowerDefence.Entity.Skills.Buffs
{
	public interface IExpirable
	{
		/// <summary>
		/// Duration of the effect in seconds.
		/// </summary>
		public float Duration { get; }

		/// <summary>
		/// Current time of the effect in seconds.
		/// </summary>
		public float Time { get; }
		//
		public void Init();
		public void Tick(float time);
		public void Expire();
		public bool IsExpired();
		//
		public event Action<IExpirable> OnExpired;
	}

}