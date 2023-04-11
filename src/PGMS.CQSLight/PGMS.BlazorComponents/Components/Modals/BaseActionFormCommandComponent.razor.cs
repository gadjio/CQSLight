using Blazorise;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.BlazorComponents.Models;

namespace PGMS.BlazorComponents.Components.Modals
{
    public abstract partial class BaseActionFormCommandComponent<TActionItem, TCommand, TIcon> : ComponentBase where TActionItem : BaseCqsActionComponent<TCommand>
    {
        [Inject] private ISessionInfoProvider SessionInfoProvider { get; set; }
        [Inject] private IIconGenerator<TIcon> IconGenerator { get; set; }

        [Parameter] public ActionDisplayType DisplayType { get; set; }
        [Parameter] public string Text { get; set; }

        [Parameter] public TIcon ActionIcon { get; set; }

        [Parameter] public string ModalTitle { get; set; }
        [Parameter] public string ActionButtonLabel { get; set; }
        [Parameter] public EventCallback OnActionCompleted { get; set; }

        [Parameter] public ActionFormType ActionFormType { get; set; }

        [Parameter] public ModalSize ModalSize { get; set; } = ModalSize.Default;
        [Parameter] public bool Disabled { get; set; } = false;

        private IActionComponent actionItem;

        protected Dictionary<string, object> actionParameters;

        private bool isAllowed;
        private string notAllowedMessage;

        public abstract Dictionary<string, object> GetActionParameters();

        protected override void OnParametersSet()
        {
            actionParameters = GetActionParameters();
            base.OnParametersSet();
        }

        protected override async Task OnInitializedAsync()
        {
            isAllowed = await IsUserAllowed();

            await base.OnInitializedAsync();
        }

        public Task<bool> IsUserAllowed()
        {
            var commandRoles = PGMS.CQSLight.Infra.Security.AllowedRolesHelper.GetAllowedRoles<TCommand>();

            var isAllowedOnCommand = commandRoles == null || SessionInfoProvider.GetUserRoles().Intersect(commandRoles).Any();

            var roleList = JsonConvert.SerializeObject(commandRoles).Trim(new[] { '{', '}', '[', ']' });
            notAllowedMessage = $"{localizer["Section-" + typeof(TActionItem).Name]} : Require roles : {roleList}";

            return Task.FromResult(isAllowedOnCommand);
        }

        private async Task DisplayModal()
        {
            actionItem.ActionParameters = actionParameters;
            await actionItem.Show();
        }

    }
}
