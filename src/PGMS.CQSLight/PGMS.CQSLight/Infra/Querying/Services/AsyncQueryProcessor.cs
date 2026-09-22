using System;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using PGMS.CQSLight.Infra.Commands;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Querying.Services
{
	public interface IQueryProcessorAsync
	{
		Task<TResult> Process<TResult>(IQuery<TResult> query);
		Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds);

        async Task<TResult> Process<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await Process(query);
        }

        // Custom implementations retain compatibility without sharing query-only caches across callers.
        Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds) =>
            Process(query, contextInfo);
	}

	public class QueryProcessorAsync : IQueryProcessorAsync
	{
		private readonly IComponentContext context;

		private readonly LocalScopeCacheRepository<object, object> cacheRepository = new LocalScopeCacheRepository<object, object>();


		public QueryProcessorAsync(IComponentContext context)
		{
			this.context = context;
		}

		public async Task<TResult> Process<TResult>(IQuery<TResult> query)
		{
            QueryRoleValidator.Validate(query);
            return await ProcessCore(query);
        }

        public async Task<TResult> Process<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await ProcessCore(query);
        }

        private async Task<TResult> ProcessCore<TResult>(IQuery<TResult> query)
        {
			var asyncHandlerType = typeof(IHandleQueryAsync<,>).MakeGenericType(query.GetType(), typeof(TResult));
			dynamic asyncHandler;
			if (context.TryResolve(asyncHandlerType, out asyncHandler))
			{
				return await asyncHandler.Handle((dynamic)query);
			}

			var asyncEnumeratorHandlerType = typeof(IHandleQueryAsyncEnumerable<,>).MakeGenericType(query.GetType(), typeof(TResult));
			dynamic asyncEnumHandler;
			if (context.TryResolve(asyncEnumeratorHandlerType, out asyncEnumHandler))
			{
				return asyncEnumHandler.Handle((dynamic)query);
			}

			var handlerType = typeof(IHandleQuery<,>).MakeGenericType(query.GetType(), typeof(TResult));
			dynamic handler = context.Resolve(handlerType);

			return handler.Handle((dynamic)query);
		}


		public async Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds)
		{
            var caller = QueryRoleValidator.Validate(query);
            return await ProcessWithCachingCore(query, caller, cacheDurationInSeconds);
        }

        public async Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await ProcessWithCachingCore(query, contextInfo, cacheDurationInSeconds);
        }

        private async Task<TResult> ProcessWithCachingCore<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            var cacheKey = JsonConvert.SerializeObject(new { Query = query, ContextInfo = contextInfo, QueryType = query.GetType().AssemblyQualifiedName });
			var cached = cacheRepository.Get(cacheKey);
			if (cached != null)
			{
				return (TResult)cached;
			}

			var result = await ProcessCore(query);
			cacheRepository.Set(cacheKey, result, DateTime.Now.AddSeconds(cacheDurationInSeconds));

			return result;
		}
	}
}