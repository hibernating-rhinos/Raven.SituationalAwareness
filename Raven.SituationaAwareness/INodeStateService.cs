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
		IDictionary<string, string> GetMetadata();
	}

	[ServiceContract(ProtectionLevel = ProtectionLevel.None, Name = "Raven.SituationaAwareness.INodeStateService", Namespace = "http://hibernatingrhinos.com/raven/situational.awareness/2011/04")]
	public interface INodeStateServiceAsync
	{
		[OperationContract(ProtectionLevel = ProtectionLevel.None, AsyncPattern = true)]
		IAsyncResult BeginGetMetadata(AsyncCallback callback,object state);

		IDictionary<string, string> EndGetMetadata(IAsyncResult ar);
	}
}