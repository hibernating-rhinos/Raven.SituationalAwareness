using System;
using System.Collections.Generic;

namespace Raven.SituationalAwareness
{
	public class NodeMetadata : EventArgs
	{
		public TopologyChangeType ChangeType { get; set; }
		public IDictionary<string, string> Metadata { get; set; }
		public Uri Uri { get; set; }
		public string ClusterName { get; set; }
	}
}