using System.Runtime.Serialization;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	[DataContract]
	public class ProposalSubsumed : PaxosMessage
	{
		[DataMember]
		public int BallotNumber { get; set; }
		[DataMember]
		public int ProposalNumber { get; set; }

		public override string ToString()
		{
			return string.Format("BallotNumber: {0}, ProposalNumber: {1}", BallotNumber, ProposalNumber);
		}
	}
}