using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public interface IScenarioTestAction
{
    public void Run(IScenarioTestHelper scenarioTestHelper);
    string GetDescription();
}