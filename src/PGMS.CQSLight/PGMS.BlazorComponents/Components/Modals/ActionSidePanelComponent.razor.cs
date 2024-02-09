using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.BlazorComponents.Extensions;
using PGMS.BlazorComponents.Models;

namespace PGMS.BlazorComponents.Components.Modals;

public partial class ActionSidePanelComponent<TActionItem> : ComponentBase, IActionComponent where TActionItem : BaseCqsActionComponent
{
    [Parameter]
	public Dictionary<string, object> ActionParameters { get; set; }

	[Parameter]
	public string[] AuthorizedRoles { get; set; }

	[Parameter]
	public string ModalTitle { get; set; }

	[Parameter]
	public string ActionButtonLabel { get; set; }

    [Parameter]
    public bool ShowCancelButton { get; set; } = true;

    [Parameter]
	public EventCallback OnActionCompleted { get; set; }

    [Parameter] 
    public ModalPanelSide Side { get; set; } = ModalPanelSide.Right;

    [Parameter]
    public ModalPanelSize Size { get; set; } = ModalPanelSize.Medium;

	private Task OnBeforeShow { get; set; }

    protected BaseCqsActionComponent actionComponent;

    private bool actionProcessing;

	public string roles = "";

    public bool open = false;

	protected override async Task OnParametersSetAsync()
	{
		Action<BaseCqsActionComponent> p1 = RegisterActionItem;
        ActionParameters.AddOrUpdate(nameof(BaseCqsActionComponent.Register), p1);

        if (AuthorizedRoles?.Any() == true)
		{
			foreach (var role in AuthorizedRoles)
			{
				roles = roles + ", " + role;
			}
			roles = roles.Trim(new[] { ' ', ',' });
		}

		await base.OnParametersSetAsync();
	}

    void RegisterActionItem(BaseCqsActionComponent actionItem)
	{
		actionComponent = actionItem;
	}


	public async Task ProcessAction()
	{
		actionProcessing = true;
		await Task.Delay(1);

		var actionCompleted = await actionComponent.ProcessAction();
		if (actionCompleted)
		{
			//await modal.Hide();
			//sidepanel.Close();
            open = false;
			if (OnActionCompleted.HasDelegate)
			{
				await OnActionCompleted.InvokeAsync();
			}
		}

		actionProcessing = false;
	}


	public async Task Show()
	{
		//Allow to refresh the dynamic component to SetParameters
        StateHasChanged();

		actionProcessing = false;
		//sidepanel.Open(ModalTitle, typeof(TActionItem), null, ActionParameters);
        open = true;

		if (actionComponent != null)
		{
            await actionComponent.SetLoading();
			await actionComponent.InitCommand();
			await actionComponent.SetLoadingComplete();
			StateHasChanged();
		}

	}

	public Task HideModal()
    {
        open = false;
        //sidepanel.Close();
        return Task.CompletedTask;
    }

    public string GetSidePanelMainClasses()
    {
		var sizeClass = Size switch
        {
            ModalPanelSize.Small => "is-open-sm",
			ModalPanelSize.Medium => "is-open-md",
			ModalPanelSize.Large => "is-open-lg",
            _ => ""
        };

        var sideClass = Side == ModalPanelSide.Right ? "aside-right" : "aside-left";
        var openClasses = $"{sizeClass} {sideClass}";
        var closedClasses = $"is-close {sideClass}";
        
        return open ? openClasses : closedClasses;
    }
}