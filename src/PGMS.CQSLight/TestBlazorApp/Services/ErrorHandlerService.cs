﻿using PGMS.BlazorComponents;
using PGMS.CQSLight.Infra.Exceptions;

namespace TestBlazorApp.Services;

public class ErrorHandlerService : IErrorHandlerService
{
    public void ShowError(DomainValidationException domainValidationException)
    {

    }

    public Task<bool> HandleError(Exception exception)
    {
        return Task.FromResult(false);
    }
}