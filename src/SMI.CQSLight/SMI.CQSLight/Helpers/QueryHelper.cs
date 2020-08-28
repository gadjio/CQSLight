using SMI.CQSLight.Infra.Querying;
using SMI.CQSLight.Infra.Querying.Services;

namespace SMI.CQSLight.Helpers
{
	public class QueryHelper
	{
		private readonly IQueryProcessor queryProcessor;

		public QueryHelper(IQueryProcessor queryProcessor)
		{
			this.queryProcessor = queryProcessor;
		}

		public T Process<T>(IQuery<T> query)
		{
			return queryProcessor.Process(query);
		}

	}
}