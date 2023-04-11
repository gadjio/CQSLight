using PGMS.CQSLight.Infra.Exceptions;

namespace PGMS.BlazorComponents
{
    public interface IErrorHandlerService
    {
        void ShowError(DomainValidationException domainValidationException);

        Task<bool> HandleError(Exception exception);
    }
}