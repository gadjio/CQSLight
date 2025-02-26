using PGMS.Data.Services;
using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public class ScenarioTestDataManipulation : IScenarioTestAction
{
    private readonly Action<IEntityRepository> validation;

    public ScenarioTestDataManipulation(Action<IEntityRepository> validation)
    {
        this.validation = validation;
    }

    public void Run(IScenarioTestHelper scenarioTestHelper)
    {
        validation.Invoke(scenarioTestHelper.GetEntityRepository());
    }

    public string GetDescription()
    {
        return "Scenario Test Data Manipulation";
    }
}