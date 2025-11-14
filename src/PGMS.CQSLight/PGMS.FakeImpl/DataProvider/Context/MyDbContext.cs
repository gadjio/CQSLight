using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PGMS.FakeImpl.DataProvider.Context;

public class MyDbContext : BaseDbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<GameGridReporting> GameGrids { get; set; }
    public DbSet<GameState> GameStates { get; set; }

    public DbSet<Player> Players { get; set; }

    public DbSet<DevOpsCommitReporting> DevOpsCommits { get; set; }
    public DbSet<DevOpsCommitDiffReporting> DevOpsCommitDiffs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<GameGridReporting>(entity =>
        {
            entity.HasOne(d => d.GameStateNavigation)
                .WithMany()
                .HasForeignKey(i => i.AggregateRootId)
                .HasPrincipalKey(i => i.GameGridId);

            entity.Navigation(e => e.GameStateNavigation).AutoInclude();
        });

        builder.Entity<Player>(entity =>
        {
            entity.HasMany(p => p.GameStates)
                .WithOne().HasForeignKey(i => i.PlayerId).HasPrincipalKey(i => i.AggregateRootId);
            entity.Navigation(e => e.GameStates).AutoInclude();
        });

        builder.Entity<DevOpsCommitReporting>(entity =>
        {

            entity.HasIndex(e => new { Organization = e.DevOpsOrganizationName, e.RepositoryId });
            entity.HasIndex(e => e.AuthorEntityId);
            entity.HasIndex(e => e.AuthorEmail); // Keep for backward compatibility and reference
            entity.HasIndex(e => e.AuthorDate);
            entity.HasIndex(e => e.CommitterEntityId);
            entity.HasMany(c => c.Diffs)
                .WithOne(d => d.Commit)
                .HasForeignKey(d => new { d.OrganizationId, d.CommitId })
                .HasPrincipalKey(c => new { c.OrganizationId, c.CommitId });
            entity.Navigation(x => x.Diffs).AutoInclude();
        });
    }
}

public class GameGridReporting
{
    public long Id { get; set; }
    public Guid AggregateRootId { get; set; }
    public GameState GameStateNavigation { get; set; }
}

public class GameState
{
    public long Id { get; set; }
    public Guid GameGridId { get; set; }
    public Guid PlayerId { get; set; }
    public string State { get; set; }
}

public class Player
{
    public long Id { get; set; }
    public Guid AggregateRootId { get; set; }
    public List<GameState> GameStates { get; set; } = new List<GameState>();
}


public class DevOpsCommitReporting
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CommitId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string RepositoryId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Project { get; set; } = string.Empty;

    public string DevOpsOrganizationName { get; set; }

    /// <summary>
    /// EntityId of the author from DevOpsAuthorStatsReporting table
    /// </summary>
    [Required]
    public long AuthorEntityId { get; set; }

    /// <summary>
    /// Original author name from the source system (preserved for reference)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Original author email from the source system (preserved for reference)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// EntityId of the committer from DevOpsAuthorStatsReporting table (nullable for backward compatibility)
    /// </summary>
    public long? CommitterEntityId { get; set; }

    /// <summary>
    /// Original committer name from the source system (preserved for reference)
    /// </summary>
    [MaxLength(200)]
    public string CommitterName { get; set; } = string.Empty;

    /// <summary>
    /// Original committer email from the source system (preserved for reference)
    /// </summary>
    [MaxLength(200)]
    public string CommitterEmail { get; set; } = string.Empty;

    /// <summary>
    /// Original Author Date in UTC
    /// </summary>
    [Required]
    public DateTime AuthorDate { get; set; }

    /// <summary>
    /// Original Comitter Date in UTC
    /// </summary>
    [Required]
    public DateTime CommitterDate { get; set; }

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string? Comment { get; set; }

    [MaxLength(1000)]
    public string ParentIdsJson { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TreeId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    public int ChangeCount { get; set; }

    public bool IsProcessed { get; set; }

    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Pre-calculated value of Σ(lines_added_per_commit × impact_score_per_commit) for performance optimization
    /// </summary>
    public double? LinesAddedRealValue { get; set; }

    /// <summary>
    /// Indicates whether this commit has been analyzed for complexity and impact
    /// </summary>
    public bool IsAnalyzed { get; set; }

    /// <summary>
    /// Indicates whether this commit has been manually ignored by a user
    /// </summary>
    public bool IsIgnored { get; set; }

    [Required]
    public DateTime SyncedAt { get; set; }

    public List<DevOpsCommitDiffReporting> Diffs { get; set; } = new();

    /// <summary>
    /// DevOpsOrganizationName/Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    public Guid OrganizationId { get; set; }

    public bool IsMergeCommit { get; set; }

    public DateTime CommitLocalizeDate { get; set; }
}

public class DevOpsCommitDiffReporting 
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CommitId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ChangeType { get; set; } = string.Empty;

    public int LinesAdded { get; set; }

    public int LinesDeleted { get; set; }

    public int LinesModified { get; set; }

    [MaxLength(10)]
    public string FileExtension { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public bool IsBinary { get; set; }

    /// <summary>
    /// Flag indicating whether this file diff is ignored based on organization ignore rules
    /// </summary>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// JSON array of rule IDs (OrganizationIgnoreRule.EntityId) that matched to mark this diff as ignored
    /// </summary>
    [MaxLength(1000)]
    public string IgnoredRuleIdsJson { get; set; } = string.Empty;

    [Required]
    public DateTime SyncedAt { get; set; }

    public DevOpsCommitReporting Commit { get; set; } = null!;

    /// <summary>
    /// DevOpsOrganizationName/Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    public Guid OrganizationId { get; set; }

    public string DevOpsOrganizationName { get; set; }
}