using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Querying.Services;
using PGMS.UnitTests.CQSLight.Infra.Querying.QueryProcessorFixtures.FakeQueries;

namespace PGMS.UnitTests.CQSLight.Infra.Querying.QueryProcessorFixtures
{
	[TestFixture]
	public class QueryProcessorResolveHandlerFixture
	{
		private IContainer container;

		[SetUp]
		public void SetUp()
		{
			var builder = new ContainerBuilder();

			builder.RegisterAssemblyTypes(typeof(QueryProcessorResolveHandlerFixture).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains("PGMS.UnitTests.CQSLight.Infra.Querying.QueryProcessorFixtures.FakeQueries"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(QueryProcessor).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains("PGMS.CQSLight.Infra.Querying.Services"))
				.AsImplementedInterfaces();

			container = builder.Build();
		}

		[Test]
		public void Handler()
		{
			var queryProcessor = container.Resolve<IQueryProcessor>();

			var result = queryProcessor.Process(new TestQuery());

			Assert.That(result, Is.EqualTo(47));
		}


		[Test]
		public void AsyncHandler()
		{
			var queryProcessor = container.Resolve<IQueryProcessor>();

			var result = queryProcessor.Process(new TestAsyncQuery());
			

			Assert.That(result, Is.EqualTo(147));
		}

		[Test]
		public async Task AsyncEnumHandler()
		{
			var queryProcessor = container.Resolve<IQueryProcessor>();

			var result = queryProcessor.Process(new TestAsyncEnumQuery());

			int expectedStart = 1047;
			await foreach (var i in result)
			{
				Assert.That(i, Is.EqualTo(expectedStart));
				expectedStart++;
			}

			Assert.That(expectedStart, Is.EqualTo(1052));
		}
	}
}