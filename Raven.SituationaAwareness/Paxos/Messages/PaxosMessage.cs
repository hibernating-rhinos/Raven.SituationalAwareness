using System;

namespace Raven.SituationaAwareness.Paxos.Messages
{
	public abstract class PaxosMessage
	{
		public Uri Originator { get; set; }
	}
}