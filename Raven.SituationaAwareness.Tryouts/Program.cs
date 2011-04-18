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
			presence.TopologyChanged+=(sender, nodeMetadata) =>
			{
				if(nodeMetadata.ChangeType == TopologyChangeType.Gone)
				{
					Console.WriteLine("Oh no, {0} is gone!", nodeMetadata.Uri);
					return;
				}
				Console.WriteLine("Found {0}", nodeMetadata.Uri);
				foreach (var item in nodeMetadata.Metadata)
				{
					Console.WriteLine("\t{0}: {1}", item.Key, item.Value);
				}
			};
			presence.Start();
			Console.WriteLine("Waiting...");
			Console.ReadLine();
		}
	}
}
