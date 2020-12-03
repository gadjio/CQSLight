using System;
using System.Linq;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.Data.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;
using Sample.Data.Models.WeatherForecast;

namespace Sample.BlazorApp
{
	public static class InitialDataGenerator
	{
		public static void InitializeData(IEntityRepository entityRepository, IBus bus, IDataService dataService)
		{
			var region = entityRepository.FindFirst<RegionReporting>(x => x.Name == "Shawinigan");
			Guid regionId;
			if (region == null)
			{
				regionId = Guid.NewGuid();
				bus.Send(new AddANewRegionCommand{AggregateRootId = regionId, Name = "Shawinigan", ByUsername = "InitialDataGenerator" });
			}
			else
			{
				regionId = region.AggregateRootId;
			}

			var todaysForecasts = entityRepository.FindAll<WeatherInfoReporting>(x => x.AggregateRootId == regionId && x.Date == DateTime.Today);
			if (!todaysForecasts.Any())
			{
				var rng = new Random();
				bus.Send(new AddWeatherForecastCommand
				{
					AggregateRootId = regionId, 
					EntityId = dataService.GenerateId(),
					Date = DateTime.Today,
					TemperatureC = rng.Next(-20, 55),
					Summary = Summaries[rng.Next(Summaries.Length - 1)]
				});
			}
		}

		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

	}
}