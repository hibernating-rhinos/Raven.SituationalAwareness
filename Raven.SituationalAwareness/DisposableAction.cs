using System;

namespace Raven.SituationalAwareness
{
	public class DisposableAction : IDisposable
	{
		private readonly Action action;

		public DisposableAction(Action action)
		{
			this.action = action;
		}

		public void Dispose()
		{
			action();
		}
	}
}