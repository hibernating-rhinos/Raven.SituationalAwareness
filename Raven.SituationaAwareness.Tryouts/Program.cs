using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.SituationaAwareness.Tryouts
{
	class Program
	{
		static void Main(string[] args)
		{
			var presence = new Presence("RavenDB", new Dictionary<string, string>
			{
				{"StartTime", DateTime.Now.ToString("r")}
			}, TimeSpan.FromSeconds(3));
			presence.TopologyChanged += (sender, nodeMetadata) =>
			{
				switch (nodeMetadata.ChangeType)
				{
					case TopologyChangeType.MasterSelected:
						Console.WriteLine("Master selected {0}", nodeMetadata.Uri);
						break;
					case TopologyChangeType.Discovered:
						Console.WriteLine("Found {0}", nodeMetadata.Uri);
						break;
					case TopologyChangeType.Gone:
						Console.WriteLine("Oh no, {0} is gone!", nodeMetadata.Uri);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
			presence.Start();
			Console.WriteLine(presence.Address);
			Console.WriteLine("Waiting...");
			Console.ReadLine();
		}
	}
}
