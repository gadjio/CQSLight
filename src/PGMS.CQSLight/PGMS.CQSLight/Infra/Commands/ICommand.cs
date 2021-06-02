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
        string ByUsername { get; set; }
        Guid AggregateRootId { get; set; }
    }

    public class BaseCommand : ICommand
    {
        public Guid Id { get; private set; }

        public string ByUsername { get; set; }

        public Guid AggregateRootId { get; set; }

        public BaseCommand()
        {
            Id = Guid.NewGuid();
        }
    }
}