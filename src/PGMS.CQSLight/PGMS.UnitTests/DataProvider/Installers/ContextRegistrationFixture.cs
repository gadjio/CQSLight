using Autofac;
using NUnit.Framework;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Installers;
using PGMS.FakeImpl.DataProvider.Context;

namespace PGMS.UnitTests.DataProvider.Installers
{
	[TestFixture]
	public class ContextRegistrationFixture
	{
		[Test]
		public void Test()
		{
			var contextFactory = new FakeContextFactory();

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