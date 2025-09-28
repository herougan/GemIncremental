namespace TowerDefence.Library
{
	public interface ILifecycle
	{
		/// <summary>
		/// Advance the object's state by one tick or time unit.
		/// </summary>
		public void Tick();

		/// <summary>
		/// Reset the object to its initial state.
		/// </summary>
		public void Reset();

		/// <summary>
		/// Dispose of the object, cleaning up any resources or references.
		/// </summary>
		public void Dispose();

		/// <summary>
		/// Initialize the object, setting up any necessary state or references.
		/// </summary>
		public void Init();
	}
}