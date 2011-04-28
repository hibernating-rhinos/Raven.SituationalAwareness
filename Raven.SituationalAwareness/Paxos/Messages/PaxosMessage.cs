using System;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	public abstract class PaxosMessage
	{
		public Uri Originator { get; set; }
	}
}