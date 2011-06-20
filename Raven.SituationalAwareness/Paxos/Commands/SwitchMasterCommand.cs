using System;
using System.Runtime.Serialization;

namespace Raven.SituationalAwareness.Paxos.Commands
{
	[DataContract]
	public class SwitchMasterCommand : ICommandState
	{
		[DataMember]
		public Uri NewMaster { get; set; }

		public int CompareTo(ICommandState other)
		{
			var switchMasterCommand = other as SwitchMasterCommand;
			if (switchMasterCommand == null)
				return 1;
			return NewMaster.AbsoluteUri.CompareTo(switchMasterCommand.NewMaster.AbsoluteUri);
		}
	}
}