using Moq;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.ScenarioTesting.ScenarioTestActions;
using PGMS.ScenarioTesting.ScenarioTestHelpers;
using System;
using PGMS.CQSLight.Infra.Exceptions;

namespace PGMS.UnitTests.ScenarioTesting.ScenarioTestActions;

[TestFixture]
public class ScenarioTestCommandWithValidationFailureFixture
{
    [Test]
    public void Test()
    {
        var scenarioTestHelperMock = new Mock<IScenarioTestHelper>();
        scenarioTestHelperMock.Setup(x => x.SendCommand(It.IsAny<FakeCommand1>(), null)).Throws(new DomainValidationException("On purpose"));
        
        Exception caughtException = null;
        try
        {
            var sut = new ScenarioTestCommandWithValidationFailure<FakeCommand1>(
                new FakeCommand1(),
                domainValidationException =>
                {
                    Assert.That(domainValidationException.Message, Is.EqualTo("On purpose"));
                });
            
            sut.Run(scenarioTestHelperMock.Object);
        }
        catch (Exception e)
        {
            caughtException = e;
            Console.WriteLine(e);
        }
       
        Assert.That(caughtException, Is.Null);
    }

    [Test]
    public void When_Validation_Is_null()
    {
        var scenarioTestHelperMock = new Mock<IScenarioTestHelper>();
        scenarioTestHelperMock.Setup(x => x.SendCommand(It.IsAny<FakeCommand1>(), null)).Throws(new DomainValidationException("On purpose"));

        Exception caughtException = null;
        try
        {
            var sut = new ScenarioTestCommandWithValidationFailure<FakeCommand1>(
                new FakeCommand1(),
                domainValidationException =>
                {
                    Assert.That(domainValidationException.Message, Is.EqualTo("On purpose"));
                });

            sut.Run(scenarioTestHelperMock.Object);
        }
        catch (Exception e)
        {
            caughtException = e;
            Console.WriteLine(e);
        }

        Assert.That(caughtException, Is.Null);
    }

    [Test]
    public void When_Validation_Is_not_Thrown_Expect_fail()
    {
        var scenarioTestHelperMock = new Mock<IScenarioTestHelper>();
        //scenarioTestHelperMock.Setup(x => x.SendCommand(It.IsAny<FakeCommand1>(), null)).Throws(new DomainValidationException("On purpose"));

        Exception caughtException = null;
        try
        {
            var sut = new ScenarioTestCommandWithValidationFailure<FakeCommand1>(
                new FakeCommand1(),
                null);
            sut.Run(scenarioTestHelperMock.Object);
        }
        catch (Exception e)
        {
            caughtException = e;
            Console.WriteLine(e);
        }

        Assert.That(caughtException, Is.Not.Null);
        Assert.That(caughtException.Message, Is.EqualTo("DomainValidationException was expected but no exception was thrown during Run. Case: FakeCommand1 - '00000000-0000-0000-0000-000000000000', Scenario: IScenarioTestHelperProxy, FailWhenNoException: True"));
    }

    [Test]
    public void When_Validation_Is_not_Thrown_FailWhenNoExceptionFalse_Expect_pass()
    {
        var scenarioTestHelperMock = new Mock<IScenarioTestHelper>();
        //scenarioTestHelperMock.Setup(x => x.SendCommand(It.IsAny<FakeCommand1>(), null)).Throws(new DomainValidationException("On purpose"));

        Exception caughtException = null;
        try
        {
            var sut = new ScenarioTestCommandWithValidationFailure<FakeCommand1>(
                new FakeCommand1(),
                null, false);
            sut.Run(scenarioTestHelperMock.Object);
        }
        catch (Exception e)
        {
            caughtException = e;
            Console.WriteLine(e);
        }

        Assert.That(caughtException, Is.Null);
    }
}

public class FakeCommand1 : ICommand
{
    public Guid Id { get; }
    public Guid AggregateRootId { get; set; }
}