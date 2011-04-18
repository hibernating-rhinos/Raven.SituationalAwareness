﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.SituationaAwareness.Tryouts
{
	class Program
	{
		static void Main(string[] args)
		{
			var presence = new Presence(new Dictionary<string, string>
			{
				{"ClusterName", "Ayende"},
				{"StartTime", DateTime.Now.ToString("r")}
			});
			presence.TopologyChanged+=PresenceOnTopologyChanged;
			presence.Start();
			Console.WriteLine("Waiting...");
			Console.ReadLine();
		}

		private static void PresenceOnTopologyChanged(object sender, NodeMetadata nodeMetadata)
		{
			Console.WriteLine("Found {0}", nodeMetadata.Uri);
			foreach (var item in nodeMetadata.Metadata)
			{
				Console.WriteLine("\t{0}: {1}", item.Key, item.Value);
			}
		}
	}
}