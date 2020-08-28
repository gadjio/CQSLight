using System.Reflection;
using Autofac;

namespace Sample.Core.Installers
{
	public class ServiceLayerInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder)
		{
			PGMS.CQSLight.Installers.CqsLightInfraInstaller.ConfigureServices(builder);

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service") || (t.Namespace != null && t.Namespace.Contains(".Services")))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains(".Repositories"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains(".QueryHandlers"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains(".CommandHandlers"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains(".EventHandlers"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(ServiceLayerInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace != null && t.Namespace.Contains(".ApiCachingQueryHandlers"))
				.AsImplementedInterfaces();
		}
	}
}