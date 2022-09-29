using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Extensions;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IHandleCommand<T> where T : ICommand
    {
        Task Execute(T command);
        Task OnFail(T command);
    }

    public abstract class BaseCommandHandler<T> : IHandleCommand<T> where T : BaseCommand
    {
        private readonly IBus bus;

        protected BaseCommandHandler(IBus bus)
        {
            this.bus = bus;
        }

        public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

        public abstract IEvent PublishEvent(T command);

        public async Task Execute(T command)
        {
	        command.Validate(out var commandValidationResult);
            if (commandValidationResult != null && commandValidationResult.Any())
            {
                throw new DomainValidationException(GetErrorMessage(commandValidationResult), commandValidationResult);
            }

            var validationResults = ValidateCommand(command)?.ToList();
            if (validationResults != null && validationResults.Any())
            {
                throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
            }

            var @event = PublishEvent(command);
            @event.ByUsername = command.ByUsername;
            @event.ByUserId = command.ByUserId;
            @event.AggregateId = command.AggregateRootId;

            await bus.Publish(@event);

        }

        private static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
        {
            var sb = new StringBuilder();
            foreach (var result in commandValidationResult)
            {
                var prefix = "";
                if (result.MemberNames != null && result.MemberNames.Any())
                {
                    prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
                    prefix = prefix + " - ";
                }

                sb.AppendLine(prefix + result.ErrorMessage);
            }
            return sb.ToString();
        }

        public Task OnFail(T command)
        {
	        return Task.CompletedTask;
        }
    }

    public abstract class BaseCommandHandlerMultipleEvents<T> : IHandleCommand<T> where T : BaseCommand
    {
	    private readonly IBus bus;

	    protected BaseCommandHandlerMultipleEvents(IBus bus)
	    {
		    this.bus = bus;
	    }

	    public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

	    public abstract List<IEvent> PublishEvents(T command);

	    public async Task Execute(T command)
	    {
		    command.Validate(out var commandValidationResult);
		    if (commandValidationResult != null && commandValidationResult.Any())
		    {
			    throw new DomainValidationException(GetErrorMessage(commandValidationResult), commandValidationResult);
		    }

		    var validationResults = ValidateCommand(command).ToList();
		    if (validationResults != null && validationResults.Any())
		    {
			    throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
		    }

		    var @events = PublishEvents(command);

		    foreach (var @event in events)
		    {
			    if (string.IsNullOrEmpty(@event.ByUsername))
			    {
				    @event.ByUsername = command.ByUsername;
			    }

                if (string.IsNullOrEmpty(@event.ByUserId))
                {
                    @event.ByUserId = command.ByUserId;
                }

				if (@event.AggregateId == Guid.Empty)
			    {
				    @event.AggregateId = command.AggregateRootId;
			    }
            }
		    
		    await bus.Publish(@events);
	    }

	    private static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
	    {
		    var sb = new StringBuilder();
		    foreach (var result in commandValidationResult)
		    {
			    var prefix = "";
			    if (result.MemberNames != null && result.MemberNames.Any())
			    {
				    prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
				    prefix = prefix + " - ";
			    }

			    sb.AppendLine(prefix + result.ErrorMessage);
		    }
		    return sb.ToString();
	    }

	    public Task OnFail(T command)
	    {
		    return Task.CompletedTask;
	    }
    }


   
}