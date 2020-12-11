using System.Collections.Generic;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Querying;

namespace PGMS.UnitTests.CQSLight.Infra.Querying.QueryProcessorFixtures.FakeQueries
{
	public class TestQuery : IQuery<int>
	{
	}

	public class TestQueryHandler : IHandleQuery<TestQuery, int>
	{
		public int Handle(TestQuery query)
		{
			return 47;
		}
	}

	public class TestAsyncQuery : IQuery<int>
	{
	}

	public class TestAsyncQueryHandler : IHandleQueryAsync<TestAsyncQuery, int>
	{
		public async Task<int> Handle(TestAsyncQuery query)
		{
			return 147;
		}
	}

	public class TestAsyncEnumQuery : IQuery<IAsyncEnumerable<int>>
	{
	}

	public class TestAsyncEnumQueryHandler : IHandleQueryAsyncEnumerable<TestAsyncEnumQuery, IAsyncEnumerable<int>>
	{
		public async IAsyncEnumerable<int> Handle(TestAsyncEnumQuery query)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return 1047 + i;
			}
		}
	}
}