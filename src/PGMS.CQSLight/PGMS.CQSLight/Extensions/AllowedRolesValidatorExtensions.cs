using System.Collections.Generic;
using System.Linq;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Extensions;

public static class AllowedRolesValidatorExtensions
{
    public static bool IsAllowed<T>(this T obj, List<string> roles)
    {
        var allowedRoles = AllowedRolesHelper.GetAllowedRoles<T>();
        if (allowedRoles == null)
        {
            return true;
        }

        var convertedRoles = roles.Select(x => x.ToLowerInvariant()).ToList();
        allowedRoles = allowedRoles.Select(x => x.ToLowerInvariant()).ToList();

        return convertedRoles.Intersect(allowedRoles).Any();
    }
}