using PGMS.BlazorComponents.Models;
using PGMS.CQSLight.Infra.Commands;

namespace PGMS.BlazorComponents
{
    public interface ISessionInfoProvider
    {
        ContextInfo GetContextInfo();
        List<string> GetUserRoles();
    }

    public interface IIconGenerator<in TIcon>
    {
        Type GetIconDynamicComponentType();
        Dictionary<string, object> GetParameters(TIcon iconInfo, ActionDisplayType actionDisplayType);
    }
}