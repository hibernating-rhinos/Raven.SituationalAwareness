using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Raven.SituationaAwareness.Paxos.Commands;
using Raven.SituationaAwareness.Paxos.Messages;

namespace Raven.SituationaAwareness.Paxos
{
	public class Acceptor : Agent
	{
		private readonly ConcurrentDictionary<int, AcceptState> acceptorState = new ConcurrentDictionary<int, AcceptState>();
		private readonly Uri originatorUri;
		private readonly Uri[] learners;

		public Acceptor(Presence presence, Uri originatorUri, Uri[] learners) : base(presence)
		{
			this.originatorUri = originatorUri;
			this.learners = learners;
			Register<Propose>(OnPropose);
			Register<Accept>(OnAccept);
		}

		public int AcceptedProposalNumber
		{
			get
			{
				return acceptorState.Where(x => x.Value.AcceptedValue != null)
					.Select(x => x.Key)
					.OrderByDescending(x => x)
					.FirstOrDefault();
			}
		}

		private void OnPropose(Propose propose)
		{
			AcceptState state;
			if (acceptorState.TryGetValue(propose.ProposalNumber, out state))
			{
				if (propose.BallotNumber <= state.BallotNumber)
				{
					SendAsync(propose.Originator, new ProposalSubsumed
					{
						Originator = originatorUri,
						BallotNumber = propose.BallotNumber,
						ProposalNumber = propose.ProposalNumber
					});
				}
				else
				{
					state.BallotNumber = propose.BallotNumber;
					SendAsync(propose.Originator, new Promise
					{
						Originator = originatorUri,
						AcceptedValue = state.AcceptedValue,
						BallotNumber = propose.BallotNumber,
						ProposalNumber = propose.ProposalNumber
					});
				}
			}
			else
			{
				acceptorState[propose.ProposalNumber] = new AcceptState
				{
					BallotNumber = propose.BallotNumber,
					ProposalNumber = propose.ProposalNumber
				};
				SendAsync(propose.Originator, new Promise
				{
					AcceptedValue = null,
					BallotNumber = propose.BallotNumber,
					ProposalNumber = propose.ProposalNumber,
					Originator = originatorUri
				});
			}
		}

		private void OnAccept(Accept accept)
		{
			AcceptState state;
			if (acceptorState.TryGetValue(accept.ProposalNumber, out state) == false)
				return; // trying to accept without a propsal?
			if (accept.BallotNumber < state.BallotNumber)
			{
				SendAsync(accept.Originator, new ProposalSubsumed
				{
					BallotNumber = state.BallotNumber,
					Originator = originatorUri,
					ProposalNumber = state.ProposalNumber
				});
				return;
			}
			state.AcceptedValue = accept.Value;
			state.BallotNumber = accept.BallotNumber;
			SendAsync(accept.Originator, new Accepted
			{
				Originator = originatorUri,
				ProposalNumber = accept.ProposalNumber,
				BallotNumber = accept.BallotNumber,
				Value = accept.Value
			});
			foreach (var learnerUri in learners)
			{
				SendAsync(learnerUri, new Accepted
				{
					Originator = learnerUri,
					ProposalNumber = accept.ProposalNumber,
					BallotNumber = accept.BallotNumber,
					Value = accept.Value
				});
			}
		}

		#region Nested type: AcceptState

		private class AcceptState
		{
			public int ProposalNumber { get; set; }
			public int BallotNumber { get; set; }
			public ICommandState AcceptedValue { get; set; }
		}

		#endregion
	}
}