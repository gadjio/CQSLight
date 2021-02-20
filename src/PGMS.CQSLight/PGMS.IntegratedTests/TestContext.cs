using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;

namespace PGMS.IntegratedTests
{
	public class TestContext : BaseDbContext
	{
		public TestContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<LocationReporting> Locations { get; set; }
		public DbSet<PersonReporting> Persons { get; set; }
		public DbSet<LogEntryReporting> LogEntries { get; set; }

	}

	public class LocationReporting
	{
		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		public long LastUpdate { get; set; }

		public string Name { get; set; }
	}

	public class PersonReporting
	{
		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		public long LastUpdate { get; set; }

		public string Name { get; set; }
	}


	public class LogEntryReporting
	{
		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		public long LastUpdate { get; set; }

		public Guid PersonId { get; set; }
		public Guid LocationId { get; set; }

		public long UnixTimeStampMs { get; set; }
	}
}
