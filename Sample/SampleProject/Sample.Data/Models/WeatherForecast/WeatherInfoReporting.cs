using System;

namespace Sample.Data.Models.WeatherForecast
{
	public class WeatherInfoReporting : BaseAggregateRootReporting
	{
		public long EntityId { get; set; }

		public string RegionName { get; set; }

		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public string Summary { get; set; }
	}
}