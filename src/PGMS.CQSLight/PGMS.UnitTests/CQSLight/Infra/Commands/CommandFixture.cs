using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Security;
using PGMS.CQSLight.Installers;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.Data.Services;

namespace PGMS.UnitTests.CQSLight.Infra.Commands;

[TestFixture]
public class CommandFixture
{
    private IContainer container;
    private InMemoryEntityRepository inMemoryEntityRepository;

	[SetUp]
    public void SetUp()
    {
		var builder = new ContainerBuilder();

        CqsLightInfraInstaller.ConfigureServices(builder);

        builder.RegisterAssemblyTypes(typeof(CommandFixture).GetTypeInfo().Assembly)
            .Where(t => t.Namespace != null && t.Namespace.Contains("PGMS.UnitTests.CQSLight.Infra.Commands"))
            .AsImplementedInterfaces();

        inMemoryEntityRepository = new InMemoryEntityRepository();
        builder.Register(c => inMemoryEntityRepository).As<IEntityRepository>();
        builder.Register(c => inMemoryEntityRepository).As<IScopedEntityRepository>();
        builder.Register(c => inMemoryEntityRepository).As<IUnitOfWorkProvider>();

		builder.Register(c => new NullLogger<IBus>()).As<ILogger<IBus>>();
        builder.Register(c => new NullLogger<IEvent>()).As<ILogger<IEvent>>();

		container = builder.Build();
	}

    [Test]
    public void ValidateEventHandlerRegistration()
    {
        var handler = container.Resolve<IHandleEvent<FakeDomainEvent>>();
        Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public void Check_Base_event_fields()
    {
        var startTime = DateTime.Now;
        var command = new FakeCommand{AggregateRootId = Guid.NewGuid(), Name = "ABC"};
        var contextInfo = new ContextInfo { ByUsername = "PrimeUser", ByUserId = "47" };

        //Act
        var bus = container.Resolve<IBus>();
        bus.Send(command, contextInfo);

        //Assert
        var reporting = inMemoryEntityRepository.FindFirst<FakeReporting>(x => x.AggregateRootId == command.AggregateRootId);
        Assert.That(reporting, Is.Not.Null);
        Assert.That(reporting.CommandType, Is.EqualTo(typeof(FakeCommand).FullName));
        Assert.That(reporting.ByUsername, Is.EqualTo("PrimeUser"));
        Assert.That(reporting.ByUserId, Is.EqualTo("47"));
        Assert.That(reporting.TimeStamp, Is.GreaterThanOrEqualTo(startTime.ToEpoch()));
        Assert.That(reporting.TimeStamp, Is.LessThanOrEqualTo(DateTime.Now.ToEpoch()));

        Assert.That(reporting.Name, Is.EqualTo("ABC"));
	}
}

public interface IFakeCommand
{
    string Name { get; set; }
}

public class FakeCommand : BaseCommand, IFakeCommand
{
    public string Name { get; set; }
}

public class FakeCommandHandler : BaseCommandHandler<FakeCommand>
{
    public FakeCommandHandler(IBus bus) : base(bus)
    {
    }

    public override IEnumerable<ValidationResult> ValidateCommand(FakeCommand command)
    {
        return new List<ValidationResult>();
    }

    public override IEvent PublishEvent(FakeCommand command)
    {
        return new FakeDomainEvent(command);
    }
}

public class FakeDomainEvent : DomainEvent<IFakeCommand>
{
    public FakeDomainEvent(IFakeCommand parameters) : base(parameters)
    {
    }
}

public class FakeDomainEventHandler : BaseEventHandler<FakeDomainEvent>
{
    public FakeDomainEventHandler(IScopedEntityRepository entityRepository, ILogger<IEvent> logger) : base(entityRepository, logger)
    {
    }

    protected override void HandleEvent(FakeDomainEvent @event, IUnitOfWork unitOfWork)
    {
        entityRepository.InsertOperation(unitOfWork, new FakeReporting
        {
            AggregateRootId = @event.AggregateId,
            ByUsername = @event.ByUsername,
            ByUserId = @event.ByUserId,

            CommandType = @event.CommandType,

            TimeStamp = @event.Timestamp,

            Name = @event.Parameters.Name
        });
    }
}

public class FakeReporting
{
    public Guid AggregateRootId { get; set; }
    public string ByUsername { get; set; }
    public string ByUserId { get; set; }

    public string Name { get; set; }
    
    public string CommandType { get; set; }
    
    public long TimeStamp { get; set; }
}

