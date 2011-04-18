using System;
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel;

namespace Raven.SituationaAwareness
{
	[ServiceContract(ProtectionLevel = ProtectionLevel.None, Name = "Raven.SituationaAwareness.INodeStateService", Namespace = "http://hibernatingrhinos.com/raven/situational.awareness/2011/04")]
	public interface INodeStateService
	{
		[OperationContract(ProtectionLevel = ProtectionLevel.None)]
		RemoteNodeMetadata GetMetadata(string clusterName, Uri source);
	}

	[ServiceContract(ProtectionLevel = ProtectionLevel.None, Name = "Raven.SituationaAwareness.INodeStateService", Namespace = "http://hibernatingrhinos.com/raven/situational.awareness/2011/04")]
	public interface INodeStateServiceAsync
	{
		[OperationContract(ProtectionLevel = ProtectionLevel.None, AsyncPattern = true)]
		IAsyncResult BeginGetMetadata(string clusterName, Uri source, AsyncCallback callback, object state);

		 RemoteNodeMetadata EndGetMetadata(IAsyncResult ar);
	}

	public class RemoteNodeMetadata
	{
		public IDictionary<string, string> Metadata { get; set; }
		public string ClusterName { get; set; }
	}
}