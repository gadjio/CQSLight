using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Services;
using Sample.DataProvider.Contexts;

namespace Sample.DataProvider.Services
{
	public class SampleContextFactory : ContextFactory<SampleContext>
	{
		private bool reUseContext = true;
		private SampleContext context = null;

		public override SampleContext CreateContext(DbContextOptions<SampleContext> options)
		{
			if (reUseContext == false)
			{
				return new SampleContext(options);
			}

			if (context != null)
			{
				return context;
			}
			context = new SampleContext(options);
			return context;
		}

		public void InitContextUsage(bool reUseContext)
		{
			this.reUseContext = reUseContext;
		}
	}
}