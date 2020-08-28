using System;

namespace SMI.CQSLight.Infra.Commands
{
	public interface ICommand : IMessage
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