using Microsoft.AspNetCore.Components;
using PGMS.BlazorComponents.Components.Actions;

namespace PGMS.BlazorComponents.Components.Modals;


public class ActionFormCommandComponent<TActionItem, TCommand, TIcon> : BaseActionFormCommandComponent<TActionItem, TCommand, TIcon> where TActionItem : BaseCqsActionComponent<TCommand>
{
    public override Dictionary<string, object> GetActionParameters()
    {
        var result = new Dictionary<string, object>();

        var parentProps = typeof(BaseCqsActionComponent<>).GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false)).Select(x => x.Name).ToList();
        var props = this.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));
        var actionProps = typeof(TActionItem).GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));

        foreach (var propertyInfo in actionProps)
        {
            if (parentProps.Contains(propertyInfo.Name))
            {
                continue;
            }

            var matchingProp = props.First(x => x.Name == propertyInfo.Name);

            result.Add(matchingProp.Name, matchingProp.GetValue(this));
        }



        return result;
    }
}


public class ActionFormComponent<TActionItem, TIcon> : BaseActionFormComponent<TActionItem, TIcon> where TActionItem : BaseCqsActionComponent
{
    public override Dictionary<string, object> GetActionParameters()
    {
        var result = new Dictionary<string, object>();

        var parentProps = typeof(BaseCqsActionComponent<>).GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false)).Select(x => x.Name).ToList();
        var props = this.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));
        var actionProps = typeof(TActionItem).GetProperties().Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));

        foreach (var propertyInfo in actionProps)
        {
            if (parentProps.Contains(propertyInfo.Name))
            {
                continue;
            }

            var matchingProp = props.First(x => x.Name == propertyInfo.Name);

            result.Add(matchingProp.Name, matchingProp.GetValue(this));
        }


        return result;
    }
}
