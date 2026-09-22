using System;

namespace PGMS.CQSLight.Infra.Querying;

/// <summary>
/// Identifies the trusted IContextInfo property used for query role validation.
/// Without this attribute, ContextInfo and UserContextInfo are recognized by convention.
/// Never populate a caller context from unvalidated request JSON.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class QueryContextAttribute : Attribute
{
}
