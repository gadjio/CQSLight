using System;
using System.Collections.Generic;
using PGMS.CQSLight.Infra.Querying;
using Sample.Core.AppCore.RegionalWeatherForecast.QueryResults;

namespace Sample.Core.AppCore.RegionalWeatherForecast.Queries
{
	public class GetWeatherForecastQuery : IQuery<List<WeatherForecastPresentationModel>>
	{
		public DateTime? Date { get; }
		public Guid? RegionId { get; }

		public GetWeatherForecastQuery(DateTime date)
		{
			Date = date;
		}

		public GetWeatherForecastQuery(Guid regionId)
		{
			RegionId = regionId;
		}
	}
}