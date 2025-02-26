using PGMS.Data.Services;
using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public class ScenarioTestDataValidation : IScenarioTestAction
{
    private readonly Action<IEntityRepository> validation;

    public ScenarioTestDataValidation(Action<IEntityRepository> validation)
    {
        this.validation = validation;
    }

    public void Run(IScenarioTestHelper scenarioTestHelper)
    {
        validation.Invoke(scenarioTestHelper.GetEntityRepository());
    }

    public string GetDescription()
    {
        return "Scenario Test Validation";
    }
}