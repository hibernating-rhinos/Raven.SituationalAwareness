using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Raven.SituationalAwareness.Discovery
{
	public class DiscoveryHost : IDisposable
	{
		private Socket socket;
		readonly byte[] buffer = new byte[1024*4];

		public void Start()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
								   new MulticastOption(IPAddress.Parse("224.0.0.1"))); // all hosts group

			socket.Bind(new IPEndPoint(IPAddress.Any, 12391));
			StartListening();
		}

		private void StartListening()
		{
			var socketAsyncEventArgs = new SocketAsyncEventArgs
			{
				RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0),
			};
			socketAsyncEventArgs.Completed += Completed;
			socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);

			bool startedAsync;
			try
			{
				startedAsync = socket.ReceiveFromAsync(socketAsyncEventArgs);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			if(startedAsync == false)
				Completed(this, socketAsyncEventArgs);
		}

		private void Completed(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
		{
			using(socketAsyncEventArgs)
			{
				try
				{
					using (var stream = new MemoryStream(socketAsyncEventArgs.Buffer, 0, socketAsyncEventArgs.BytesTransferred))
					using (var streamReader = new StreamReader(stream))
					{
						var clientDiscovered = new ClientDiscoveredEventArgs
						{
							ClusterName = streamReader.ReadLine()
						};

						var uri = streamReader.ReadLine();

						Uri result;
						if (Uri.TryCreate(uri, UriKind.Absolute, out result))
						{
							clientDiscovered.Uri = result;
							InvokeClientDiscovered(clientDiscovered);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				StartListening();
			}
		}

		public event EventHandler<ClientDiscoveredEventArgs> ClientDiscovered;

		private void InvokeClientDiscovered(ClientDiscoveredEventArgs e)
		{
			EventHandler<ClientDiscoveredEventArgs> handler = ClientDiscovered;
			if (handler != null) handler(this, e);
		}

		public class ClientDiscoveredEventArgs : EventArgs
		{
			public string ClusterName { get; set; }
			public Uri Uri { get; set; }
		}

		public void Dispose()
		{
			if (socket != null)
				socket.Dispose();
		}
	}
}