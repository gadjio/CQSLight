using Microsoft.EntityFrameworkCore;
using SMI.Data.Services;

namespace SMI.DataProvider.EFCore.Contexts
{
	public class BaseDbContext : DbContext, IDbContext
	{
		public BaseDbContext(DbContextOptions options)
			: base(options)
		{ }

		public DbSet<DbSequenceHiLo> sequenceHiLo { get; set; }
	}

	public class DbSequenceHiLo
	{
		public long Id { get; set; }
		public string id_parametres { get; set; }
		public int intval { get; set; }
	}
}