using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Extensions;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IHandleCommand<T> where T : ICommand
    {
        Task Execute(T command, IContextInfo contextInfo);
        Task OnFail(T command);
    }

    public abstract class HandleCommand<T> : IHandleCommand<T> where T : BaseCommand
    {
        public abstract Task DoAdditionalValidations(T command, IContextInfo contextInfo);
        public abstract Task ProcessCommand(T command, IContextInfo contextInfo);

        public async Task Execute(T command, IContextInfo contextInfo)
        {
            command.Validate(out var commandValidationResult);
            if (commandValidationResult != null && commandValidationResult.Any())
            {
                throw new DomainValidationException(GetErrorMessage(commandValidationResult), commandValidationResult);
            }

            if (contextInfo?.SkipRoleValidation == false)
            {
                if (command.IsAllowed(contextInfo.UserRoles) == false)
                {
                    var commandRoles = AllowedRolesHelper.GetAllowedRoles<T>();
                    throw new DomainSecurityValidationException(commandRoles, typeof(T), $"User not permitted on {typeof(T).Name}");
                }
            }

            await DoAdditionalValidations(command, contextInfo);

            await ProcessCommand(command, contextInfo);
        }

        protected static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
        {
            var sb = new StringBuilder();
            foreach (var result in commandValidationResult)
            {
                var prefix = "";
                if (result.MemberNames != null && result.MemberNames.Any())
                {
                    prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
                    prefix = prefix + " - ";
                }

                sb.AppendLine(prefix + result.ErrorMessage);
            }
            return sb.ToString();
        }


        public Task OnFail(T command)
        {
            return Task.CompletedTask;
        }
    }
}