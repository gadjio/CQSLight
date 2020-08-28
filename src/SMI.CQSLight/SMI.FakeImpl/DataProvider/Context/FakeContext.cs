using Microsoft.EntityFrameworkCore;
using SMI.DataProvider.EFCore.Contexts;
using SMI.DataProvider.EFCore.Services;

namespace SMI.FakeImpl.DataProvider.Context
{
	public class FakeContext : BaseDbContext	
	{
		public FakeContext(DbContextOptions options) : base(options)
		{
		}
	}

	public class FakeContextFactory : ContextFactory<FakeContext>
	{
		private bool reUseContext = true;
		private FakeContext context = null;

		public override FakeContext CreateContext(DbContextOptions<FakeContext> options)
		{
			if (reUseContext == false)
			{
				return new FakeContext(options);
			}

			if (context != null)
			{
				return context;
			}
			context = new FakeContext(options);
			return context;
		}

		public void InitContextUsage(bool reUseContext)
		{
			this.reUseContext = reUseContext;
		}
	}
}