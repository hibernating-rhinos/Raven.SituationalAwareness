using System.Collections.Generic;

namespace Raven.SituationaAwareness
{
	public class RemoteNodeMetadata
	{
		public IDictionary<string, string> Metadata { get; set; }
		public string ClusterName { get; set; }
	}
}