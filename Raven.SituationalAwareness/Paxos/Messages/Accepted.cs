using System.Runtime.Serialization;
using Raven.SituationalAwareness.Paxos.Commands;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	[DataContract]
	public class Accepted : PaxosMessage
	{
		[DataMember]
		public int ProposalNumber { get; set; }
		[DataMember]
		public int BallotNumber { get; set; }
		[DataMember]
		public ICommandState Value { get; set; }

		public override string ToString()
		{
			return string.Format("ProposalNumber: {0}, BallotNumber: {1}, Value: {2}", ProposalNumber, BallotNumber, Value);
		}
	}
}