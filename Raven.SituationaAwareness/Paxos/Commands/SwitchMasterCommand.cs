using System;

namespace Raven.SituationaAwareness.Paxos.Commands
{
	public class SwitchMasterCommand : ICommandState
	{
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