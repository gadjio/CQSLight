using System;
using System.Threading.Tasks;
using Autofac;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Querying.Services
{
	public interface IQueryProcessorAsync
	{
		Task<TResult> Process<TResult>(IQuery<TResult> query);
		Task<TResult> ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds);
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
			var cached = cacheRepository.Get(query);
			if (cached != null)
			{
				return (TResult)cached;
			}

			var result = await Process(query);
			cacheRepository.Set(query, result, DateTime.Now.AddSeconds(cacheDurationInSeconds));

			return result;
		}
	}
}