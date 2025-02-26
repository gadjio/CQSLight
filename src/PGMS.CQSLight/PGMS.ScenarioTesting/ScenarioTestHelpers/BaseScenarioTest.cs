using NUnit.Framework;
using NUnit.Framework.Internal;
using PGMS.ScenarioTesting.ScenarioTestActions;

namespace PGMS.ScenarioTesting.ScenarioTestHelpers;

public abstract class BaseScenarioTest
{
    protected abstract IScenarioTestHelper ScenarioTestHelper { get; }
    public abstract List<IScenarioTestAction> Givens();
    public abstract List<Tuple<string, IScenarioTestAction>> GetActions();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Console.WriteLine("Givens");
        var step = 1;
        foreach (var given in Givens())
        {
            var start = DateTime.Now;
            Console.Write($@"Given {step} - {given.GetDescription()}");
            RunScenarioTestAction(given);

            var elapsed = DateTime.Now - start;
            Console.WriteLine($" : COMPLETED - {(int)elapsed.TotalMilliseconds} ms");
            step++;
        }
        Console.WriteLine("");
    }

    [Test, Order(1)]
    public void RunScenarioTest()
    {
        Console.WriteLine("Actions");
        var step = 1;
        foreach (var tuple in GetActions())
        {
            var start = DateTime.Now;
            Console.Write($@"Step {step} {tuple.Item1} - {tuple.Item2.GetDescription()}");
            RunScenarioTestAction(tuple.Item2);

            var elapsed = DateTime.Now - start;
            Console.WriteLine($" : COMPLETED - {(int)elapsed.TotalMilliseconds} ms");
            step++;
        }
        Console.WriteLine("");
        Console.WriteLine("Scenario COMPLETED");
    }

    protected void RunScenarioTestAction(IScenarioTestAction scenarioTestAction)
    {
        scenarioTestAction.Run(ScenarioTestHelper);
    }
}

public class AfterScenarioValidationAttribute : TestAttribute
{
    private int Order = 2;

    public new void ApplyToTest(Test test)
    {
        base.ApplyToTest(test);
        test.Properties.Set(PropertyNames.Order, Order);
    }
}