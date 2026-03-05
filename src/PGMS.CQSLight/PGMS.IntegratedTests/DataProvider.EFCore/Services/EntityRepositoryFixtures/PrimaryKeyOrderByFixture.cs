using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;
using PGMS.FakeImpl.DataProvider.Context;
using TestContext = PGMS.FakeImpl.DataProvider.Context.TestContext;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class PrimaryKeyOrderByFixture
{
    private BaseEntityRepository<TestContext> entityRepository;
    private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

    private long unixTimeStampMs;
    private Guid projectId;

    [SetUp]
    public void SetUp()
    {
        entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
        using (var unitOfWork = entityRepository.GetUnitOfWork())
        {
            ((TestContext)unitOfWork.GetDbContext()).Database.EnsureCreated();
        }

        var location = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == QADataProvider.LocationId);
        if (location == null)
        {
            entityRepository.Insert(new LocationReporting { AggregateRootId = QADataProvider.LocationId, Name = "UnitTests" });
        }

        unixTimeStampMs = (long)DateTime.Now.ToEpochInMilliseconds();
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.AngelaMartinId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.CreedBrattonId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.DwightSchruteId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });

        // Setup composite key entity
        projectId = Guid.NewGuid();
        entityRepository.Insert(new ProjectReporting { AggregateRootId = projectId });
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);
        entityRepository.Insert(new ProjectParticipant { ProjectId = project.Id, ClientId = 1001 });
        entityRepository.Insert(new ProjectParticipant { ProjectId = project.Id, ClientId = 1002 });
        entityRepository.Insert(new ProjectParticipant { ProjectId = project.Id, ClientId = 1003 });
    }

    [Test]
    public void FindAll_WithoutOrderBy_SinglePK_ReturnsAllData()
    {
        // Act - no orderBy specified, should fallback to PK ordering
        var result = entityRepository.FindAll<LogEntryReporting>(x => x.UnixTimeStampMs == unixTimeStampMs);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));

        // Verify ordered by Id (PK)
        var ids = result.Select(x => x.Id).ToList();
        Assert.That(ids, Is.Ordered, "Results should be ordered by primary key (Id)");
    }

    [Test]
    public async Task FindAllAsync_WithoutOrderBy_SinglePK_ReturnsAllData()
    {
        // Act
        var result = await entityRepository.FindAllAsync<LogEntryReporting>(x => x.UnixTimeStampMs == unixTimeStampMs);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));

        var ids = result.Select(x => x.Id).ToList();
        Assert.That(ids, Is.Ordered, "Results should be ordered by primary key (Id)");
    }

    [Test]
    public void Get_WithoutOrderBy_SinglePK_ReturnsOrderedData()
    {
        // Act - Get uses GetOperation with Skip/Take
        var result = entityRepository.Get<LogEntryReporting>(x => x.UnixTimeStampMs == unixTimeStampMs, null, 2, 0);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var ids = result.Select(x => x.Id).ToList();
        Assert.That(ids, Is.Ordered, "Results should be ordered by primary key (Id)");
    }

    [Test]
    public async Task GetAsync_WithoutOrderBy_SinglePK_PaginationIsConsistent()
    {
        // Act - fetch in 2 pages
        var page1 = await entityRepository.GetAsync<LogEntryReporting>(x => x.UnixTimeStampMs == unixTimeStampMs, null, 2, 0);
        var page2 = await entityRepository.GetAsync<LogEntryReporting>(x => x.UnixTimeStampMs == unixTimeStampMs, null, 2, 2);

        // Assert - no duplicates between pages
        Assert.That(page1.Count, Is.EqualTo(2));
        Assert.That(page2.Count, Is.EqualTo(1));

        var allIds = page1.Select(x => x.Id).Concat(page2.Select(x => x.Id)).ToList();
        Assert.That(allIds.Distinct().Count(), Is.EqualTo(allIds.Count), "No duplicates should exist across pages");
        Assert.That(allIds, Is.Ordered, "All IDs across pages should be ordered");
    }

    [Test]
    public void FindAll_WithoutOrderBy_CompositePK_ReturnsOrderedData()
    {
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);

        // Act - ProjectParticipant has composite PK (ClientId, ProjectId)
        var result = entityRepository.FindAll<ProjectParticipant>(x => x.ProjectId == project.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));

        // Verify ordered by first PK column (ClientId)
        var clientIds = result.Select(x => x.ClientId).ToList();
        Assert.That(clientIds, Is.Ordered, "Results should be ordered by the first composite PK column (ClientId)");
    }

    [Test]
    public async Task GetAsync_WithoutOrderBy_CompositePK_PaginationIsConsistent()
    {
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);

        // Act - fetch in 2 pages
        var page1 = await entityRepository.GetAsync<ProjectParticipant>(x => x.ProjectId == project.Id, null, 2, 0);
        var page2 = await entityRepository.GetAsync<ProjectParticipant>(x => x.ProjectId == project.Id, null, 2, 2);

        // Assert - no duplicates between pages
        Assert.That(page1.Count, Is.EqualTo(2));
        Assert.That(page2.Count, Is.EqualTo(1));

        var allClientIds = page1.Select(x => x.ClientId).Concat(page2.Select(x => x.ClientId)).ToList();
        Assert.That(allClientIds.Distinct().Count(), Is.EqualTo(allClientIds.Count), "No duplicates should exist across pages with composite PK");
    }

    [Test]
    public void GetOperation_WithoutOrderBy_KeylessEntity_DoesNotThrow()
    {
        // Act & Assert - KeylessViewReporting has no PK (HasNoKey())
        // GetPrimaryKeyOrderBy should return null, and the query should not throw
        // Note: We can't insert into a keyless view, so we just verify the query executes without error
        Assert.DoesNotThrow(() =>
        {
            entityRepository.Get<KeylessViewReporting>(null, null, 10, 0);
        });
    }

    [Test]
    public void FindAll_WithExplicitOrderBy_UsesProvidedOrderBy()
    {
        // Act - explicit descending order should be respected, not overridden by PK
        var result = entityRepository.FindAll<LogEntryReporting>(
            x => x.UnixTimeStampMs == unixTimeStampMs,
            q => q.OrderByDescending(e => e.Id));

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));

        var ids = result.Select(x => x.Id).ToList();
        Assert.That(ids, Is.Ordered.Descending, "Results should be ordered by Id descending as explicitly specified");
    }

    [Test]
    public async Task FindTopAsync_WithoutOrderBy_ReturnsOrderedData()
    {
        // Act
        var result = await entityRepository.FindTopAsync<LogEntryReporting>(
            x => x.UnixTimeStampMs == unixTimeStampMs, null, 2, 0);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var ids = result.Select(x => x.Id).ToList();
        Assert.That(ids, Is.Ordered, "Results should be ordered by primary key (Id)");
    }
}

