using System.Reflection;
using Autofac;

namespace SMI.Data.Installers
{
	public class DataLayerInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(DataLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(DataLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsImplementedInterfaces();

		}
    }
}