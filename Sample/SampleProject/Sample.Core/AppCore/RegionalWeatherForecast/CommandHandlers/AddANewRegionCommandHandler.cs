using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.Data.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;
using Sample.Core.AppCore.RegionalWeatherForecast.Events;
using Sample.Data.Models.App;
using Sample.Data.Models.WeatherForecast;

namespace Sample.Core.AppCore.RegionalWeatherForecast.CommandHandlers
{
	public class AddANewRegionCommandHandler : BaseCommandHandler<AddANewRegionCommand>
	{
		private readonly IEntityRepository entityRepository;

		public AddANewRegionCommandHandler(IBus bus, IEntityRepository entityRepository) : base(bus)
		{
			this.entityRepository = entityRepository;
		}

		public override IEnumerable<ValidationResult> ValidateCommand(AddANewRegionCommand command)
		{
			var result = new List<ValidationResult>();
			var existing = entityRepository.FindFirst<RegionReporting>(x => x.Name == command.Name);
			if (existing != null)
			{
				result.Add(new ValidationResult($"Region '{command.Name}' already exists."));
			}

			return result;
		}

		public override IEvent PublishEvent(AddANewRegionCommand command)
		{
			return new NewRegionAddedEvent(command);
		}
	}
}