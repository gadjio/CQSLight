using System;

namespace Sample.Core.AppCore.RegionalWeatherForecast.QueryResults
{
	public class WeatherForecastPresentationModel
	{
		public Guid RegionId { get; set; }
		public long Id { get; set; }

		public string RegionName { get; set; }

		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

		public string Summary { get; set; }
	}
}