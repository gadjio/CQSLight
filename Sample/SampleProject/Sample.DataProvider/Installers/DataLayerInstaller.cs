using Autofac;
using PGMS.DataProvider.EFCore.Installers;
using Sample.DataProvider.Services;

namespace Sample.DataProvider.Installers
{
	public class DataLayerInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder, string connectionString)
		{
			var floorPlanContextFactory = new SampleContextFactory();
			floorPlanContextFactory.InitContextUsage(false);

			DataProviderLayerInstaller.ConfigureServices(builder);
			DataProviderLayerInstaller.RegisterContext(builder, connectionString, floorPlanContextFactory);
		}
    }
}