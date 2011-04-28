using Raven.SituationalAwareness.Paxos.Commands;

namespace Raven.SituationalAwareness.Paxos.Messages
{
	public class Accept : PaxosMessage
	{
		public int ProposalNumber { get; set; }
		public int BallotNumber { get; set; }
		public ICommandState Value { get; set; }

		public override string ToString()
		{
			return string.Format("ProposalNumber: {0}, BallotNumber: {1}, Value: {2}", ProposalNumber, BallotNumber, Value);
		}
	}
}