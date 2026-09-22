using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace PGMS.CQSLight.Infra.Security;

/// <summary>Allows a command or query when the caller has at least one declared role (case-insensitive).</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowedRolesAttribute : Attribute
{
    public string[] AllowedRoles { get; }

    public AllowedRolesAttribute(string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

public static class AllowedRolesHelper
{
    public static List<string> GetAllowedRoles<T>()
    {
        var t = typeof(T);
        return GetAllowedRoles(t);
    }

    public static List<string> GetAllowedRoles(Type t)
    {
        var allowedRolesAttribute = t.GetCustomAttribute(typeof(AllowedRolesAttribute));
        if (allowedRolesAttribute != null)
        {
            var attribute = allowedRolesAttribute as AllowedRolesAttribute;
            return attribute.AllowedRoles.ToList();
        }

        return null;
    }
}