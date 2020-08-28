using System;
using Autofac;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Querying.Services
{
    public interface IQueryProcessor
    {
        TResult Process<TResult>(IQuery<TResult> query);
		TResult ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds);

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
            var handlerType = typeof(IHandleQuery<,>).MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = context.Resolve(handlerType);

            return handler.Handle((dynamic)query);
        }

		public TResult ProcessWithCaching<TResult>(IQuery<TResult> query, int cacheDurationInSeconds)
		{					
			var cached = cacheRepository.Get(query);
			if (cached != null)
			{
				return (TResult)cached;
			}

			var result = Process(query);
			cacheRepository.Set(query, result, DateTime.Now.AddSeconds(cacheDurationInSeconds));

			return result;
		}
	}	
}