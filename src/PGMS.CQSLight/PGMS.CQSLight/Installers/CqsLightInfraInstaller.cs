using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;

namespace PGMS.CQSLight.Installers
{
	public class CqsLightInfraInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(CqsLightInfraInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service") || (t.Namespace != null && t.Namespace.Contains(".Services")))
				.AsImplementedInterfaces();
		}
	}
}