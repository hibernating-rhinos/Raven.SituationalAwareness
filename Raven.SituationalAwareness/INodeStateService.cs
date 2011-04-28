using System;
using System.Net.Security;
using System.ServiceModel;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness
{
	[ServiceKnownType("GetKnownTypes", typeof(PaxosAndCommandsKnownTypes))]
	[ServiceContract(ProtectionLevel = ProtectionLevel.None, Name = "Raven.SituationaAwareness.INodeStateService", Namespace = "http://hibernatingrhinos.com/raven/situational.awareness/2011/04")]
	public interface INodeStateService
	{
		[OperationContract(ProtectionLevel = ProtectionLevel.None)]
		RemoteNodeMetadata GetMetadata(string clusterName, Uri[] knownSiblings);

		[OperationContract(ProtectionLevel = ProtectionLevel.None, IsOneWay = true)]
		void ReceivePaxosMessage(PaxosMessage message);
	}
}