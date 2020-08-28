using System.Collections.Generic;
using System.Threading.Tasks;
using SMI.CQSLight.Infra.Commands;
using SMI.CQSLight.Infra.Commands.Services;

namespace SMI.CQSLight.Helpers
{
	public class CommandHelper
	{
		private readonly IBus bus;

		public CommandHelper(IBus bus)
		{
			this.bus = bus;
		}

		public async Task Send(ICommand command, string username)
		{
			command.ByUsername = username;
			
			bus.Send(command);
		}

		public async Task Send(List<ICommand> commands, string username)
		{
			foreach (var command in commands)
			{
				command.ByUsername = username;
				bus.Send(command);
			}
		}
	}
}