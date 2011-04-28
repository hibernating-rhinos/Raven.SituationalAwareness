using System;

namespace Raven.SituationalAwareness.Paxos.Commands
{
	public interface ICommandState : IComparable<ICommandState>
	{
	}
}