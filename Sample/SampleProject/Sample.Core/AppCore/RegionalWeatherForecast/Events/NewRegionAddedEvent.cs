using PGMS.CQSLight.Infra.Commands;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;

namespace Sample.Core.AppCore.RegionalWeatherForecast.Events
{
	public class NewRegionAddedEvent : DomainEvent<IAddANewRegion>
	{
		public NewRegionAddedEvent(IAddANewRegion parameters) : base(parameters)
		{
		}
	}
}