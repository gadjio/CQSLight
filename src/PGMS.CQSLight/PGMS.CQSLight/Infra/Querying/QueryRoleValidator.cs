using System;
using System.Linq;
using System.Reflection;
using PGMS.CQSLight.Extensions;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.CQSLight.Infra.Querying;

/// <summary>Applies the command role policy to queries before resolving a handler or reading a cache.</summary>
public static class QueryRoleValidator
{
    public static IContextInfo Validate(object query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (AllowedRolesHelper.GetAllowedRoles(query.GetType()) == null) return null;

        var properties = query.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var candidates = properties.Where(property => property.IsDefined(typeof(QueryContextAttribute), true)).ToArray();
        if (candidates.Length == 0)
            candidates = properties.Where(property => property.Name is "ContextInfo" or "UserContextInfo").ToArray();
        if (candidates.Length > 1)
            throw new InvalidOperationException("A query must identify a single caller context with QueryContextAttribute.");

        IContextInfo context = null;
        if (candidates.Length == 1)
        {
            var property = candidates[0];
            if (property.GetMethod?.IsPublic != true || property.GetIndexParameters().Length != 0
                || !typeof(IContextInfo).IsAssignableFrom(property.PropertyType))
                throw new InvalidOperationException("A query caller context must be a readable IContextInfo property.");
            context = (IContextInfo)property.GetValue(query);
        }
        return Validate(query, context);
    }

    /// <summary>
    /// An explicit context takes precedence over every property on the query, including when null.
    /// SkipRoleValidation is reserved for trusted internal callers, as on the command path.
    /// </summary>
    public static IContextInfo Validate(object query, IContextInfo context)
    {
        ArgumentNullException.ThrowIfNull(query);
        var type = query.GetType();
        var allowedRoles = AllowedRolesHelper.GetAllowedRoles(type);
        if (allowedRoles == null) return context;
        if (context == null || (!context.SkipRoleValidation && !AllowedRolesValidatorExtensions.IsAllowed(type, context.UserRoles)))
            throw new DomainSecurityValidationException(allowedRoles, type, $"User not permitted on {type.Name}");
        return context;
    }
}
