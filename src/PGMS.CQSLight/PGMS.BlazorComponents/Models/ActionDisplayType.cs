using Blazorise;

namespace PGMS.BlazorComponents.Models;

public enum ActionDisplayType
{
    Button,
    QuickAction,
    ActionLink
}

public enum ActionFormType
{
    SidePanel,
    Modal,
}

public interface IActionIcon
{
    Type GetIconDynamicComponentType();
    Dictionary<string, object> GetParameters(ActionDisplayType actionDisplayType);
}