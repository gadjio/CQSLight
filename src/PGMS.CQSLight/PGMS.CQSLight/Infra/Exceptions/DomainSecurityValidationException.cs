using System;
using System.Collections.Generic;

namespace PGMS.CQSLight.Infra.Exceptions;

public class DomainSecurityValidationException : Exception
{
    public List<string> AllowedRoles { get; }
    public Type CommandType { get; }

    public DomainSecurityValidationException(List<string> allowedRoles, Type commandType, string message) : base(message)
    {
        AllowedRoles = allowedRoles;
        CommandType = commandType;
    }
}