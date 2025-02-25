using Microsoft.EntityFrameworkCore;
using PGMS.DataProvider.EFCore.Contexts;
using System;
using System.Collections.Generic;

namespace PGMS.FakeImpl.DataProvider.Context;

public class MyDbContext : BaseDbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<GameGridReporting> GameGrids { get; set; }
    public DbSet<GameState> GameStates { get; set; }

    public DbSet<Player> Players { get; set; }

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