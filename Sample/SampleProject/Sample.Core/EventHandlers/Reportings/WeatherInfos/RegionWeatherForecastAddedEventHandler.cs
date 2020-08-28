using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Infra.Commands;
using PGMS.Data.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Events;
using Sample.Data.Models.WeatherForecast;

namespace Sample.Core.EventHandlers.Reportings.WeatherInfos
{
	public class RegionWeatherForecastAddedEventHandler : BaseEventHandler<RegionWeatherForecastAddedEvent>
	{
		public RegionWeatherForecastAddedEventHandler(IScopedEntityRepository entityRepository, ILogger<IEvent> logger) : base(entityRepository, logger)
		{
		}

		protected override void HandleEvent(RegionWeatherForecastAddedEvent @event, IUnitOfWork unitOfWork)
		{
			var parameter = @event.Parameters;

			var region = entityRepository.FindFirstOperation<RegionReporting>(unitOfWork, x => x.AggregateRootId == @event.AggregateId);

			var reporting = new WeatherInfoReporting
			{
				AggregateRootId = @event.AggregateId,
				LastUpdate = @event.Timestamp,
				
				EntityId = parameter.EntityId,

				Date = parameter.Date,
				Summary = parameter.Summary,
				TemperatureC = parameter.TemperatureC,

				//We store this denormalized information because the system is 90% read and 10% write, so we take the hit on write instead of joining the information on read
				RegionName = region.Name
			};

			entityRepository.InsertOperation(unitOfWork, reporting);
		}
	}
}