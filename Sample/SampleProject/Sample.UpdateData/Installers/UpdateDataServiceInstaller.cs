using System.Reflection;
using Autofac;

namespace Sample.UpdateData.Installers
{
	public class UpdateDataServiceInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(UpdateDataServiceInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service") || (t.Namespace != null && t.Namespace.Contains(".Services")))
				.AsImplementedInterfaces();


			builder.RegisterAssemblyTypes(typeof(UpdateDataServiceInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Namespace.Contains(".UpdateDataServices."))
				.AsImplementedInterfaces();
		}
	}
}