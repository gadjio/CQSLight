using PGMS.CQSLight.Infra.Commands;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;

namespace Sample.Core.AppCore.RegionalWeatherForecast.Events
{
	public class RegionWeatherForecastAddedEvent : DomainEvent<IAddWeatherForecast>
	{
		public RegionWeatherForecastAddedEvent(IAddWeatherForecast parameters) : base(parameters)
		{
		}
	}
}