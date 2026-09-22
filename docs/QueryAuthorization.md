# Query role authorization (versions 4 and 5)

Queries use the same `AllowedRolesAttribute`, `IContextInfo`, role matching and
`DomainSecurityValidationException` as commands. Built-in `QueryProcessor` and
`QueryProcessorAsync` validate the runtime query type before resolving a handler
or looking up a cached result. This applies to all synchronous, asynchronous and
async-enumerable handler dispatch paths.

## Declare roles and supply the caller

```csharp
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Querying;
using PGMS.CQSLight.Infra.Security;

[AllowedRoles(new[] { "Admin", "Manager" })]
public class EmployeeCountQuery : IQuery<int>
{
    public string DepartmentId { get; set; }
}

// Build this context on the server from the authenticated caller.
IContextInfo caller = new ContextInfo
{
    ByUserId = authenticatedUserId,
    UserRoles = authenticatedRoles
};
var count = await queryProcessor.ProcessAsync(query, caller);
var cachedCount = await queryProcessor.ProcessWithCachingAsync(query, caller, 60);
```

The caller needs **any one** declared role, matched case-insensitively. An empty
role declaration permits nobody unless a trusted caller explicitly sets
`SkipRoleValidation = true`, matching the command bypass convention. Null or
empty caller roles do not authorize a protected query.

Explicit-context overloads exist on both processor interfaces:

| Processor | Methods (context follows query) |
| --- | --- |
| `IQueryProcessor` | `Process(query, context)`, `ProcessAsync(query, context)` |
| `IQueryProcessor` | `ProcessWithCaching(query, context, seconds)`, `ProcessWithCachingAsync(query, context, seconds)` |
| `IQueryProcessorAsync` | `Process(query, context)`, `ProcessWithCaching(query, context, seconds)` |

## Context stored on the query

Existing calls such as `ProcessAsync(query)` can use a context property. Public
readable `IContextInfo` properties named `ContextInfo` or `UserContextInfo` are
recognized. For another name, or to select between multiple properties, use
`[QueryContext]`:

```csharp
[AllowedRoles(new[] { "Manager" })]
public class EmployeeCountQuery : IQuery<int>
{
    [QueryContext]
    public IContextInfo Caller { get; set; }
}

var query = new EmployeeCountQuery { Caller = trustedServerContext };
var count = await queryProcessor.ProcessAsync(query);
```

An explicitly passed context always takes precedence, even when it is null.
The processor does not copy it into the query. If a handler reads a query's
context for tenant or data filtering, the caller must populate that property
with the same trusted context. Never accept roles, identity or
`SkipRoleValidation` from unvalidated request JSON. Role authorization does not
replace the handler's row-level or tenant-level access checks.

## Compatibility and caching

- Queries without `[AllowedRoles]` retain their existing authorization behavior;
  their context properties are not inspected by the validator.
- **Migration:** a query with `[AllowedRoles]` now requires a context. Missing
  context is denied instead of silently skipping authorization. Commands retain
  their existing behavior.
- Inherited role declarations are honored. A malformed or ambiguous context
  property produces `InvalidOperationException`; missing permission produces
  `DomainSecurityValidationException`. Its `OperationType` identifies the query;
  the existing `CommandType` property remains available for compatibility.
- Every cached call rechecks roles. Built-in caches include the runtime query
  type, query and caller context in a serialized snapshot of the cache key. This
  separates users, roles and serializable fields of derived contexts (for
  example, tenant IDs), even if the query context property is `[JsonIgnore]`.
  Include every authorization-relevant context field in serialization when
  caching, or use uncached calls.
- Existing custom processor implementations still compile through default
  interface methods. New explicit-context calls validate before legacy
  dispatch. Default contextual caching overloads deliberately use uncached
  dispatch until the custom processor implements its own caller-aware cache.
  Custom implementations remain responsible for their original overloads;
  direct calls to a query handler bypass processor authorization.

No AI, HTTP, JWT or dependency on a product-specific context is required.
