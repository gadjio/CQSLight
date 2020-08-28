using System.Reflection;
using Autofac;
using Sample.Core.Infra.Configs;

namespace Sample.BlazorApp.Installers
{
	public class BlazorAppInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder, string connectionString, AppSettings appSettings)
		{
			builder.RegisterAssemblyTypes(typeof(BlazorAppInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service"))
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(BlazorAppInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsImplementedInterfaces();


			var environment = appSettings.Environment;
		}
	}
}