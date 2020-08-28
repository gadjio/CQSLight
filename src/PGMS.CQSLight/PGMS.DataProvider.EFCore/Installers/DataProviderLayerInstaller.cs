using System.Reflection;
using Autofac;
using PGMS.Data.Installers;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.DataProvider.EFCore.Installers
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


        public static void RegisterContext<TDbContext>(ContainerBuilder builder, string connectionString, ContextFactory<TDbContext> contextFactory, string appName = "Default", string schema = null) 
	        where TDbContext : BaseDbContext
        {
	        var entityRepository = new BaseEntityRepository<TDbContext>(new ConnectionStringProvider(connectionString), contextFactory);
	        builder.Register(c => entityRepository).As<IUnitOfWorkProvider>().SingleInstance();
	        builder.Register(c => entityRepository).As<IEntityRepository>().SingleInstance();
	        builder.Register(c => entityRepository).As<IScopedEntityRepository>().SingleInstance();
	        builder.Register(c => new DataService<TDbContext>(entityRepository, appName, schema)).As<IDataService>().SingleInstance();
        }

        public static void RegisterContext<TDbContext, TDataService>(ContainerBuilder builder, string connectionString, ContextFactory<TDbContext> contextFactory, string appName = "Default", string schema = null) 
	        where TDbContext : BaseDbContext
            where TDataService : IDataService
        {
	        var entityRepository = new BaseEntityRepository<TDbContext>(new ConnectionStringProvider(connectionString), contextFactory);
	        builder.Register(c => entityRepository).As<IUnitOfWorkProvider>().SingleInstance();
	        builder.Register(c => entityRepository).As<IEntityRepository>().SingleInstance();
	        builder.Register(c => entityRepository).As<IScopedEntityRepository>().SingleInstance();
	        builder.Register(c => new DataService<TDbContext>(entityRepository, appName, schema)).As<TDataService>().SingleInstance();
        }
    }
}