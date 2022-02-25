
using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;
using Sample.Data.Models.App;
using Sample.Data.Models.WeatherForecast;

namespace Sample.DataProvider.Contexts
{
	public class SampleContext : BaseDbContext
	{
		public SampleContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<ActivityLog> ActivityLogs { get; set; }
		public DbSet<DomainEventReporting> DomainEventReporting { get; set; }
		public DbSet<DomainEventProviderReporting> DomainEventProviderReporting { get; set; }
		public DbSet<UpdateServiceLookup> UpdateServiceLookup { get; set; }
		public DbSet<ApiAuthToken> ApiAuthTokens { get; set; }

		public DbSet<RegionReporting> Regions { get; set; }
		public DbSet<WeatherInfoReporting> WeatherInfos { get; set; }
	}
}