using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Infra.Commands;
using PGMS.Data.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Events;
using Sample.Data.Models.App;
using Sample.Data.Models.WeatherForecast;

namespace Sample.Core.EventHandlers.Reportings.Regions
{
	public class NewRegionAddedEventHandler : BaseEventHandler<NewRegionAddedEvent>
	{
		public NewRegionAddedEventHandler(IScopedEntityRepository entityRepository, ILogger<IEvent> logger) : base(entityRepository, logger)
		{
		}

		protected override void HandleEvent(NewRegionAddedEvent @event, IUnitOfWork unitOfWork)
		{
			var parameters = @event.Parameters;

			var reporting = new RegionReporting
			{
				AggregateRootId = @event.AggregateId,
				LastUpdate = @event.Timestamp,

				Name = parameters.Name
			};

			entityRepository.InsertOperation(unitOfWork, reporting);
		}
	}
}