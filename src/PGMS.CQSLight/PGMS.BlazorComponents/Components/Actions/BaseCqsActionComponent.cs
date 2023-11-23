using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using PGMS.Data.Services;
using ICommand = PGMS.CQSLight.Infra.Commands.ICommand;

namespace PGMS.BlazorComponents.Components.Actions;

public abstract class BaseCqsActionComponent : BaseSecureComponent
{
    [Inject] public IDataService dataService { get; set; }
    [Inject] public IEntityRepository entityRepository { get; set; }

    public event Action RefreshRequested;
    public void CallRequestRefresh()
    {
        RefreshRequested?.Invoke();
    }

    [Parameter]
    public EventCallback<Action<BaseCqsActionComponent>> RegisterItemChanged { get; set; }

    private Action<BaseCqsActionComponent> refAction;

    [Parameter]
    public Action<BaseCqsActionComponent> Register
    {
        get => refAction;
        set
        {
            if (refAction == value)
            {
                return;
            }

            refAction = value;
            RegisterItemChanged.InvokeAsync(value);
        }
    }

    protected override void OnParametersSet()
    {
        if (refAction != null)
        {
            // register this component
            Register(this);
            //var value = this;
        }
    }

    public void ResetValidationResults()
    {
        CommandValidationResults = new List<ValidationResult>();
    }

    public abstract Task InitCommand();
    public abstract Task<bool> ProcessAction();

    public List<System.ComponentModel.DataAnnotations.ValidationResult> CommandValidationResults;

    public async Task<bool> ProcessCommand(ICommand command)
    {
        var result = await SendCommand(command);
        if (!result.IsSuccess)
        {
            CommandValidationResults = result.ValidationResults;
            return false;
        }

        return true;
    }

    public async Task<bool> ProcessCommands(List<ICommand> commands)
    {
        var result = await SendCommands(commands);
        if (!result.IsSuccess)
        {
            CommandValidationResults = result.ValidationResults;
            return false;
        }

        return true;
    }

    public virtual Task<bool> IsSubmitDisabled()
    {
        return Task.FromResult(false);
    }

    public virtual string? ActionButtonLabel()
    {
        return null;
    }
}

public class SendCommandResult
{
    public bool IsSuccess { get; set; }

    public List<ValidationResult> ValidationResults { get; set; }
}
