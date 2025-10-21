using System.Collections.Generic;
using Util.Events;

namespace TowerDefence.Entity.Events
{
	public interface ICompositeEvents
	{
		List<WrappedAction> WrappedActions { get; }
	}

	public class CompositeEvents : ICompositeEvents
	{
		public List<WrappedAction> WrappedActions { get; } = new();
	}
}