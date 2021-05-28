using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using PGMS.CQSLight.Infra.Commands.Services;

namespace PGMS.CQSLight.Installers
{
	public class CqsLightInfraInstaller
	{
		public static void ConfigureServices(ContainerBuilder builder, IDirectBusConfigurationProvider config)
		{
			builder.Register(c => config).As<IDirectBusConfigurationProvider>().SingleInstance();
			builder.RegisterAssemblyTypes(typeof(CqsLightInfraInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service") || (t.Namespace != null && t.Namespace.Contains(".Services")))
				.AsImplementedInterfaces();
		}

		public static void ConfigureServices(ContainerBuilder builder, bool useAutoFlush = false)
		{
			var config = new DirectBusConfigurationProvider(useAutoFlush);
			builder.Register(c => config).As<IDirectBusConfigurationProvider>().SingleInstance();
			builder.RegisterAssemblyTypes(typeof(CqsLightInfraInstaller).GetTypeInfo().Assembly)
				.Where(t => t.Name.EndsWith("Service") || (t.Namespace != null && t.Namespace.Contains(".Services")))
				.AsImplementedInterfaces();
		}
	}

	public class DirectBusConfigurationProvider : IDirectBusConfigurationProvider
	{
		public DirectBusConfigurationProvider(bool useAutoFlush)
		{
			UseAutoFlush = useAutoFlush;
		}

		public bool UseAutoFlush { get; }
	}
}