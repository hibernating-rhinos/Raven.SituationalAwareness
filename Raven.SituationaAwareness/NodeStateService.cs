using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace Raven.SituationaAwareness
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
	public class NodeStateService : INodeStateService
	{
		private readonly Action<Uri> onConnect;
		private readonly Dictionary<string, string> metadata;

		public NodeStateService(IDictionary<string, string> metadata, Action<Uri> onConnect)
		{
			this.onConnect = onConnect;
			// state is readonly, and cannot be modified, so we copy it locally
			this.metadata = new Dictionary<string, string>(metadata);
		}

		public IDictionary<string, string> GetMetadata(Uri source)
		{
			onConnect(source);
			return metadata;
		}
	}
}