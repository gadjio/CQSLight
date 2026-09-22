using System;
using System.Collections.Generic;

namespace PGMS.CQSLight.Infra.Exceptions;

public class DomainSecurityValidationException : Exception
{
    public List<string> AllowedRoles { get; }
    public Type CommandType { get; }
    /// <summary>The rejected command or query type. CommandType remains for compatibility.</summary>
    public Type OperationType => CommandType;

    public DomainSecurityValidationException(List<string> allowedRoles, Type commandType, string message) : base(message)
    {
        AllowedRoles = allowedRoles;
        CommandType = commandType;
    }
}