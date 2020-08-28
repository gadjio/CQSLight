using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SMI.CQSLight.Extensions;
using SMI.CQSLight.Infra.Commands.Services;
using SMI.CQSLight.Infra.Exceptions;

namespace SMI.CQSLight.Infra.Commands
{
    public interface IHandleCommand<T> where T : ICommand
    {
        void Execute(T command);
        void OnFail(T command);
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

        public void Execute(T command)
        {
            List<ValidationResult> commandValidationResult;
            command.Validate(out commandValidationResult);
            if (commandValidationResult != null && commandValidationResult.Any())
            {
                throw new DomainValidationException(GetErrorMessage(commandValidationResult));
            }

            var validationResults = ValidateCommand(command);
            if (validationResults != null && validationResults.Any())
            {
                throw new DomainValidationException(GetErrorMessage(validationResults.ToList()));
            }

            var @event = PublishEvent(command);
            @event.ByUser = command.ByUsername;
            @event.AggregateId = command.AggregateRootId;

            bus.Publish(@event);
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

        public void OnFail(T command)
        {

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

	    public void Execute(T command)
	    {
		    List<ValidationResult> commandValidationResult;
		    command.Validate(out commandValidationResult);
		    if (commandValidationResult != null && commandValidationResult.Any())
		    {
			    throw new DomainValidationException(GetErrorMessage(commandValidationResult));
		    }

		    var validationResults = ValidateCommand(command);
		    if (validationResults != null && validationResults.Any())
		    {
			    throw new DomainValidationException(GetErrorMessage(validationResults.ToList()));
		    }

		    var @events = PublishEvents(command);

		    foreach (var @event in events)
		    {
			    @event.ByUser = command.ByUsername;
			    @event.AggregateId = command.AggregateRootId;
            }
		    
		    bus.Publish(@events);
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

	    public void OnFail(T command)
	    {

	    }
    }


   
}