using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IHandleEvent<T> where T : IEvent
    {
        Task Handle(T @event, IUnitOfWork unitOfWork);
    }

    public abstract class BaseEventHandler<T> : IHandleEvent<T> where T : IEvent
    {
        protected readonly IScopedEntityRepository entityRepository;
        private readonly ILogger<IEvent> logger;

        protected BaseEventHandler(IScopedEntityRepository entityRepository, ILogger<IEvent> logger)
        {
            this.entityRepository = entityRepository;
            this.logger = logger;
        }

        public Task Handle(T @event, IUnitOfWork unitOfWork)
        {
            try
            {
                HandleEvent(@event, unitOfWork);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Transaction commit failed on Handling {typeof(T).Name} - Hanlder : {this.GetType().FullName} - {ex.GetErrorDetails()}");
                logger.LogWarning(sb.ToString());

                throw;
            }
            return Task.CompletedTask;
        }

        protected abstract void HandleEvent(T @event, IUnitOfWork unitOfWork);
    }

    public abstract class BaseEventHandlerAsync<T> : IHandleEvent<T> where T : IEvent
    {
	    protected readonly IScopedEntityRepository entityRepository;
	    private readonly ILogger<IEvent> logger;

	    protected BaseEventHandlerAsync(IScopedEntityRepository entityRepository, ILogger<IEvent> logger)
	    {
		    this.entityRepository = entityRepository;
		    this.logger = logger;
	    }

	    public async Task Handle(T @event, IUnitOfWork unitOfWork)
	    {
		    try
		    {
			    await HandleEvent(@event, unitOfWork);
		    }
		    catch (Exception ex)
		    {
			    var sb = new StringBuilder();
			    sb.AppendLine($"Transaction commit failed on Handling {typeof(T).Name} - Hanlder : {this.GetType().FullName} - {ex.GetErrorDetails()}");
			    logger.LogWarning(sb.ToString());

			    throw;
		    }
	    }

	    protected abstract Task HandleEvent(T @event, IUnitOfWork unitOfWork);
    }
}