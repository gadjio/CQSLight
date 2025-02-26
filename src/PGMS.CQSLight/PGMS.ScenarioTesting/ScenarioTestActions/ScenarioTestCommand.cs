using Newtonsoft.Json;
using PGMS.CQSLight.Extensions;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.Data.Services;
using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public class ScenarioTestCommand<T> : IScenarioTestAction where T : ICommand
{
    private T Command { get; }
    private Func<T, IEntityRepository, bool>? IsNeeded { get; }


    public ScenarioTestCommand(T command)
    {
        Command = command;
    }

    public ScenarioTestCommand(T command, Func<T, IEntityRepository, bool> isNeeded)
    {
        Command = command;
        IsNeeded = isNeeded;
    }

    public void Run(IScenarioTestHelper scenarioTestHelper)
    {
        if (IsNeeded != null)
        {
            if (!IsNeeded(Command, scenarioTestHelper.GetEntityRepository()))
            {
                Console.WriteLine(@" No Need to run command");
                return;
            }
        }

        try
        {
            scenarioTestHelper.SendCommand(Command);
        }
        catch (DomainValidationException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(JsonConvert.SerializeObject(e.ValidationResult).JsonPrettify());
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }

    public string GetDescription()
    {
        return $"{typeof(T).Name} - '{Command.AggregateRootId}'";
    }
}