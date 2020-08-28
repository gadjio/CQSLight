using PGMS.CQSLight.Infra.Commands;

namespace Sample.Core.AppCore.RegionalWeatherForecast.Commands
{
	public interface IAddANewRegion
	{
		string Name { get; set; }
	}

	public class AddANewRegionCommand : BaseCommand, IAddANewRegion
	{
		public string Name { get; set; }
	}
}