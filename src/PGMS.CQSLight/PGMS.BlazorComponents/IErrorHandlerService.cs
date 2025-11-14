using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;

namespace PGMS.BlazorComponents
{
    public interface IErrorHandlerService
    {
        void ShowError(DomainValidationException domainValidationException);

        Task<bool> HandleError(ICommand? command, ContextInfo? contextInfo, Exception exception);
    }
}