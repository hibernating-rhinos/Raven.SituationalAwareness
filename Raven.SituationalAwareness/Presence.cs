using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Raven.SituationalAwareness.Paxos;
using Raven.SituationalAwareness.Paxos.Commands;
using Raven.SituationalAwareness.Paxos.Messages;

namespace Raven.SituationalAwareness
{
	public class Presence : IDisposable
	{
		private readonly string clusterName;
		private readonly TimeSpan heartbeat;
		private Timer timer;
		private ServiceHost serviceHost;
	    private readonly IDictionary<string, string> nodeMetadata;
		private Uri myAddress;
		private readonly ConcurrentDictionary<Uri, IDictionary<string, string>> topologyState = new ConcurrentDictionary<Uri, IDictionary<string, string>>();

		private Agent[] agents = new Agent[0];

		public Uri MasterSelectedByQuorum { get; private set; }

		public Uri CurrentMaster
		{
			get
			{
				// if there is no master, we are the master, but we make sure
				// that you can discover that we weren't selected by the quorom
				return MasterSelectedByQuorum ?? myAddress;
			}
		}

		public Action<string, object[]> Log { get; set; }

		public Uri Address
		{
			get
			{
				return myAddress;
			}
		}

		public IEnumerable<NodeMetadata> Topology
		{
			get
			{
				return topologyState.Select(x => new NodeMetadata
				{
					ChangeType = TopologyChangeType.None,
					ClusterName = clusterName,
					Metadata = x.Value,
					Uri = x.Key
				});
			}
		}

		public event EventHandler<NodeMetadata> TopologyChanged = delegate { };

		public Presence(string clusterName, IDictionary<string, string> nodeMetadata)
			: this(clusterName, nodeMetadata, TimeSpan.FromMinutes(5))
		{
		}

		public Presence(string clusterName, IDictionary<string, string> nodeMetadata, TimeSpan heartbeat)
		{
			TopologyChanged = SelectNewMasterOnTopologyChanged;
			Log = (s, objects) => { };// don't log
			this.clusterName = clusterName;
			this.heartbeat = heartbeat;
		    this.nodeMetadata = nodeMetadata;

			RefreshServiceHost();
		}

		protected virtual void SelectNewMasterOnTopologyChanged(object sender, NodeMetadata nodeMetadata)
		{
			if (nodeMetadata.ChangeType == TopologyChangeType.MasterSelected)
				return;//nothing to do here

			MasterSelectedByQuorum = null; // Who is the master is in doubt, because the topology changed

			var others = topologyState.Keys.ToArray();
			var acceptor = new Acceptor(this, myAddress, others);
			var proposer = new Proposer(this, acceptor, myAddress, others);
			var learner = new Learner(this, others);
			learner.OnAcceptedValue += LearnerOnOnAcceptedValue;
			agents = new Agent[] { acceptor, proposer, learner };

			proposer.StartProposing(new SwitchMasterCommand
			{
				NewMaster = others.OrderBy(x => x.AbsoluteUri).First()
			});
		}

		private void LearnerOnOnAcceptedValue(ICommandState commandState)
		{
			var switchMasterCommand = commandState as SwitchMasterCommand;
			if (switchMasterCommand == null)
				return;
			if (switchMasterCommand.NewMaster == MasterSelectedByQuorum)
				return; // nothing changed

			IDictionary<string, string> value;
			lock (this)
			{
				if (switchMasterCommand.NewMaster == MasterSelectedByQuorum)
					return; // nothing changed


				if (topologyState.TryGetValue(switchMasterCommand.NewMaster, out value) == false)
				{
					// we got a master that we don't know of, let's discover the master
					// which will initiate another master selection round
					FindNewEndpointMetadata(switchMasterCommand.NewMaster);
					return;
				}
				MasterSelectedByQuorum = switchMasterCommand.NewMaster;
			}

			TopologyChanged(this, new NodeMetadata
			{
				ChangeType = TopologyChangeType.MasterSelected,
				ClusterName = clusterName,
				Metadata = value,
				Uri = MasterSelectedByQuorum
			});
		}

		private void OnPaxosMessage(PaxosMessage msg)
		{
			foreach (var agent in agents)
			{
				agent.DispatchMessage(msg);
			}
		}

		private void OnNewEndpointsDiscovered(Uri[] uris)
		{
			Parallel.ForEach(uris, FindNewEndpointMetadata);
		}

		private void FindNewEndpointMetadata(Uri uri)
		{
			if (topologyState.ContainsKey(uri))
				return;
			FindEndpointMetadata(uri);
		}

