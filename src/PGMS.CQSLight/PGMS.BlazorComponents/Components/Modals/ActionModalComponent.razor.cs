using Blazorise;
using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.BlazorComponents.Extensions;

namespace PGMS.BlazorComponents.Components.Modals
{
    public interface IActionComponent
    {
        Dictionary<string, object> ActionParameters { get; set; }
        Task Show();
    }


    public partial class ActionModalComponent<TActionItem> : ComponentBase, IActionComponent where TActionItem : BaseCqsActionComponent
	{
        [Inject] public BlazorComponentsOptions BlazorComponentsOptions { get; set; }

        [Parameter]
		public Dictionary<string, object> ActionParameters { get; set; } = new Dictionary<string, object>();

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
        public EventCallback OnCloseModal { get; set; }

        [Parameter]
        public ModalSize ModalSize { get; set; } = ModalSize.Large;

        [Parameter]
        public RenderFragment SectionFooter { get; set; }

        [Parameter]
        public RenderFragment SectionHeader { get; set; }

        [Parameter]
        public RenderFragment<List<System.ComponentModel.DataAnnotations.ValidationResult>> ErrorDisplay { get; set; }

        /// <summary>
        /// Will override the ActionModalSettings.PreventClose settings
        /// </summary>
        [Parameter]
        public bool? PreventClose { get; set; }

        protected BaseCqsActionComponent actionComponent;

        protected Modal modal;

        private bool actionProcessing;

        public string roles = "";

        private bool isSubmitDisabled = false;

        private bool isPreventClose;
        private bool cancelClose;

        protected override async Task OnParametersSetAsync()
        {
            isPreventClose = BlazorComponentsOptions?.ActionModalSettings?.PreventClose == true;
            if (PreventClose.HasValue)
            {
                isPreventClose = PreventClose.Value;
            }

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


        protected void RegisterActionItem(BaseCqsActionComponent actionItem)
        {
            actionComponent = actionItem;
            actionComponent.RefreshRequested += async () => await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            isSubmitDisabled = await actionComponent.IsSubmitDisabled();
            StateHasChanged();
        }

        public async Task ProcessAction()
        {
            actionProcessing = true;
            await Task.Delay(1);

            var actionCompleted = await actionComponent.ProcessAction();
            if (actionCompleted)
            {
                cancelClose = false;
                await modal.Hide();
                if (OnActionCompleted.HasDelegate)
                {
                    await OnActionCompleted.InvokeAsync();
                }
			}

            actionProcessing = false;
        }

        public async Task Show()
        {
            cancelClose = true;
            //Allow to refresh the dynamic component to SetParameters
            await InvokeAsync(StateHasChanged);

            actionProcessing = false;

            if (actionComponent != null)
            {
	            await actionComponent.SetLoading();
                actionComponent.ResetValidationResults();
                await modal.Show();
                await actionComponent.InitCommand();
                isSubmitDisabled = await actionComponent.IsSubmitDisabled();
                await actionComponent.SetLoadingComplete();
                StateHasChanged();
			}
        }

        

        public async Task HideModal()
        {
            cancelClose = false;
            await modal.Hide();

            if (OnCloseModal.HasDelegate)
            {
                await OnCloseModal.InvokeAsync();
            }
            
        }

        private async Task OnModalClosing(ModalClosingEventArgs arg)
        {
            if (isPreventClose)
            {
                arg.Cancel = cancelClose
                           || arg.CloseReason != CloseReason.UserClosing;
            }

            if (arg.Cancel)
            {
                return;
            }

            if (OnCloseModal.HasDelegate)
            {
                await OnCloseModal.InvokeAsync();
            }
        }

        private string GetActionButtonLabel()
        {
            if (string.IsNullOrEmpty(actionComponent.ActionButtonLabel()) == false)
            {
                return actionComponent.ActionButtonLabel();
            }

            return ActionButtonLabel;
        }
    }
}
