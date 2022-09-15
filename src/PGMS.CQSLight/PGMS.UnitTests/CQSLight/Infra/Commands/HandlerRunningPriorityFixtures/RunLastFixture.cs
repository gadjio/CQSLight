using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Querying.Services;
using PGMS.CQSLight.Installers;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.Data.Services;
using PGMS.UnitTests.CQSLight.Infra.Querying.QueryProcessorFixtures;

namespace PGMS.UnitTests.CQSLight.Infra.Commands.HandlerRunningPriorityFixtures;

[TestFixture]
public class RunLastFixture
{
	private IContainer container;

	private IEntityRepository entityRepository;

	[SetUp]
	public void SetUp()
	{
		var builder = new ContainerBuilder();

		entityRepository = new InMemoryReportingRepository();

		builder.Register(c => entityRepository).As<IEntityRepository>().SingleInstance();

		builder.Register(c => new DirectBusConfigurationProvider(true)).As<IDirectBusConfigurationProvider>().SingleInstance();
		builder.Register(c => new NullLogger<IBus>()).As<ILogger<IBus>>().SingleInstance();
		builder.Register(c => new FakeUnitOfWorkProvider()).As<IUnitOfWorkProvider>().SingleInstance();

		builder.RegisterAssemblyTypes(typeof(RunLastFixture).GetTypeInfo().Assembly)
			.Where(t => t.Namespace != null && t.Namespace.Contains("PGMS.UnitTests.CQSLight.Infra.Commands.HandlerRunningPriorityFixtures"))
			.AsImplementedInterfaces();

		builder.RegisterAssemblyTypes(typeof(QueryProcessor).GetTypeInfo().Assembly)
			.Where(t => t.Namespace != null && t.Namespace.Contains("PGMS.CQSLight.Infra.Commands.Services"))
			.AsImplementedInterfaces();

		container = builder.Build();
	}

	[Test]
	public void CheckRunLast()
	{
		var bus = container.Resolve<IBus>();

		//Act
		var task = bus.Publish(new FakeEvent(null));
		task.Wait();

		//Assert
		var traces = entityRepository.FindAll<TraceToken>().OrderBy(x => x.CreationTicks).ToList();

		Assert.That(traces.Count(), Is.EqualTo(3));
		Assert.That(traces.Select(x => x.TraceName).Distinct().ToList().Count, Is.EqualTo(3));

		Assert.That(traces.Last().TraceName, Is.EqualTo("Handler Last"));
	}

	[Test]
	public void CheckRunLast_MultipleEvents()
	{
		var bus = container.Resolve<IBus>();

		//Act
		var task = bus.Publish(new List<IDomainEvent> { new FakeEvent(null), new FakeEvent(null) });
		task.Wait();

		//Assert
		var traces = entityRepository.FindAll<TraceToken>().OrderBy(x => x.CreationTicks).ToList();

		Assert.That(traces.Count(), Is.EqualTo(6));
		Assert.That(traces.Select(x => x.TraceName).Distinct().ToList().Count, Is.EqualTo(3));

		Assert.That(traces[2].TraceName, Is.EqualTo("Handler Last"));
		Assert.That(traces.Last().TraceName, Is.EqualTo("Handler Last"));
	}
}

public interface IFakeEvent
{}

public class FakeEvent : DomainEvent<IFakeEvent>
{
	public FakeEvent(IFakeEvent parameters) : base(parameters)
	{
	}
}


public class TraceToken
{
	public long CreationTicks { get; }
	public string TraceName { get; }

	public TraceToken(long creationTicks, string traceName)
	{
		CreationTicks = creationTicks;
		TraceName = traceName;
	}
}

public class FakeEventHandler1 : IHandleEvent<FakeEvent>
{
	private readonly IEntityRepository entityRepository;

	public FakeEventHandler1(IEntityRepository entityRepository)
	{
		this.entityRepository = entityRepository;
	}

	public Task Handle(FakeEvent @event, IUnitOfWork unitOfWork)
	{
		entityRepository.Insert(new TraceToken(DateTime.Now.Ticks, "Handler1"));
		Thread.Sleep(1);
		return Task.CompletedTask;
	}
}

[EventHandlerPriority(EventHandlerProcessingPriority.RunLast)]
public class FakeEventHandlerLast : IHandleEvent<FakeEvent>
{
	private readonly IEntityRepository entityRepository;

	public FakeEventHandlerLast(IEntityRepository entityRepository)
	{
		this.entityRepository = entityRepository;
	}

	public Task Handle(FakeEvent @event, IUnitOfWork unitOfWork)
	{
		entityRepository.Insert(new TraceToken(DateTime.Now.Ticks, "Handler Last"));
		Thread.Sleep(1);
		return Task.CompletedTask;
	}
}


[EventHandlerPriority(EventHandlerProcessingPriority.Standard)]
public class FakeEventHandlerStd : IHandleEvent<FakeEvent>
{
	private readonly IEntityRepository entityRepository;

	public FakeEventHandlerStd(IEntityRepository entityRepository)
	{
		this.entityRepository = entityRepository;
	}

	public Task Handle(FakeEvent @event, IUnitOfWork unitOfWork)
	{
		entityRepository.Insert(new TraceToken(DateTime.Now.Ticks, "Handler Std"));
		Thread.Sleep(1);
		return Task.CompletedTask;
	}
}

