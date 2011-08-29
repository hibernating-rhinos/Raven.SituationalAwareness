using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Raven.SituationalAwareness.Discovery
{
	public class DiscoveryClient : IDisposable
	{
		private readonly byte[] buffer;
		private readonly UdpClient udpClient;
		private readonly IPEndPoint allHostsGroup;

		public DiscoveryClient(string clusterName, Uri uri)
		{
			buffer = Encoding.UTF8.GetBytes(clusterName + Environment.NewLine + uri);
			udpClient = new UdpClient()
			{
				ExclusiveAddressUse = false
			};
			allHostsGroup = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 12391);
		}

		public void PublishMyPresence()
		{
			Task.Factory.FromAsync<byte[], int, IPEndPoint, int>(udpClient.BeginSend, udpClient.EndSend, buffer, buffer.Length, allHostsGroup, null)
				.ContinueWith(task =>
				{
					var _ = task.Exception;
					// basically just ignoring this error
				});
		}

		public void Dispose()
		{
			udpClient.Close();
		}
	}
}