using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests
{
	public class IntegratedTestContextFactory : ContextFactory<TestContext>
	{
		

		public override TestContext CreateContext(DbContextOptions<TestContext> options)
		{
			return new TestContext(options);
		}

	}
}