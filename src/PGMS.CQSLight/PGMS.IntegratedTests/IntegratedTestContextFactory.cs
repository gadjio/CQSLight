using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests
{
	public class IntegratedTestContextFactory : ContextFactory<TestContext>
	{
		private bool reUseContext = false;
		private TestContext context = null;

		public override TestContext CreateContext(DbContextOptions<TestContext> options)
		{
			if (reUseContext == false)
			{
				return new TestContext(options);
			}

			if (context != null)
			{
				return context;
			}
			context = new TestContext(options);
			return context;
		}

		public void InitContextUsage(bool reUseContext)
		{
			this.reUseContext = reUseContext;
		}
	}
}