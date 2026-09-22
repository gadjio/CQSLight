using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Infra.Querying;
using PGMS.CQSLight.Infra.Querying.Services;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.UnitTests.CQSLight.Infra.Querying;

[TestFixture]
public class QueryAuthorizationFixture
{
    public static readonly string[] Paths = { "sync", "sync-cache", "async", "async-cache", "async-processor", "async-processor-cache" };
    private IContainer container;
    private QueryProcessor processor;
    private QueryProcessorAsync asyncProcessor;
    private int resolutions;
    private int executions;

    [SetUp]
    public void SetUp() => Configure("sync");

    private void Configure(string handlerKind)
    {
        container?.Dispose();
        resolutions = executions = 0;
        var builder = new ContainerBuilder();
        if (handlerKind == "async")
            builder.Register(_ => { resolutions++; return new AsyncHandler(() => ++executions); })
                .As<IHandleQueryAsync<ProtectedQuery, int>>();
        else
        {
            var registration = builder.Register(_ => { resolutions++; return new Handler(() => ++executions); });
            if (handlerKind == "enumerable") registration.As<IHandleQueryAsyncEnumerable<ProtectedQuery, int>>();
            else registration.As<IHandleQuery<ProtectedQuery, int>>();
        }
        container = builder.Build();
        processor = new QueryProcessor(container);
        asyncProcessor = new QueryProcessorAsync(container);
    }

    [TearDown]
    public void TearDown() => container.Dispose();

    private Task<int> Invoke(string path, ProtectedQuery query, bool explicitContext, IContextInfo caller) => path switch
    {
        "sync" => Task.FromResult(explicitContext ? processor.Process(query, caller) : processor.Process(query)),
        "sync-cache" => Task.FromResult(explicitContext ? processor.ProcessWithCaching(query, caller, 60) : processor.ProcessWithCaching(query, 60)),
        "async" => explicitContext ? processor.ProcessAsync(query, caller) : processor.ProcessAsync(query),
        "async-cache" => explicitContext ? processor.ProcessWithCachingAsync(query, caller, 60) : processor.ProcessWithCachingAsync(query, 60),
        "async-processor" => explicitContext ? asyncProcessor.Process(query, caller) : asyncProcessor.Process(query),
        _ => explicitContext ? asyncProcessor.ProcessWithCaching(query, caller, 60) : asyncProcessor.ProcessWithCaching(query, 60)
    };

    private static ContextInfo Caller(string role = "Manager", string id = "alice") =>
        new ContextInfo { ByUserId = id, UserRoles = new List<string> { "Unrelated", role } };

    [Test, Combinatorial]
    public async Task Authorized_calls_use_existing_handler_resolution(
        [ValueSource(nameof(Paths))] string path, [Values] bool explicitContext,
        [Values("sync", "async", "enumerable")] string handlerKind)
    {
        Configure(handlerKind);
        var caller = Caller("mAnAgEr");
        Assert.That(await Invoke(path, new ProtectedQuery { ContextInfo = caller }, explicitContext, caller), Is.EqualTo(1));
    }

    [Test, Combinatorial]
    public void Denied_calls_never_resolve_a_handler([ValueSource(nameof(Paths))] string path, [Values] bool explicitContext)
    {
        var caller = Caller("Reader");
        var exception = Assert.ThrowsAsync<DomainSecurityValidationException>(async () =>
            await Invoke(path, new ProtectedQuery { ContextInfo = caller }, explicitContext, caller));
        Assert.Multiple(() =>
        {
            Assert.That(resolutions, Is.Zero);
            Assert.That(executions, Is.Zero);
            Assert.That(exception.OperationType, Is.EqualTo(typeof(ProtectedQuery)));
            Assert.That(exception.CommandType, Is.EqualTo(exception.OperationType));
            Assert.That(exception.AllowedRoles, Is.EquivalentTo(new[] { "Admin", "Manager" }));
        });
    }

