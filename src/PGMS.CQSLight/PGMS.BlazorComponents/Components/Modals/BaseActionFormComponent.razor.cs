using Blazorise;
using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.BlazorComponents.Models;

namespace PGMS.BlazorComponents.Components.Modals
{
    public abstract partial class BaseActionFormComponent<TActionItem, TIcon> : ComponentBase where TActionItem : BaseCqsActionComponent
    {
        [Inject] private IIconGenerator<TIcon> IconGenerator { get; set; }

        [Parameter] public ActionDisplayType DisplayType { get; set; }
        [Parameter] public string Text { get; set; }

        [Parameter] public TIcon ActionIcon { get; set; }

        //[Parameter] public ActionIconType Icon { get; set; }
        //[Parameter] public ActionIconColor IconColor { get; set; }
        //[Parameter] public string IconCustomClass { get; set; }

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

        private string GetIconClass()
        {
            return null;
            //   return $"{IconClassHelper.GetBasedOnActionDisplayType(DisplayType)} {IconClassHelper.GetBaseOnIconType(Icon)} {IconClassHelper.GetBasedOnIconColor(IconColor)} {IconCustomClass}";
        }

    }
}
