using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Raven.SituationalAwareness.Paxos.Commands;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness
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