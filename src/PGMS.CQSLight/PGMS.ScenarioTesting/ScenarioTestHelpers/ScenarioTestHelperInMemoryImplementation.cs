using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Querying;
using PGMS.CQSLight.Infra.Querying.Services;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.Data.Services;

namespace PGMS.ScenarioTesting.ScenarioTestHelpers;

public class ScenarioTestHelperInMemoryImplementation<TContext> : IScenarioTestHelper where TContext : DbContext
{
    
    private InMemoryReportingRepository<TContext> entityRepository;
    private IContainer container;
    private IBus bus;
    private IQueryProcessor queryProcessor;

    public ScenarioTestHelperInMemoryImplementation(TContext context, ContainerBuilder builder)
    {
        entityRepository = new InMemoryReportingRepository<TContext>(context);

        builder.Register(c => entityRepository).As<IEntityRepository>().SingleInstance();
        builder.Register(c => entityRepository).As<IScopedEntityRepository>().SingleInstance();
        builder.Register(c => new FakeDataService()).As<IDataService>().SingleInstance();

        builder.Register(c => new FakeUnitOfWorkProvider()).As<IUnitOfWorkProvider>().SingleInstance();

        builder.Register(c => new NullLogger<IBus>()).As<ILogger<IBus>>().SingleInstance();
        builder.Register(c => new NullLogger<IEvent>()).As<ILogger<IEvent>>().SingleInstance();
        
        builder.Register(c => new QueryProcessor(container)).As<IQueryProcessor>().SingleInstance();

        container = builder.Build();

        bus = container.Resolve<IBus>();
        queryProcessor = new QueryProcessor(container);
    }

    public IEntityRepository GetEntityRepository() => entityRepository;

    public virtual void SendCommand(ICommand command, List<string>? roles = null)
    {
        //if (command is BaseAuthTokenCommand baseAuthCommand)
        //{
        //    // To force FakeSecurityUserRepository - SetAuthToken to string.Empty
        //    if (baseAuthCommand.UserAuthToken == null)
        //    {
        //        baseAuthCommand.UserAuthToken = "";
        //    }
        //}

        var contextInfo = new ContextInfo()
        {
            UserRoles = roles,
            ByUserId = "1",
            ByUsername = "1",
            SkipRoleValidation = true
        };

        try
        {
            var task = bus.Send(command, contextInfo);
            task.Wait();
        }
        catch (Exception e)
        {
            if (e.InnerException != null)
            {
                throw e.InnerException;
            }
            throw;
        }

    }

    public T ProcessQuery<T>(IQuery<T> query)
    {
        //if (query is BaseAuthTokenQuery baseAuthTokenQuery)
        //{
        //    // To force FakeSecurityUserRepository - SetAuthToken to string.Empty
        //    if (baseAuthTokenQuery.UserAuthToken == null)
        //    {
        //        baseAuthTokenQuery.UserAuthToken = "";
        //    }
        //}

        var task = queryProcessor.ProcessAsync(query);
        task.Wait();
        return task.Result;
    }


    

    public T GetRegisteredService<T>()
    {
        return container.Resolve<T>();
    }

    public List<T> GetRegisteredServices<T>()
    {
        return container.ResolveAll(typeof(T)).Cast<T>().ToList();
    }

    public override string ToString() => $"InMemory";

}