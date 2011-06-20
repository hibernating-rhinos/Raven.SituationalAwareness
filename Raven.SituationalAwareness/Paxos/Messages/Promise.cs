using System.Runtime.Serialization;
using Raven.SituationalAwareness.Paxos.Commands;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	[DataContract]
	public class Promise : PaxosMessage
	{
		[DataMember]
		public ICommandState AcceptedValue { get; set; }
		[DataMember]
		public int ProposalNumber { get; set; }
		[DataMember]
		public int BallotNumber { get; set; }

		public override string ToString()
		{
			return string.Format("AcceptedValue: {0}, ProposalNumber: {1}, BallotNumber: {2}", AcceptedValue, ProposalNumber,
			                     BallotNumber);
		}
	}
}