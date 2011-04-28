using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness.Paxos
{
	public class Agent
	{
		private readonly IDictionary<Type, Action<object>> registrations = new Dictionary<Type, Action<object>>();
		private Presence presence;

		public Agent(Presence presence)
		{
			this.presence = presence;
		}

		[DebuggerNonUserCode]
		public void DispatchMessage(PaxosMessage result)
		{
			Action<object> value;
			if (registrations.TryGetValue(result.GetType(), out value) == false) 
				return;
			value(result);
		}

		[DebuggerNonUserCode]
		public void Register<T>(Action<T> action)
		{
			registrations[typeof(T)] = new InvokerWithoutDebugger<T>(action).Invoke;
		}

		protected Task SendAsync(Uri destination, PaxosMessage msg)
		{
			var channel = presence.GetNodeStateServiceAsync(destination);
			return Task.Factory.FromAsync(channel.BeginReceivePaxosMessage, channel.EndReceivePaxosMessage, msg, null)
				.ContinueWith(task =>
				{
					presence.CloseWcf(channel);
					if (task.Exception == null)
						return;
					// we accept the possibility of failure. 
					presence.Log("Could not communicate with {0}, because: {1}", new object[] { destination, task.Exception });
				});
		}


		#region Nested type: InvokerWithoutDebugger

		public class InvokerWithoutDebugger<T>
		{
			private readonly Action<T> action;

			public InvokerWithoutDebugger(Action<T> action)
			{
				this.action = action;
			}

			[DebuggerNonUserCode]
			public void Invoke(object obj)
			{
				action((T)obj);
			}
		}

		#endregion
	}
}