using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Infra.Commands;

public abstract class BaseCommandHandlerAsync<T> : HandleCommand<T> where T : BaseCommand
{
    private readonly IBus bus;

    protected BaseCommandHandlerAsync(IBus bus)
    {
        this.bus = bus;
    }

    public abstract Task<List<ValidationResult>> ValidateCommand(T command);

    public abstract Task<IEvent> PublishEvent(T command);


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
        var @event = await PublishEvent(command);
        
        SetEventContextInfo(command, contextInfo, @event);
        
        await bus.Publish(@event);
    }

    protected virtual void SetEventContextInfo(T command, IContextInfo contextInfo, IEvent @event)
    {
        @event.ByUsername = contextInfo.ByUsername;
        @event.ByUserId = contextInfo.ByUserId;
        @event.AggregateId = command.AggregateRootId;
        @event.CommandType = typeof(T).FullName;
    }
}