using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;
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

		public DbSet<ProjectReporting> Projects { get; set; }
		public DbSet<ProjectParticipant> ProjectParticipants { get; set; }
		public DbSet<ProjectSupplier> ProjectSuppliers { get; set; }

		public DbSet<ProjectReporting_Custom> Projects_Custom { get; set; }
		public DbSet<ProjectParticipant_Custom> ProjectParticipants_Custom { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ProjectReporting>(entity =>
			{
				entity.HasMany(d => d.Participants)
					.WithOne(p => p.Project);
			});
			modelBuilder.Entity<ProjectParticipant>(entity =>
			{
				entity.HasKey(k => new {k.ClientId, k.ProjectId});
			});
			modelBuilder.Entity<ProjectSupplier>(entity =>
			{
				entity.HasKey(k => new { k.SupplierId, k.ProjectId });
			});

			// Custom Lazy Attribute

			modelBuilder.Entity<ProjectReporting_Custom>(entity =>
			{
				entity.HasMany(d => d.Participants)
					.WithOne(p => p.Project);
			});

			modelBuilder.Entity<ProjectParticipant_Custom>(entity =>
			{
				entity.HasKey(k => new { k.ClientId, k.ProjectId });
			});

			// Old Lazy (Full lazy)
			modelBuilder.Entity<ProjectReportingFullLazy>(entity =>
			{
				entity.HasMany(d => d.Participants)
					.WithOne(p => p.Project);
			});
			modelBuilder.Entity<ProjectParticipantFullLazy>(entity =>
			{
				entity.HasKey(k => new { k.ClientId, k.ProjectId });
			});
			modelBuilder.Entity<ProjectSupplierFullLazy>(entity =>
			{
				entity.HasKey(k => new { k.SupplierId, k.ProjectId });
			});
		}
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

	public class ProjectReporting
	{
		public ProjectReporting()
		{
			Participants = new List<ProjectParticipant>();
			Suppliers = new List<ProjectSupplier>();
		}

		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		[PGMS.Data.Services.LazyLoad]
		public virtual IList<ProjectParticipant> Participants { get; set; }

		public virtual IList<ProjectSupplier> Suppliers { get; set; }
	}

	public class ProjectSupplier
	{
		public long ProjectId { get; set; }

		public long SupplierId { get; set; }

		public virtual ProjectReporting Project { get; set; }
	}


	public class ProjectParticipant
	{
		public long ProjectId { get; set; }

		public long ClientId { get; set; }

		public virtual ProjectReporting Project { get; set; }
	}

	public class ProjectReporting_Custom
	{
		public ProjectReporting_Custom()
		{
			Participants = new List<ProjectParticipant_Custom>();
		}

		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		[Islazyloading]
		public virtual IList<ProjectParticipant_Custom> Participants { get; set; }
	}

	public class ProjectParticipant_Custom
	{
		public long ProjectId { get; set; }

		public long ClientId { get; set; }

		public virtual ProjectReporting_Custom Project { get; set; }
	}

	public class IslazyloadingAttribute : Attribute
	{

	}


	public class ProjectReportingFullLazy
	{
		public ProjectReportingFullLazy()
		{
			Participants = new List<ProjectParticipantFullLazy>();
			Suppliers = new List<ProjectSupplierFullLazy>();
		}

		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		[PGMS.Data.Services.IsLazyLoading]
		public virtual IList<ProjectParticipantFullLazy> Participants { get; set; }

		public virtual IList<ProjectSupplierFullLazy> Suppliers { get; set; }
	}

	public class ProjectSupplierFullLazy
	{
		public long ProjectId { get; set; }

		public long SupplierId { get; set; }

		public virtual ProjectReportingFullLazy Project { get; set; }
	}


	public class ProjectParticipantFullLazy
	{
		public long ProjectId { get; set; }

		public long ClientId { get; set; }

		public virtual ProjectReportingFullLazy Project { get; set; }
	}
}
