using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;

namespace PGMS.UnitTests.CQSLight.Infra.Commands;

[TestFixture]
public class CommandHandlerAddToContextInfo
{
    private FakeBus bus;
    private MyFakeCommandHandler commandHandler;

    [SetUp]
    public void SetUp()
    {
        bus = new FakeBus();
        commandHandler = new MyFakeCommandHandler(bus);
        
    }

    [Test]
    public void Test()
    {
        var cmd = new MyFakeCommand();
        var myContext = new MyCustomContextInfo
        {
            ByUsername = "PrimeUser",
            UserFullName = "Optimus Prime"
        };
        //Act
        var executeCommandTask = commandHandler.Execute(cmd, myContext);
        executeCommandTask.Wait();

        //Assert
        var @event = bus.PublishedEvents[0];
        Assert.That(@event, Is.TypeOf<MyFakeEvent>());
        var myFakeEvent = (@event as MyFakeEvent);
        Assert.That(myFakeEvent, Is.Not.Null);
        Assert.That(myFakeEvent.UserFullName, Is.EqualTo("Optimus Prime"));

        Assert.That(bus.PublishedEvents.Count, Is.EqualTo(1));
    }

    [Test]
    public void Retrocompatibility_RegularContextInfo()
    {
        var cmd = new MyFakeCommand();
        var myContext = new ContextInfo
        {
            ByUsername = "PrimeUser",
        };
        //Act
        var executeCommandTask = commandHandler.Execute(cmd, myContext);
        executeCommandTask.Wait();

        //Assert
        var @event = bus.PublishedEvents[0];
        Assert.That(@event, Is.TypeOf<MyFakeEvent>());
        var myFakeEvent = (@event as MyFakeEvent);
        Assert.That(myFakeEvent, Is.Not.Null);
        Assert.That(myFakeEvent.UserFullName, Is.Null);

        Assert.That(bus.PublishedEvents.Count, Is.EqualTo(1));
    }

    [Test]
    public void Retrocompatibility_StandardEvent()
    {
        var cmd = new MyFakeCommand();
        var myContext = new MyCustomContextInfo
        {
            ByUsername = "PrimeUser",
            UserFullName = "Optimus Prime"
        };
        //Act
        var myCommandHandler = new MyFakeDomainEventCommandHandler(bus);
        var executeCommandTask = myCommandHandler.Execute(cmd, myContext);
        executeCommandTask.Wait();

        //Assert
        var @event = bus.PublishedEvents[0];
        Assert.That(@event, Is.TypeOf<MyFakeDomainEvent>());
        var myFakeEvent = (@event as MyFakeDomainEvent);
        Assert.That(myFakeEvent, Is.Not.Null);
        Assert.That(myFakeEvent.UserFullName, Is.Null);

        Assert.That(bus.PublishedEvents.Count, Is.EqualTo(1));
    }
}

public class MyCustomContextInfo : ContextInfo
{
    public string UserFullName { get; set; }
}

public interface IMyCustomDomainEvent
{
    string UserFullName { get; set; }
}
public abstract class MyCustomDomainEvent<T> : DomainEvent<T>, IMyCustomDomainEvent
{
    protected MyCustomDomainEvent(T parameters) : base(parameters)
    {
    }

    public string UserFullName { get; set; }
}

public abstract class MyCustomBaseCommandHandlerAsync<T> : BaseCommandHandlerAsync<T> where T : BaseCommand
{
    protected MyCustomBaseCommandHandlerAsync(IBus bus) : base(bus)
    {
    }

    protected override void SetEventContextInfo(T command, IContextInfo contextInfo, IEvent @event)
    {
        base.SetEventContextInfo(command, contextInfo, @event);

        if (contextInfo is MyCustomContextInfo customContextInfo && @event is IMyCustomDomainEvent customDomainEvent)
        {
            customDomainEvent.UserFullName = customContextInfo.UserFullName;
        }

    }
}

public interface IMyFakeCommand
{
    string FakeItem { get; set; }
}

public class MyFakeCommand : BaseCommand, IMyFakeCommand
{
    public string FakeItem { get; set; }
}

public class MyFakeCommandHandler : MyCustomBaseCommandHandlerAsync<MyFakeCommand>
{
    public MyFakeCommandHandler(IBus bus) : base(bus)
    {
    }

    public override Task<List<ValidationResult>> ValidateCommand(MyFakeCommand command)
    {
        return Task.FromResult(new List<ValidationResult>());
    }

    public override Task<IEvent> PublishEvent(MyFakeCommand command)
    {
        return Task.FromResult<IEvent>(new MyFakeEvent(command));
    }
}

public class MyFakeEvent : MyCustomDomainEvent<IMyFakeCommand>
{
    public MyFakeEvent(IMyFakeCommand parameters) : base(parameters)
    {
    }
}

#region Fake command hanlder with base domain event

public class MyFakeDomainEvent : DomainEvent<IMyFakeCommand>
{
    public MyFakeDomainEvent(IMyFakeCommand parameters) : base(parameters)
    {
    }

    public string UserFullName { get; set; }
}

public class MyFakeDomainEventCommandHandler : MyCustomBaseCommandHandlerAsync<MyFakeCommand>
{
    public MyFakeDomainEventCommandHandler(IBus bus) : base(bus)
    {
    }

    public override Task<List<ValidationResult>> ValidateCommand(MyFakeCommand command)
    {
        return Task.FromResult(new List<ValidationResult>());
    }

    public override Task<IEvent> PublishEvent(MyFakeCommand command)
    {
        return Task.FromResult<IEvent>(new MyFakeDomainEvent(command));
    }
}
#endregion
