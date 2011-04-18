using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Raven.SituationaAwareness
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class NodeStateService : INodeStateService
	{
		private readonly Dictionary<string, string> metadata;

		public NodeStateService(IDictionary<string, string> metadata)
		{
			// state is readonly, and cannot be modified, so we copy it locally
			this.metadata = new Dictionary<string, string>(metadata);
		}

		public IDictionary<string, string> GetMetadata()
		{
			return metadata;
		}
	}
}