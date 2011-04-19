using System;
using System.Collections.Generic;

namespace Raven.SituationaAwareness.Paxos.Commands
{
	public interface ICommandState : IComparable<ICommandState>
	{
	}
}