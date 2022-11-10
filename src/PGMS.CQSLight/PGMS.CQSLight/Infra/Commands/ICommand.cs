using System;

namespace PGMS.CQSLight.Infra.Commands
{
	public interface IInternalCommand : IDomainMessage
	{
		Guid AggregateRootId { get; set; }
    }

	public interface ICommand : IDomainMessage
    {
        Guid Id { get; }
        Guid AggregateRootId { get; set; }
    }

    public class BaseCommand : ICommand
    {
        public Guid Id { get; private set; }

        public Guid AggregateRootId { get; set; }

        public BaseCommand()
        {
            Id = Guid.NewGuid();
        }
    }
}