using PGMS.BlazorComponents;
using PGMS.CQSLight.Infra.Commands;

namespace TestBlazorApp.Services;

public class SessionInfoProvider : ISessionInfoProvider
{
    
    public ContextInfo GetContextInfo() => new()
    {
        //ByUsername = userIdentity.UserName,
        ////ByUserId = userIdentity.TokenUserInfo.UserId,
        //UserRoles = userIdentity.Roles,
        //SkipRoleValidation = false
    };

    public List<string> GetUserRoles()
    {
        return new List<string>();
    }
}