    [Test, Combinatorial]
    public void Missing_context_is_denied([ValueSource(nameof(Paths))] string path, [Values] bool explicitContext)
    {
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () =>
            await Invoke(path, new ProtectedQuery(), explicitContext, null));
        Assert.That(resolutions, Is.Zero);
    }

    [Test, Combinatorial]
    public void Explicit_context_cannot_be_overridden_by_query_payload([ValueSource(nameof(Paths))] string path, [Values] bool missing)
    {
        var forged = Caller("Admin");
        forged.SkipRoleValidation = true;
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () =>
            await Invoke(path, new ProtectedQuery { ContextInfo = forged }, true, missing ? null : Caller("Reader")));
        Assert.That(resolutions, Is.Zero);
    }

    [Test, Combinatorial]
    public async Task Cached_results_still_require_current_roles(
        [Values("sync-cache", "async-cache", "async-processor-cache")] string path, [Values] bool explicitContext)
    {
        var caller = Caller();
        var query = new ProtectedQuery { ContextInfo = caller };
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(1));
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(1));
        caller.UserRoles.Clear();
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () => await Invoke(path, query, explicitContext, caller));
        Assert.That(executions, Is.EqualTo(1));
    }

    [Test]
    public async Task Legacy_custom_processors_keep_compiling_and_validate_new_overloads_without_reusing_legacy_cache()
    {
        IQueryProcessor legacy = new LegacyProcessor();
        IQueryProcessorAsync legacyAsync = new LegacyAsyncProcessor();
        var query = new ProtectedQuery();
        var caller = Caller();
        Assert.That(legacy.Process(query, caller), Is.EqualTo(42));
        Assert.That(legacy.ProcessWithCaching(query, caller, 60), Is.EqualTo(42));
        Assert.That(await legacy.ProcessAsync(query, caller), Is.EqualTo(42));
        Assert.That(await legacy.ProcessWithCachingAsync(query, caller, 60), Is.EqualTo(42));
        Assert.That(await legacyAsync.Process(query, caller), Is.EqualTo(42));
        Assert.That(await legacyAsync.ProcessWithCaching(query, caller, 60), Is.EqualTo(42));
        caller.UserRoles.Clear();
        Assert.Throws<DomainSecurityValidationException>(() => legacy.Process(query, caller));
        Assert.Throws<DomainSecurityValidationException>(() => legacy.ProcessWithCaching(query, caller, 60));
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () => await legacy.ProcessAsync(query, caller));
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () => await legacy.ProcessWithCachingAsync(query, caller, 60));
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () => await legacyAsync.Process(query, caller));
        Assert.ThrowsAsync<DomainSecurityValidationException>(async () => await legacyAsync.ProcessWithCaching(query, caller, 60));
    }

    [Test]
    public async Task Explicit_context_is_authoritative_even_when_query_context_is_denied()
    {
        foreach (var path in Paths)
            Assert.That(await Invoke(path, new ProtectedQuery { ContextInfo = Caller("Reader") }, true, Caller()), Is.GreaterThan(0));
    }

    [Test, Combinatorial]
    public async Task Cache_is_partitioned_by_user_and_derived_context(
        [Values("sync-cache", "async-cache", "async-processor-cache")] string path, [Values] bool explicitContext)
    {
        var query = new ProtectedQuery();
        var caller = new TenantContext { ByUserId = "alice", Tenant = "one", UserRoles = new List<string> { "Manager" } };
        query.ContextInfo = caller;
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(1));
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(1));
        caller.ByUserId = "bob";
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(2));
        caller.Tenant = "two";
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(3));
        Assert.That(await Invoke(path, query, explicitContext, caller), Is.EqualTo(3));
    }

    [Test]
    public void Supports_context_attribute_conventions_and_inherited_roles()
    {
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new NamedContextQuery { Caller = Caller() }));
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new UserContextQuery { UserContextInfo = Caller() }));
        ProtectedQuery derived = new DerivedQuery { ContextInfo = Caller() };
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(derived));
        derived.ContextInfo.UserRoles = null;
        Assert.Throws<DomainSecurityValidationException>(() => QueryRoleValidator.Validate(derived));
        Assert.Throws<DomainSecurityValidationException>(() => QueryRoleValidator.Validate(new EmptyRolesQuery(), Caller()));
    }

    [Test]
    public void Unannotated_queries_do_not_read_context_or_require_roles()
    {
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new OpenQuery()));
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new OpenQuery(), null));
    }

    [Test]
    public void Trusted_internal_callers_can_explicitly_skip_validation()
    {
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new ProtectedQuery(), new ContextInfo { SkipRoleValidation = true }));
    }

    [Test]
    public void Ambiguous_or_invalid_context_is_rejected_unless_explicitly_supplied()
    {
        Assert.Throws<InvalidOperationException>(() => QueryRoleValidator.Validate(new AmbiguousQuery()));
        Assert.Throws<InvalidOperationException>(() => QueryRoleValidator.Validate(new InvalidContextQuery()));
        Assert.DoesNotThrow(() => QueryRoleValidator.Validate(new AmbiguousQuery(), Caller()));
    }

    [AllowedRoles(new[] { "Admin", "Manager" })]
    public class ProtectedQuery : IQuery<int>
    {
        [JsonIgnore] public IContextInfo ContextInfo { get; set; }
    }
    public class DerivedQuery : ProtectedQuery { }
    public class TenantContext : ContextInfo { public string Tenant { get; set; } }
    [AllowedRoles(new[] { "Manager" })]
    public class NamedContextQuery : IQuery<int>
    {
        [QueryContext] public IContextInfo Caller { get; set; }
        public IContextInfo ContextInfo => throw new InvalidOperationException("Must use attributed property");
    }
    [AllowedRoles(new[] { "Manager" })]
    public class UserContextQuery : IQuery<int> { public IContextInfo UserContextInfo { get; set; } }
    [AllowedRoles(new string[0])]
    public class EmptyRolesQuery : IQuery<int> { }
    public class OpenQuery : IQuery<int> { public IContextInfo ContextInfo => throw new InvalidOperationException(); }
    public class AmbiguousQuery : ProtectedQuery { public IContextInfo UserContextInfo { get; set; } }
    [AllowedRoles(new[] { "Manager" })]
    public class InvalidContextQuery : IQuery<int> { [QueryContext] public string Caller { get; set; } }

    public class Handler : IHandleQuery<ProtectedQuery, int>, IHandleQueryAsyncEnumerable<ProtectedQuery, int>
    {
        private readonly Func<int> execute;
        public Handler(Func<int> execute) => this.execute = execute;
        public int Handle(ProtectedQuery query) => execute();
    }
    public class AsyncHandler : IHandleQueryAsync<ProtectedQuery, int>
    {
        private readonly Func<int> execute;
        public AsyncHandler(Func<int> execute) => this.execute = execute;
        public Task<int> Handle(ProtectedQuery query) => Task.FromResult(execute());
    }

    private class LegacyProcessor : IQueryProcessor
    {
        public TResult Process<TResult>(IQuery<TResult> query) => (TResult)(object)42;
        public Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query) => Task.FromResult(Process(query));
        public TResult ProcessWithCaching<TResult>(IQuery<TResult> query, int seconds) => throw new InvalidOperationException("Unsafe legacy cache");
        public Task<TResult> ProcessWithCachingAsync<TResult>(IQuery<TResult> query, int seconds) => throw new InvalidOperationException("Unsafe legacy cache");
    }

    private class LegacyAsyncProcessor : IQueryProcessorAsync
    {
        public Task<TResult> Process<TResult>(IQuery<TResult> query) => Task.FromResult((TResult)(object)42);
        public Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, int seconds) => throw new InvalidOperationException("Unsafe legacy cache");
    }
}
