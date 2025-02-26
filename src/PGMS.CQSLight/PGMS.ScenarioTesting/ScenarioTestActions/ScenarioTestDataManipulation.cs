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

public class ScenarioTestLocalDataManipulation : IScenarioTestAction
{
    private readonly Action manipulation;

    public ScenarioTestLocalDataManipulation(Action manipulation)
    {
        this.manipulation = manipulation;
    }

    public void Run(IScenarioTestHelper scenarioTestHelper)
    {
        manipulation.Invoke();
    }


    public string GetDescription()
    {
        return "Scenario Test Local Data Manipulation";
    }
}