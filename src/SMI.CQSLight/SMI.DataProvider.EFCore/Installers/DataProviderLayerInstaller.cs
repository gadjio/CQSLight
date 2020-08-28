using System.Reflection;
using Autofac;
using SMI.Data.Installers;
using SMI.Data.Services;
using SMI.DataProvider.EFCore.Contexts;
using SMI.DataProvider.EFCore.Services;

namespace SMI.DataProvider.EFCore.Installers
{
    public class DataProviderLayerInstaller
    {
        public static void ConfigureServices(ContainerBuilder builder)
        {
	        builder.RegisterAssemblyTypes(typeof(DataProviderLayerInstaller).GetTypeInfo().Assembly)
                .Where(t => t.Namespace.Contains(".Services"))
                .AsImplementedInterfaces();


            DataLayerInstaller.ConfigureServices(builder);
        }


        public static void RegisterContext<TDbContext>(ContainerBuilder builder, string connectionString, ContextFactory<TDbContext> contextFactory) where TDbContext : BaseDbContext
        {
	        var entityRepository = new BaseEntityRepository<TDbContext>(new ConnectionStringProvider(connectionString), contextFactory);
	        builder.Register(c => entityRepository).As<IUnitOfWorkProvider>().SingleInstance();
	        builder.Register(c => entityRepository).As<IEntityRepository>().SingleInstance();
	        builder.Register(c => entityRepository).As<IScopedEntityRepository>().SingleInstance();
	        builder.Register(c => new DataService<TDbContext>(entityRepository)).As<IDataService>().SingleInstance();
        }

        //TODO RegisterContextPerWebRequest if needed - Not converted yet
        //private static void RegisterContextPerWebRequest(ContainerBuilder builder, string connectionString)
        //{
        //	builder.Register(c => new ConnectionStringProvider(connectionString)).As<IConnectionStringProvider>().SingleInstance();
        //	builder.RegisterType<WorkplanManagerEntityRepository>().As<IScopedEntityRepository>().InstancePerLifetimeScope();
        //	builder.RegisterType<WorkplanManagerEntityRepository>().As<IEntityRepository>().InstancePerLifetimeScope();

        //	var workplanManagerContextFactory = new WorkplanManagerContextFactory();
        //	workplanManagerContextFactory.InitContextUsage(false);

        //	var entityRepository = new WorkplanManagerEntityRepository(new ConnectionStringProvider(connectionString), workplanManagerContextFactory);
        //	builder.Register(c => new WorkplanManagerService(entityRepository)).As<IDataService>().SingleInstance();
        //}


    }
}