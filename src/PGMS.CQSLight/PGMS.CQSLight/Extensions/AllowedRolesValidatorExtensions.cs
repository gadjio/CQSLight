using System;
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

        var convertedRoles = roles == null ? new List<string>() : roles.Select(x => x.ToLowerInvariant()).ToList();
        allowedRoles = allowedRoles.Select(x => x.ToLowerInvariant()).ToList();

        return convertedRoles.Intersect(allowedRoles).Any();
    }

    public static bool IsAllowed(Type t, List<string> roles)
    {
        var allowedRoles = AllowedRolesHelper.GetAllowedRoles(t);
        if (allowedRoles == null)
        {
            return true;
        }

        var convertedRoles = roles == null ? new List<string>() : roles.Select(x => x.ToLowerInvariant()).ToList();
        allowedRoles = allowedRoles.Select(x => x.ToLowerInvariant()).ToList();

        return convertedRoles.Intersect(allowedRoles).Any();
    }
}