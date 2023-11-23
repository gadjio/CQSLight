using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;

public class FakeBus : IBus
{
	public List<IEvent> PublishedEvents { get; private set; }
	public List<ICommand> PublishedCommands { get; private set; }
	public List<KeyValuePair<ICommand, IContextInfo>> PublishedCommandsMap { get; private set; }

	public FakeBus()
	{
		PublishedEvents = new List<IEvent>();
		PublishedCommands = new List<ICommand>();
		PublishedCommandsMap = new List<KeyValuePair<ICommand, IContextInfo>>();
	}

	public Task Publish<T>(T @event) where T : class, IEvent
	{
		PublishedEvents.Add(@event);
		return Task.CompletedTask;
	}

	public Task Publish<T>(IEnumerable<T> events) where T : class, IEvent
	{
		PublishedEvents.AddRange(events);
		return Task.CompletedTask;
	}

	public Task Send(ICommand command, IContextInfo contextInfo)
	{
		PublishedCommands.Add(command);
		PublishedCommandsMap.Add(new KeyValuePair<ICommand, IContextInfo>(command, contextInfo));
		return Task.CompletedTask;
	}
}