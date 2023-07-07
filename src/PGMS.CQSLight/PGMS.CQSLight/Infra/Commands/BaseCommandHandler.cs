using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Infra.Commands;

public abstract class BaseCommandHandler<T> : HandleCommand<T> where T : BaseCommand
{
    private readonly IBus bus;

    protected BaseCommandHandler(IBus bus)
    {
        this.bus = bus;
    }

    public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

    public abstract IEvent PublishEvent(T command);


    public override Task DoAdditionalValidations(T command, IContextInfo contextInfo)
    {
        var validationResults = ValidateCommand(command)?.ToList();
        if (validationResults != null && validationResults.Any())
        {
            throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
        }

        return Task.CompletedTask;
    }

    public override async Task ProcessCommand(T command, IContextInfo contextInfo)
    {
        var @event = PublishEvent(command);
        SetEventContextInfo(command, contextInfo, @event);

        await bus.Publish(@event);
    }

    protected virtual void SetEventContextInfo(T command, IContextInfo contextInfo, IEvent @event)
    {
        @event.ByUsername = contextInfo.ByUsername;
        @event.ByUserId = contextInfo.ByUserId;
        @event.AggregateId = command.AggregateRootId;
    }
}