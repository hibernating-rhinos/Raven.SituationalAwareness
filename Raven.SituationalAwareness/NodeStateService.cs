using System;
using System.Collections.Generic;
using System.ServiceModel;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
	public class NodeStateService : INodeStateService
	{
		private readonly string clusterName;
		private readonly Action<Uri[]> onConnect;
		private readonly Action<PaxosMessage> onPaxosMessage;
		private readonly Dictionary<string, string> metadata;

		public NodeStateService(string clusterName, IDictionary<string, string> metadata, Action<Uri[]> onConnect, Action<PaxosMessage> onPaxosMessage)
		{
			this.clusterName = clusterName;
			this.onConnect = onConnect;
			this.onPaxosMessage = onPaxosMessage;
			// state is readonly, and cannot be modified, so we copy it locally
			this.metadata = new Dictionary<string, string>(metadata);
		}

		public RemoteNodeMetadata GetMetadata(string remoteClusterName, Uri[] knownSiblings)
		{
			if (remoteClusterName == clusterName)
				onConnect(knownSiblings);
			return new RemoteNodeMetadata
			{
				ClusterName = clusterName,
				Metadata = metadata
			};
		}

		public void ReceivePaxosMessage(PaxosMessage message)
		{
			onPaxosMessage(message);
		}
	}
}