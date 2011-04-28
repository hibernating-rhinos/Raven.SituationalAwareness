using System.Collections.Generic;

namespace Raven.SituationalAwareness
{
	public class RemoteNodeMetadata
	{
		public IDictionary<string, string> Metadata { get; set; }
		public string ClusterName { get; set; }
	}
}