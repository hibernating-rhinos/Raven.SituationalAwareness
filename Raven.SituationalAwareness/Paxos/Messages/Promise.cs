using Raven.SituationalAwareness.Paxos.Commands;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	public class Promise : PaxosMessage
	{
		public ICommandState AcceptedValue { get; set; }
		public int ProposalNumber { get; set; }
		public int BallotNumber { get; set; }

		public override string ToString()
		{
			return string.Format("AcceptedValue: {0}, ProposalNumber: {1}, BallotNumber: {2}", AcceptedValue, ProposalNumber,
			                     BallotNumber);
		}
	}
}