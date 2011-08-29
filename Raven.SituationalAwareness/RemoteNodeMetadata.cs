using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Raven.SituationalAwareness
{
	[DataContract]
	public class RemoteNodeMetadata
	{
		[DataMember]
		public IDictionary<string, string> Metadata { get; set; }
		[DataMember]
		public string ClusterName { get; set; }
	}
}