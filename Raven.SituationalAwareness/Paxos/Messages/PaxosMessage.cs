using System;
using System.Runtime.Serialization;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	[DataContract]
	public abstract class PaxosMessage
	{
		[DataMember]
		public Uri Originator { get; set; }
	}
}