using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.QABenchmark;

/// <summary>
/// DbContext mapping to large tables in the QA Azure SQL database for benchmark testing.
/// </summary>
public class QABenchmarkContext : BaseDbContext
{
    public QABenchmarkContext(DbContextOptions options) : base(options) { }

    public DbSet<DomainEventReportingQA> DomainEventReporting { get; set; }
    public DbSet<CQSDomainEventQA> CQSDomainEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DomainEventReportingQA>(entity =>
        {
            entity.ToTable("DomainEventReporting", "dbo");
        });

        modelBuilder.Entity<CQSDomainEventQA>(entity =>
        {
            entity.ToTable("CQSDomainEvents", "Core");
        });
    }
}

public class QABenchmarkContextFactory : ContextFactory<QABenchmarkContext>
{
    public override QABenchmarkContext CreateContext(DbContextOptions<QABenchmarkContext> options)
    {
        return new QABenchmarkContext(options);
    }
}

/// <summary>
/// Maps to dbo.DomainEventReporting (~5.4M rows)
/// </summary>
[Table("DomainEventReporting", Schema = "dbo")]
public class DomainEventReportingQA
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    public long? EntityId { get; set; }
    public Guid? EventId { get; set; }
    public string JSonDomainEvent { get; set; }
    public Guid? EventProviderId { get; set; }
    public string Type { get; set; }
    public string User { get; set; }
    public long? Timestamp { get; set; }
    public DateTime? DateHappened { get; set; }
}

/// <summary>
/// Maps to Core.CQSDomainEvents (~1.2M rows)
/// </summary>
[Table("CQSDomainEvents", Schema = "Core")]
public class CQSDomainEventQA
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    public Guid EventId { get; set; }
    public string JSonDomainEvent { get; set; }
    public Guid? EventProviderId { get; set; }
    public string Type { get; set; }
    public string UserId { get; set; }
    public string User { get; set; }
    public long Timestamp { get; set; }
    public DateTime DateHappened { get; set; }
    public string CommandType { get; set; }
}

