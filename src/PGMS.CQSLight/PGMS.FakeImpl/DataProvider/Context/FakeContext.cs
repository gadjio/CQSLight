using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.FakeImpl.DataProvider.Context
{
	public class FakeContext : BaseDbContext	
	{
		public FakeContext(DbContextOptions options) : base(options)
		{
		}
	}

	public class FakeContextFactory : ContextFactory<FakeContext>
	{
		public override FakeContext CreateContext(DbContextOptions<FakeContext> options)
		{
			return new FakeContext(options);
		}

	}
}