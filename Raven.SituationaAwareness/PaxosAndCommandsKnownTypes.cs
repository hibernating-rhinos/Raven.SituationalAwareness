using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Raven.SituationaAwareness.Paxos.Commands;
using Raven.SituationaAwareness.Paxos.Messages;

namespace Raven.SituationaAwareness
{
	public class PaxosAndCommandsKnownTypes
	{
		internal static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
		{
			return typeof (PaxosAndCommandsKnownTypes).Assembly.GetTypes()
				.Where(x => x.Namespace == typeof (ICommandState).Namespace || x.Namespace == typeof (PaxosMessage).Namespace);

		}
	}
}