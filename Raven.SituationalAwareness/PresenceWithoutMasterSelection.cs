using System;
using System.Collections.Generic;

namespace Raven.SituationalAwareness
{
	public class PresenceWithoutMasterSelection : Presence
	{
		public PresenceWithoutMasterSelection(string clusterName, IDictionary<string, string> nodeMetadata) : base(clusterName, nodeMetadata)
		{
		}

		public PresenceWithoutMasterSelection(string clusterName, IDictionary<string, string> nodeMetadata, TimeSpan heartbeat) : base(clusterName, nodeMetadata, heartbeat)
		{
		}

		protected override void OnTopologyChanged(object sender, NodeMetadata nodeMetadata)
		{
			// we don't handle master selection
		}
	}
}