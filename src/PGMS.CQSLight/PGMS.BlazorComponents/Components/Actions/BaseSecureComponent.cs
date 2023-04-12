﻿using Blazorise;
using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Attributes;
using PGMS.CQSLight.Helpers;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.Data.Services;

namespace PGMS.BlazorComponents.Components.Actions
{
    public class BaseSecureComponent : BaseComponent
    {
        [Inject] private CommandHelper CommandHelper { get; set; }
        [Inject] protected QueryHelper QueryHelper { get; set; }
        [Inject] private IDataService DataService { get; set; }
        [Inject] private IErrorHandlerService ErrorHandlerService { get; set; }
        [Inject] private ISessionInfoProvider SessionInfoProvider { get; set; }

        protected bool Loading;

        protected RestrictedViewAttribute restrictedViewAttribute;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            restrictedViewAttribute = Attribute.GetCustomAttribute(this.GetType(), typeof(RestrictedViewAttribute)) as RestrictedViewAttribute;
        }

        protected override async Task OnInitializedAsync()
        {
            await SetLoading();
            await base.OnInitializedAsync();
        }

    

        public async Task SetLoading()
        {
            Loading = true;
            await Task.Delay(1);
        }

        public async Task SetLoadingComplete()
        {
            Loading = false;
            await Task.Delay(1);
        }

        protected async Task<SendCommandResult> SendCommand(ICommand command)
        {
            try
            {
                await CommandHelper.Send(command, SessionInfoProvider.GetContextInfo());
            }
            catch (DomainValidationException e)
            {
                ErrorHandlerService.ShowError(e);
                return await Task.FromResult(new SendCommandResult { ValidationResults = e.ValidationResult });
            }
            catch (Exception e)
            {
                var exception = e.InnerException as DomainValidationException;
                if (exception != null)
                {
                    ErrorHandlerService.ShowError(exception);
                    return await Task.FromResult(new SendCommandResult { ValidationResults = exception.ValidationResult });
                }

                if (! await ErrorHandlerService.HandleError(e))
                {
                    throw;
                }

            
            }
            return await Task.FromResult(new SendCommandResult { IsSuccess = true });
        }

        protected async Task<bool> SendCommands(List<ICommand> commands)
        {
            try
            {
                await CommandHelper.Send(commands, SessionInfoProvider.GetContextInfo());
            }
            catch (DomainValidationException e)
            {
                ErrorHandlerService.ShowError(e);
                return await Task.FromResult(false);
            }
            catch (Exception e)
            {
                var exception = e.InnerException as DomainValidationException;
                if (exception != null)
                {
                    ErrorHandlerService.ShowError(exception); ;
                    return await Task.FromResult(false);
                }

                if (!await ErrorHandlerService.HandleError(e))
                {
                    throw;
                }
            }
            return await Task.FromResult(true);
        }


        protected long GenerateEntityId()
        {
            return DataService.GenerateId();
        }

    }
}