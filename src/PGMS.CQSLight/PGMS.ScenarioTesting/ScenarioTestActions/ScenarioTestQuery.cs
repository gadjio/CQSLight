using PGMS.CQSLight.Infra.Querying;
using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public class ScenarioTestQuery<T, TQueryResult> : IScenarioTestAction where T : IQuery<TQueryResult>
{
    private T Query { get; }
    private readonly Action<TQueryResult> validation;

    public ScenarioTestQuery(T query, Action<TQueryResult> validation)
    {
        Query = query;
        this.validation = validation;
    }

    public void Run(IScenarioTestHelper scenarioTestHelper)
    {
        var result = scenarioTestHelper.ProcessQuery(Query);
        validation?.Invoke(result);
    }

   

    public string GetDescription()
    {
        return typeof(T).Name;
    }
}