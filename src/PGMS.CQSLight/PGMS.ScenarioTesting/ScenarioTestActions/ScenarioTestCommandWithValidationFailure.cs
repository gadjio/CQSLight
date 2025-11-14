using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.Data.Services;
using PGMS.ScenarioTesting.ScenarioTestHelpers;

namespace PGMS.ScenarioTesting.ScenarioTestActions;

public class ScenarioTestCommandWithValidationFailure<T> : ScenarioTestCommand<T>, IScenarioTestAction where T : ICommand
{
    private readonly Action<DomainValidationException> validation;
    private readonly bool failWhenNoException;

    public ScenarioTestCommandWithValidationFailure(T command, Action<DomainValidationException> validation, bool failWhenNoException = true) : base(command)
    {
        this.validation = validation;
        this.failWhenNoException = failWhenNoException;
    }

    public ScenarioTestCommandWithValidationFailure(T command, Func<T, IEntityRepository, bool> isNeeded, Action<DomainValidationException> validation, bool failWhenNoException = true) : base(command, isNeeded)
    {
        this.validation = validation;
    }

    public override void Run(IScenarioTestHelper scenarioTestHelper)
    {
        try
        {
            base.Run(scenarioTestHelper);
        }
        catch (DomainValidationException e)
        {
            validation?.Invoke(e);
            return;
        }

        if (failWhenNoException)
        {
            throw new Exception($"DomainValidationException was expected but no exception was thrown during {nameof(Run)}. " +
                                $"Case: {this.GetDescription()}, " +
                                $"Scenario: {scenarioTestHelper?.GetType().Name ?? "unknown"}, " +
                                $"FailWhenNoException: {failWhenNoException}");
        }
    }
}