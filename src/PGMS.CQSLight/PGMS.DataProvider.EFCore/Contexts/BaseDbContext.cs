using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;

namespace PGMS.DataProvider.EFCore.Contexts
{
	public interface IBaseDbContext : IDbContext
	{
		DbSet<DbSequenceHiLo> sequenceHiLo { get; set; }
	}

	public class BaseDbContext : DbContext, IBaseDbContext
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