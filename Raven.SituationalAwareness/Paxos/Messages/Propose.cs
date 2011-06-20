using System.Runtime.Serialization;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	[DataContract]
	public class Propose : PaxosMessage
	{
		[DataMember]
		public int ProposalNumber { get; set; }
		[DataMember]
		public int BallotNumber { get; set; }

		public override string ToString()
		{
			return string.Format("ProposalNumber: {0}, BallotNumber: {1}", ProposalNumber, BallotNumber);
		}
	}
}