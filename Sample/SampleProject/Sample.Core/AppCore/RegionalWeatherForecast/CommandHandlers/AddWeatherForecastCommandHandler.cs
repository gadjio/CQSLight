using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;
using Sample.Core.AppCore.RegionalWeatherForecast.Events;

namespace Sample.Core.AppCore.RegionalWeatherForecast.CommandHandlers
{
	public class AddWeatherForecastCommandHandler : BaseCommandHandler<AddWeatherForecastCommand>
	{
		public AddWeatherForecastCommandHandler(IBus bus) : base(bus)
		{
		}

		public override IEnumerable<ValidationResult> ValidateCommand(AddWeatherForecastCommand command)
		{
			return new List<ValidationResult>();
		}

		public override IEvent PublishEvent(AddWeatherForecastCommand command)
		{
			return new RegionWeatherForecastAddedEvent(command);
		}
	}
}