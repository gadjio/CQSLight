using System;
using PGMS.CQSLight.Infra.Commands;

namespace Sample.Core.AppCore.RegionalWeatherForecast.Commands
{
	public interface IAddWeatherForecast
	{
		DateTime Date { get; set; }
		int TemperatureC { get; set; }
		string Summary { get; set; }
		long EntityId { get; set; }
	}

	public class AddWeatherForecastCommand : BaseCommand, IAddWeatherForecast
	{
		public long EntityId { get; set; }

		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public string Summary { get; set; }
	}
}