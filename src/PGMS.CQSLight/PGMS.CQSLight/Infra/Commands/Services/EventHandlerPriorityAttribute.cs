using System;

namespace PGMS.CQSLight.Infra.Commands.Services;

public class EventHandlerPriorityAttribute : Attribute
{
	public EventHandlerProcessingPriority Priority { get; }

	public EventHandlerPriorityAttribute(EventHandlerProcessingPriority priority)
	{
		Priority = priority;
	}
}

public enum EventHandlerProcessingPriority
{
	Standard,
	RunLast,
}