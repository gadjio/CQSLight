using Autofac;
using PGMS.DataProvider.EFCore.Installers;
using Sample.DataProvider.Services;

namespace Sample.DataProvider.Installers
{
	public class DataLayerInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder, string connectionString)
		{
			var sampleContextFactory = new SampleContextFactory();

			DataProviderLayerInstaller.ConfigureServices(builder);
			DataProviderLayerInstaller.RegisterContext(builder, connectionString, sampleContextFactory);
		}
    }
}