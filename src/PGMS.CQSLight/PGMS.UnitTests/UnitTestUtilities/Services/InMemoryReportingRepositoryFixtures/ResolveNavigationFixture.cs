using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.FakeImpl.DataProvider.Context;

namespace PGMS.UnitTests.UnitTestUtilities.Services.InMemoryReportingRepositoryFixtures;

[TestFixture]
public class ResolveNavigationFixture
{
    private InMemoryReportingRepository<MyDbContext> entityRepository;

    [SetUp]
    public void SetUp()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseSqlServer("connectionString");
        var dbContext = new MyDbContext(optionsBuilder.Options);

        entityRepository = new InMemoryReportingRepository<MyDbContext>(dbContext);
    }

    [Test]
    public void Test_One_WithMany()
    {
        var id = Guid.NewGuid();
        var gameState = new GameState { Id = 1, GameGridId = id, State = "Active" };
        var gameGrid = new GameGridReporting { Id = 101, AggregateRootId = id };

        entityRepository.Insert(gameState);
        entityRepository.Insert(gameGrid);

        //Act
        var result = entityRepository.FindFirst<GameGridReporting>(x => x.AggregateRootId == id);

        Assert.That(result.Id, Is.EqualTo(101));
        Assert.That(result.GameStateNavigation.Id, Is.EqualTo(1));
    }

    [Test]
    public void Test_Many_WithOne()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = new Player() { Id = 201, AggregateRootId = playerId };

        var gameState = new GameState { Id = 1, GameGridId = id, State = "Active", PlayerId = playerId};
        var gameGrid = new GameGridReporting { Id = 101, AggregateRootId = id };

        entityRepository.Insert(player);
        entityRepository.Insert(gameState);
        entityRepository.Insert(gameGrid);

        //Act
        var result = entityRepository.FindFirst<Player>(x => x.AggregateRootId == playerId);

        Assert.That(result.Id, Is.EqualTo(201));
        Assert.That(result.GameStates[0].Id, Is.EqualTo(1));
    }

    [Test]
    public void Test_Many_WithOne_Resolve_forFiltering()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = new Player() { Id = 201, AggregateRootId = playerId };

        var gameState = new GameState { Id = 1, GameGridId = id, State = "Active", PlayerId = playerId };
        var gameGrid = new GameGridReporting { Id = 101, AggregateRootId = id };

        entityRepository.Insert(player);
        entityRepository.Insert(gameState);
        entityRepository.Insert(gameGrid);

        //Act
        var result = entityRepository.FindFirst<GameGridReporting>(x => x.GameStateNavigation.GameGridId == id);

        Assert.That(result.Id, Is.EqualTo(101));
    }

    [Test]
    public void Test_ComplexKey()
    {
        var org1 = Guid.NewGuid();
        var org2 = Guid.NewGuid();

        entityRepository.Insert(new DevOpsCommitReporting { Id = 1, CommitId = "Commit-101", OrganizationId = org1});
        entityRepository.Insert(new DevOpsCommitReporting { Id = 2, CommitId = "Commit-101", OrganizationId = org2});

        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 101, CommitId = "Commit-101", OrganizationId = org1 });
        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 102, CommitId = "Commit-101", OrganizationId = org1 });

        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 999, CommitId = "Commit-2", OrganizationId = org1 });
        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 998, CommitId = "Commit-2", OrganizationId = org1 });


        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 201, CommitId = "Commit-101", OrganizationId = org2 });
        entityRepository.Insert(new DevOpsCommitDiffReporting() { Id = 202, CommitId = "Commit-101", OrganizationId = org2 });

        var result = entityRepository.FindFirst<DevOpsCommitReporting>(x => x.Id == 1);
        
        Assert.That(result.Diffs.Count, Is.EqualTo(2));
        Assert.That(result.Diffs.Any(x => x.Id == 101));
        Assert.That(result.Diffs.Any(x => x.Id == 102));
    }
}