		public void Start()
		{
		    var counter = 1;
            while (counter != 0)
            {
                try
                {
                    serviceHost.Open();
                    counter = 0;
                }
                catch (AddressAlreadyInUseException)
                {
                    if (counter == 100)
                        throw;

                    counter++;
                    RefreshServiceHost();
                }
            }
			
			RefreshTopology(serviceHost);
			timer = new Timer(RefreshTopology, serviceHost, heartbeat, heartbeat);
		}

        private void RefreshServiceHost()
        {
            serviceHost = new ServiceHost(new NodeStateService(clusterName, nodeMetadata, OnNewEndpointsDiscovered, OnPaxosMessage));
            try
            {
                serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
                myAddress = new UriBuilder("net.tcp", Environment.MachineName, GetAutoPort(), "/Raven.SituationaAwareness/NodeState").Uri;
                serviceHost.AddServiceEndpoint(typeof(INodeStateService), new NetTcpBinding(SecurityMode.None),
                                               myAddress);

                topologyState.TryAdd(myAddress, nodeMetadata);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

		private static int GetAutoPort()
		{
			var globalProperties = IPGlobalProperties.GetIPGlobalProperties();
			var activeTcpListeners = globalProperties.GetActiveTcpListeners();
			const int portRangeStart = 17232;
			for (int i = portRangeStart; i < portRangeStart + 2048; i++)
			{
				var port = i;
				if (activeTcpListeners.All(x => x.Port != port))
					return port;
			}
			throw new InvalidOperationException(
				"After scanning over 2,000 ports, couldn't find one that was open! What is going on in this machine?!");
		}

		private void RefreshTopology(object state)
		{
			// heartbeat - will remove any failing nodes
			Parallel.ForEach(topologyState.Keys, FindEndpointMetadata);

			var discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
			discoveryClient.FindProgressChanged += DiscoveryClientOnFindProgressChanged;
			discoveryClient.FindCompleted += DiscoveryClientOnFindCompleted;
			discoveryClient.FindAsync(new FindCriteria(typeof(INodeStateService)), state);

		}

		private void DiscoveryClientOnFindProgressChanged(object sender, FindProgressChangedEventArgs findProgressChangedEventArgs)
		{
			FindNewEndpointMetadata(findProgressChangedEventArgs.EndpointDiscoveryMetadata.ListenUris.First());
		}

		private void FindEndpointMetadata(Uri listenUri)
		{
			if (listenUri == myAddress)
				return;
			INodeStateServiceAsync nodeStateServiceAsync = GetNodeStateServiceAsync(listenUri);

			var knownSiblings = topologyState.Keys.Concat(new[] { myAddress }).ToArray();

			Task.Factory.FromAsync<string, Uri[], RemoteNodeMetadata>(nodeStateServiceAsync.BeginGetMetadata, nodeStateServiceAsync.EndGetMetadata, clusterName, knownSiblings, null)
				.ContinueWith(task =>
				{
					// not interested in this one, it just failed
					if (task.Exception != null)
					{
						IDictionary<string, string> value;
						if (topologyState.TryRemove(listenUri, out value) == false) // new node that we can't connect to
						{
							Log("Could not connect to {0} because: {1}", new object[] { listenUri, task.Exception });
							return;
						}
						//notify node removed
						TopologyChanged(this, new NodeMetadata
						{
							ChangeType = TopologyChangeType.Gone,
							Metadata = value,
							Uri = listenUri,
							ClusterName = clusterName
						});
						return;
					}

					CloseWcf(nodeStateServiceAsync);

					if (task.Result.ClusterName != clusterName)
						return; // not interested in this

					if (topologyState.TryAdd(listenUri, task.Result.Metadata) == false)
						return;// already added

					TopologyChanged(this, new NodeMetadata
					{
						ChangeType = TopologyChangeType.Discovered,
						Metadata = task.Result.Metadata,
						Uri = listenUri,
						ClusterName = clusterName
					});
				});
		}

		internal INodeStateServiceAsync GetNodeStateServiceAsync(Uri listenUri)
		{
			return ChannelFactory<INodeStateServiceAsync>.CreateChannel(new NetTcpBinding(SecurityMode.None),
																		new EndpointAddress(listenUri));
		}

		internal void CloseWcf(object item)
		{
			if (item == null)
				return;

			var stateServiceAsync = (ICommunicationObject)item;
			try
			{
				stateServiceAsync.Close();
			}
			catch
			{
				stateServiceAsync.Abort();
			}
		}

		private static void DiscoveryClientOnFindCompleted(object sender, FindCompletedEventArgs findCompletedEventArgs)
		{
			var disposable = sender as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}

		public void Dispose()
		{
			if (timer != null)
				timer.Dispose();
			CloseWcf(serviceHost);
		}
	}
}