using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Services;
using Sample.DataProvider.Contexts;

namespace Sample.DataProvider.Services
{
	public class SampleContextFactory : ContextFactory<SampleContext>
	{
		public override SampleContext CreateContext(DbContextOptions<SampleContext> options)
		{
			return new SampleContext(options);
		}
	}
}