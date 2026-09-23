using System;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using PGMS.CQSLight.Infra.Commands;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Querying.Services
{
	public interface IQueryProcessor
    {
        TResult Process<TResult>(IQuery<TResult> query);
		TResult ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds);

		Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query);
		Task<TResult> ProcessWithCachingAsync<TResult>(IQuery<TResult> query, int cacheDurationInSeconds);

        // Default implementations preserve existing custom processors. Context-aware caching
        // must be implemented explicitly; falling back to a query-only cache could mix users.
        TResult Process<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return Process(query);
        }

        TResult ProcessWithCaching<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds) =>
            Process(query, contextInfo);

        async Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await ProcessAsync(query);
        }

        Task<TResult> ProcessWithCachingAsync<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds) =>
            ProcessAsync(query, contextInfo);

	}

    public class QueryProcessor : IQueryProcessor
    {
        private readonly IComponentContext context;

		private readonly LocalScopeCacheRepository<object, object> cacheRepository = new LocalScopeCacheRepository<object, object>();


		public QueryProcessor(IComponentContext context)
        {
            this.context = context;            
        }

        public TResult Process<TResult>(IQuery<TResult> query)
        {
            QueryRoleValidator.Validate(query);
            return ProcessCore(query);
        }

        public TResult Process<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return ProcessCore(query);
        }

        private TResult ProcessCore<TResult>(IQuery<TResult> query)
        {
	        var asyncHandlerType = typeof(IHandleQueryAsync<,>).MakeGenericType(query.GetType(), typeof(TResult));
	        dynamic asyncHandler;
	        if (context.TryResolve(asyncHandlerType, out asyncHandler))
	        {
		        var request = asyncHandler.Handle((dynamic)query);
		        request.Wait();
		        return request.Result;
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

		public TResult ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds)
		{
            var caller = QueryRoleValidator.Validate(query);
            return ProcessWithCachingCore(query, caller, cacheDurationInSeconds);
        }

        public TResult ProcessWithCaching<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return ProcessWithCachingCore(query, contextInfo, cacheDurationInSeconds);
        }

        private TResult ProcessWithCachingCore<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            var cacheKey = JsonConvert.SerializeObject(new { Query = query, ContextInfo = contextInfo, QueryType = query.GetType().AssemblyQualifiedName });
			var cached = cacheRepository.Get(cacheKey);
			if (cached != null)
			{
				return (TResult)cached;
			}

			var result = ProcessCore(query);
			cacheRepository.Set(cacheKey, result, DateTime.Now.AddSeconds(cacheDurationInSeconds));

			return result;
		}

		public async Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query)
		{
            QueryRoleValidator.Validate(query);
            return await ProcessAsyncCore(query);
        }

        public async Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query, IContextInfo contextInfo)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await ProcessAsyncCore(query);
        }

        private async Task<TResult> ProcessAsyncCore<TResult>(IQuery<TResult> query)
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


		public async Task<TResult> ProcessWithCachingAsync<TResult>(IQuery<TResult> query, int cacheDurationInSeconds)
		{
            var caller = QueryRoleValidator.Validate(query);
            return await ProcessWithCachingAsyncCore(query, caller, cacheDurationInSeconds);
        }

        public async Task<TResult> ProcessWithCachingAsync<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            QueryRoleValidator.Validate(query, contextInfo);
            return await ProcessWithCachingAsyncCore(query, contextInfo, cacheDurationInSeconds);
        }

        private async Task<TResult> ProcessWithCachingAsyncCore<TResult>(IQuery<TResult> query, IContextInfo contextInfo, int cacheDurationInSeconds)
        {
            var cacheKey = JsonConvert.SerializeObject(new { Query = query, ContextInfo = contextInfo, QueryType = query.GetType().AssemblyQualifiedName });
			var cached = cacheRepository.Get(cacheKey);
			if (cached != null)
			{
				return (TResult)cached;
			}

			var result = await ProcessAsyncCore(query);
			cacheRepository.Set(cacheKey, result, DateTime.Now.AddSeconds(cacheDurationInSeconds));

			return result;
		}
	}	
}