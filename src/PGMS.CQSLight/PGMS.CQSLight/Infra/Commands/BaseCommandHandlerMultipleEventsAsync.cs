
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;

namespace PGMS.CQSLight.Infra.Commands
{
    public abstract class BaseCommandHandlerMultipleEventsAsync<T> : HandleCommand<T> where T : BaseCommand
	{
		private readonly IBus bus;

		protected BaseCommandHandlerMultipleEventsAsync(IBus bus)
		{
			this.bus = bus;
		}

		public abstract Task<List<ValidationResult>> ValidateCommand(T command);

		public abstract Task<List<IEvent>> PublishEvents(T command);

        public override async Task DoAdditionalValidations(T command, IContextInfo contextInfo)
        {
            var validationResults = await ValidateCommand(command);
            if (validationResults != null && validationResults.Any())
            {
                throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
            }
        }

        public override async Task ProcessCommand(T command, IContextInfo contextInfo)
        {
            var @events = await PublishEvents(command);

            foreach (var @event in events)
            {
                SetEventContextInfo(command, contextInfo, @event);
            }

            await bus.Publish(@events);
        }

        protected virtual void SetEventContextInfo(T command, IContextInfo contextInfo, IEvent @event)
        {
            if (string.IsNullOrEmpty(@event.ByUsername))
            {
                @event.ByUsername = contextInfo.ByUsername;
            }

            if (string.IsNullOrEmpty(@event.ByUserId))
            {
                @event.ByUserId = contextInfo.ByUserId;
            }

            if (@event.AggregateId == Guid.Empty)
            {
                @event.AggregateId = command.AggregateRootId;
            }

            if (string.IsNullOrEmpty(@event.CommandType))
            {
                @event.CommandType = typeof(T).FullName;
            }
        }
    }

}