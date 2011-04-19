using System;
using System.Collections.Concurrent;
using Raven.SituationaAwareness.Paxos.Commands;
using Raven.SituationaAwareness.Paxos.Messages;

namespace Raven.SituationaAwareness.Paxos
{
	public class Learner : Agent
	{
		private readonly Uri[] acceptors;
		private readonly ConcurrentDictionary<int, LearnerState> learnerState = new ConcurrentDictionary<int, LearnerState>();
		public event Action<ICommandState> OnAcceptedValue = delegate { };
 
		public Learner(Presence presence, Uri[] acceptors)
			: base(presence)
		{
			this.acceptors = acceptors;
			Register<Accepted>(OnAccepted);
		}


		private void OnAccepted(Accepted accepted)
		{
			LearnerState state = learnerState.GetOrAdd(accepted.ProposalNumber, new LearnerState
			{
				BallotNumber = accepted.BallotNumber,
				NumberOfAccepts = 1,
				ProposalNumber = accepted.ProposalNumber
			});
			if (state.BallotNumber < accepted.BallotNumber)
			{
				return;
			}
			if (state.Accepted)  // duplicate
				return;

			state.NumberOfAccepts += 1;
			if (state.NumberOfAccepts < acceptors.Length / 2)
				return;
			OnAcceptedValue(accepted.Value);
		}


		#region Nested type: LearnerState

		private class LearnerState
		{
			public int ProposalNumber { get; set; }
			public bool Accepted { get; set; }
			public int BallotNumber { get; set; }
			public int NumberOfAccepts { get; set; }
		}

		#endregion
	}
}