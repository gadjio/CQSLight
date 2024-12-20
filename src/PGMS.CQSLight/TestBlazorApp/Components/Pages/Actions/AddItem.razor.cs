using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using PGMS.BlazorComponents.Components.Actions;

namespace TestBlazorApp.Components.Pages.Actions
{
    public partial class AddItem : BaseCqsActionComponent
    {
        [Inject] private IStringLocalizer localizer { get; set; }

        public override Task InitCommand()
        {
            return Task.CompletedTask;
        }

        public override Task<bool> ProcessAction()
        {
            return Task.FromResult(true);
        }
    }
}
