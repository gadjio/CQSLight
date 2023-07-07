using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Infra.Commands;

public abstract class BaseCommandHandlerMultipleEvents<T> : HandleCommand<T> where T : BaseCommand
{
    private readonly IBus bus;

    protected BaseCommandHandlerMultipleEvents(IBus bus)
    {
        this.bus = bus;
    }

    public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

    public abstract List<IEvent> PublishEvents(T command);

    public override Task DoAdditionalValidations(T command, IContextInfo contextInfo)
    {
        var validationResults = ValidateCommand(command).ToList();
        if (validationResults != null && validationResults.Any())
        {
            throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
        }
            
        return Task.CompletedTask;
    }

    public override async Task ProcessCommand(T command, IContextInfo contextInfo)
    {
        var @events = PublishEvents(command);

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
    }

}