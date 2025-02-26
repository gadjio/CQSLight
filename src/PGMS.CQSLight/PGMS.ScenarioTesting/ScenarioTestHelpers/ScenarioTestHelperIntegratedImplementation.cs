using Microsoft.EntityFrameworkCore;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Querying;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.ScenarioTesting.ScenarioTestHelpers;

public interface ICqsApiHelper
{
    void SendCommand(ICommand command, List<string>? roles);
    T ProcessQuery<T>(IQuery<T> query);
}

public class ScenarioTestHelperIntegratedImplementation<TContext> : IScenarioTestHelper where TContext : DbContext, IDbContext
{
    private readonly ICqsApiHelper cqsApiHelper;
    private readonly string connectionString;
    private readonly ContextFactory<TContext> contextFactory;

    public ScenarioTestHelperIntegratedImplementation(ICqsApiHelper cqsApiHelper, string connectionString, ContextFactory<TContext> contextFactory)
    {
        this.cqsApiHelper = cqsApiHelper;
        this.connectionString = connectionString;
        this.contextFactory = contextFactory;
    }

    public void SendCommand(ICommand command, List<string>? roles = null)
    {
        cqsApiHelper.SendCommand(command, roles);
    }

    public T ProcessQuery<T>(IQuery<T> query)
    {
        return cqsApiHelper.ProcessQuery(query);
    }

    private IEntityRepository? entityRepository;
    public IEntityRepository GetEntityRepository()
    {
        if (entityRepository == null)
        {
            entityRepository = new BaseEntityRepository<TContext>(new ConnectionStringProvider(connectionString), contextFactory);
        }
        return entityRepository;
    }

    public override string ToString() => $"Integrated";
}