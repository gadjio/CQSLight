using Blazorise;
using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.BlazorComponents.Models;

namespace PGMS.BlazorComponents.Components.Modals
{
    public abstract partial class BaseActionFormComponent<TActionItem> : ComponentBase where TActionItem : BaseCqsActionComponent
    {
        [Parameter] public ActionDisplayType DisplayType { get; set; }
        [Parameter] public string Text { get; set; }

        [Parameter] public IDynamicIcon DynamicIcon { get; set; }

        [Parameter] public string ModalTitle { get; set; }
        [Parameter] public string ActionButtonLabel { get; set; }
        [Parameter] public EventCallback OnActionCompleted { get; set; }

        [Parameter] public ActionFormType ActionFormType { get; set; }

        [Parameter] public ModalSize ModalSize { get; set; } = ModalSize.Default;
        [Parameter] public bool Disabled { get; set; } = false;

        private IActionComponent actionItem;

        protected Dictionary<string, object> actionParameters;

        public abstract Dictionary<string, object> GetActionParameters();

        protected override void OnParametersSet()
        {
            actionParameters = GetActionParameters();
            base.OnParametersSet();
        }


        private async Task DisplayModal()
        {
            actionItem.ActionParameters = actionParameters;
            await actionItem.Show();
        }


    }
}
