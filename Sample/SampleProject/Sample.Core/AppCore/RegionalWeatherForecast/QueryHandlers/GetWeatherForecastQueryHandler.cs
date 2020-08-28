using System;
using System.Collections.Generic;
using System.Linq;
using PGMS.CQSLight.Infra.Querying;
using PGMS.Data.Services;
using Sample.Core.AppCore.RegionalWeatherForecast.Queries;
using Sample.Core.AppCore.RegionalWeatherForecast.QueryResults;
using Sample.Data.Models.WeatherForecast;

namespace Sample.Core.AppCore.RegionalWeatherForecast.QueryHandlers
{
	public class GetWeatherForecastQueryHandler : IHandleQuery<GetWeatherForecastQuery, IList<WeatherForecastPresentationModel>>
	{
		private readonly IEntityRepository entityRepository;

		public GetWeatherForecastQueryHandler(IEntityRepository entityRepository)
		{
			this.entityRepository = entityRepository;
		}

		public IList<WeatherForecastPresentationModel> Handle(GetWeatherForecastQuery query)
		{
			var reportings = GetReportings(query);

			var result = new List<WeatherForecastPresentationModel>();

			foreach (var weatherInfoReporting in reportings)
			{
				result.Add(new WeatherForecastPresentationModel
				{
					Id = weatherInfoReporting.EntityId,
					RegionId = weatherInfoReporting.AggregateRootId,
					Date = weatherInfoReporting.Date,
					TemperatureC = weatherInfoReporting.TemperatureC,
					Summary = weatherInfoReporting.Summary,
					RegionName = weatherInfoReporting.RegionName,
				});
			}

			return result;
		}

		private List<WeatherInfoReporting> GetReportings(GetWeatherForecastQuery query)
		{
			if (query.RegionId.HasValue)
			{
				return entityRepository.FindAll<WeatherInfoReporting>(x => x.AggregateRootId == query.RegionId.Value).ToList();
			}

			if (query.Date.HasValue)
			{
				return entityRepository.FindAll<WeatherInfoReporting>(x => x.Date == query.Date.Value.Date).ToList();
			}

			return new List<WeatherInfoReporting>();
		}
	}
}