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
        [Parameter]
		public Dictionary<string, object> ActionParameters { get; set; } = new Dictionary<string, object>();

        [Parameter]
        public string[] AuthorizedRoles { get; set; }

		[Parameter]
		public string ModalTitle { get; set; }

        [Parameter]
        public string ActionButtonLabel { get; set; }

        [Parameter]
        public EventCallback OnActionCompleted { get; set; }

        [Parameter]
        public ModalSize ModalSize { get; set; } = ModalSize.Large;

        [Parameter]
        public RenderFragment SectionFooter { get; set; }

        [Parameter]
        public RenderFragment SectionHeader { get; set; }

        [Parameter]
        public RenderFragment<List<System.ComponentModel.DataAnnotations.ValidationResult>> ErrorDisplay { get; set; }

        protected BaseCqsActionComponent actionComponent;

        protected Modal modal;

        private bool actionProcessing;

        public string roles = "";

        private bool isSubmitDisabled = false;

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


        protected void RegisterActionItem(BaseCqsActionComponent actionItem)
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
            //Allow to refresh the dynamic component to SetParameters
			StateHasChanged();

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
            await modal.Hide();
        }
    }
}
