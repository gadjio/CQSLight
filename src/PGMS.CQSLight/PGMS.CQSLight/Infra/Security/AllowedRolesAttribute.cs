using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace PGMS.CQSLight.Infra.Security;

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
        var allowedRolesAttribute = t.GetCustomAttribute(typeof(AllowedRolesAttribute));
        if (allowedRolesAttribute != null)
        {
            var attribute = allowedRolesAttribute as AllowedRolesAttribute;
            return attribute.AllowedRoles.ToList();
        }

        return null;
    }
}