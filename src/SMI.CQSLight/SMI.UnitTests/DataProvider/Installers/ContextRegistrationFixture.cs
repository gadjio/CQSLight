using Autofac;
using NUnit.Framework;
using SMI.Data.Services;
using SMI.DataProvider.EFCore.Installers;
using SMI.FakeImpl.DataProvider.Context;

namespace SMI.UnitTests.DataProvider.Installers
{
	[TestFixture]
	public class ContextRegistrationFixture
	{
		[Test]
		public void Test()
		{
			var contextFactory = new FakeContextFactory();
			contextFactory.InitContextUsage(false);

			var connectionString = "Fake";

			var builder = new ContainerBuilder();

			//Act
			DataProviderLayerInstaller.ConfigureServices(builder);
			DataProviderLayerInstaller.RegisterContext(builder, connectionString, contextFactory);

			var container = builder.Build();

			//Assert
			Assert.That(container.Resolve<IUnitOfWorkProvider>(), Is.Not.Null);
			Assert.That(container.Resolve<IEntityRepository>(), Is.Not.Null);
			Assert.That(container.Resolve<IScopedEntityRepository>(), Is.Not.Null);
		}
	}
}