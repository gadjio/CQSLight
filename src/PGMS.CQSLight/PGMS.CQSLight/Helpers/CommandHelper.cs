using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Helpers
{
	public class CommandHelper
	{
		private readonly IBus bus;

		public CommandHelper(IBus bus)
		{
			this.bus = bus;
		}

        public async Task Send(ICommand command, ContextInfo contextInfo)
        {
            await bus.Send(command, contextInfo);
        }

        public async Task Send(List<ICommand> commands, ContextInfo contextInfo)
        {
            foreach (var command in commands)
            {
                await bus.Send(command, contextInfo);
            }
        }

        public async Task Send(ICommand command, string username)
        {
            await bus.Send(command, new ContextInfo{ByUsername = username});
        }

		public async Task Send(List<ICommand> commands, string username)
        {
            var contextInfo = new ContextInfo { ByUsername = username };
            foreach (var command in commands)
            {
                await bus.Send(command, contextInfo);
            }
        }

		public async Task Send(ICommand command, string username, string userId)
		{
            await bus.Send(command, new ContextInfo { ByUsername = username, ByUserId = userId});
		}

		public async Task Send(List<ICommand> commands, string username, string userId)
		{
            var contextInfo = new ContextInfo { ByUsername = username, ByUserId = userId};
            foreach (var command in commands)
			{
                await bus.Send(command, contextInfo);
			}
		}
	}
}