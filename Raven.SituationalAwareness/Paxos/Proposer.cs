using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Raven.SituationalAwareness.Paxos.Commands;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness.Paxos
{
	public class Proposer : Agent
	{
		public const int BallotStep = 1000;
		private readonly Acceptor myAcceptor;
		private readonly Uri orignatorUri;
		private readonly Uri[] allAcceptors;
		private readonly ConcurrentDictionary<int, ProposalState> proposalsState = new ConcurrentDictionary<int, ProposalState>();
		private int proposalNumber;
		private readonly int ballotBase;

		public Proposer(Presence presence, Acceptor myAcceptor, Uri orignatorUri, Uri[] allAcceptors) : base(presence)
		{
			this.myAcceptor = myAcceptor;
			this.orignatorUri = orignatorUri;
			this.allAcceptors = allAcceptors;

			ballotBase = base.GetHashCode()%25; // effectively a random choice

			Register<Promise>(OnPromise);
			Register<ProposalSubsumed>(OnProposalSubsumed);
			Register<Accepted>(OnAccepted);
		}


		private void OnProposalSubsumed(ProposalSubsumed proposalSubsumed)
		{
			ProposalState state;
			if (proposalsState.TryGetValue(proposalSubsumed.ProposalNumber, out state) == false)
				return; // delayed / duplicate message, probably
			if (state.BallotNumber > proposalSubsumed.BallotNumber)
				return; // probably already suggested higher number
			state.BallotNumber += BallotStep;
			state.NumberOfPromises = 0;
			state.NumberOfAccepts = 0;
			state.LastMessage = DateTime.Now;
			state.QuorumReached = false;
			foreach (var uri in allAcceptors)
			{
				SendAsync(uri, new Propose
				{
					Originator = orignatorUri,
					ProposalNumber = state.ProposalNumber,
					BallotNumber = state.BallotNumber
				});
			}
		}

		public void StartProposing(ICommandState cmd)
		{
			ProposalState state;
			do
			{
				state = new ProposalState
				{
					InitialValue = cmd,
					BallotNumber = BallotStep + ballotBase,
					ProposalNumber = GenerateNextProposalNumber(),
					LastMessage = DateTime.Now
				};
			} while (proposalsState.TryAdd(state.ProposalNumber, state) == false);

			foreach (var uri in allAcceptors)
			{
				SendAsync(uri, new Propose
				{
					BallotNumber = state.BallotNumber,
					Originator = orignatorUri,
					ProposalNumber = state.ProposalNumber
				});
			}
		}

		
		private void OnAccepted(Accepted accepted)
		{
			ProposalState state;
			if (proposalsState.TryGetValue(accepted.ProposalNumber, out state) == false)
				return;
			state.NumberOfAccepts += 1;
			if (state.NumberOfAccepts <= allAcceptors.Length / 2)
				return;
			ProposalState _;
			proposalsState.TryRemove(state.ProposalNumber, out _);
		}

		private void OnPromise(Promise promise)
		{
			ProposalState state;
			if (proposalsState.TryGetValue(promise.ProposalNumber, out state) == false)
				return; // delayed / duplicate message, probably

			if (state.BallotNumber != promise.BallotNumber)
				return;

			if (promise.AcceptedValue != null)
				state.ValuesToBeChoosen.Add(promise.AcceptedValue);

			state.NumberOfPromises++;
			state.LastMessage = DateTime.Now;

			if (state.QuorumReached)
				return;
			if (state.NumberOfPromises <= allAcceptors.Length / 2)
				return;
			state.QuorumReached = true;
			state.ChosenValue = state.ValuesToBeChoosen.FirstOrDefault() ?? state.InitialValue;
			foreach (var uri in allAcceptors)
			{
				SendAsync(uri, new Accept
				{
					Originator = orignatorUri,
					ProposalNumber = state.ProposalNumber,
					Value = state.ChosenValue,
					BallotNumber = state.BallotNumber
				});
			}

			// if we selected a different value
			if (state.ChosenValue.CompareTo(state.InitialValue) == 0)
				return;

			//restart the whole things
			ProposalState _;
			proposalsState.TryRemove(state.ProposalNumber, out _);
			StartProposing(state.InitialValue);
		}


		private int GenerateNextProposalNumber()
		{
			if (myAcceptor.AcceptedProposalNumber > proposalNumber)
				proposalNumber = (myAcceptor.AcceptedProposalNumber + 1);
			else
				proposalNumber = (proposalNumber + 1);

			return proposalNumber;
		}
		#region Nested type: ProposalState

		private class ProposalState
		{
			public ProposalState()
			{
				ValuesToBeChoosen = new List<ICommandState>();
			}

			public bool QuorumReached { get; set; }
			public int ProposalNumber { get; set; }
			public int BallotNumber { get; set; }
			public int NumberOfPromises { get; set; }
			public List<ICommandState> ValuesToBeChoosen { get; set; }
			public ICommandState InitialValue { get; set; }
			public DateTime LastMessage { get; set; }

			public ICommandState ChosenValue { get; set; }

			public int NumberOfAccepts { get; set; }
		}

		#endregion
	}
}