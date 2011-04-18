using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace Raven.SituationaAwareness
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
	public class NodeStateService : INodeStateService
	{
		private readonly string clusterName;
		private readonly Action<Uri> onConnect;
		private readonly Dictionary<string, string> metadata;

		public NodeStateService(string clusterName, IDictionary<string, string> metadata, Action<Uri> onConnect)
		{
			this.clusterName = clusterName;
			this.onConnect = onConnect;
			// state is readonly, and cannot be modified, so we copy it locally
			this.metadata = new Dictionary<string, string>(metadata);
		}

		public RemoteNodeMetadata GetMetadata(string remoteClusterName, Uri source)
		{
			if (remoteClusterName == clusterName)
				onConnect(source);
			return new RemoteNodeMetadata
			{
				ClusterName = clusterName,
				Metadata = metadata
			};
		}
	}
}