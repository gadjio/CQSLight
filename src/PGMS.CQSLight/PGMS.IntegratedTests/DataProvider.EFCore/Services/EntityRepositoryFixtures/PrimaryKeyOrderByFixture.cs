using System;
using System.Collections.Generic;
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
    public void FindAll_CompositePK_SingleResult_ReturnsOne()
    {
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);

        // Act - filter on composite key entity that should return exactly 1 result (like ApplicationUserRole scenario)
        var result = entityRepository.FindAll<ProjectParticipant>(x => x.ProjectId == project.Id && x.ClientId == 1001);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1), "FindAll with filter on composite key entity should return exactly 1 result");
        Assert.That(result[0].ClientId, Is.EqualTo(1001));
        Assert.That(result[0].ProjectId, Is.EqualTo(project.Id));
        NUnit.Framework.TestContext.WriteLine($"[CompositePK SingleResult] FindAll returned {result.Count} result(s) — ClientId={result[0].ClientId}");
    }

    [Test]
    public void FindFirst_CompositePK_ReturnsOne()
    {
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);

        // Act - FindFirst on composite key entity with filter
        var result = entityRepository.FindFirst<ProjectParticipant>(x => x.ProjectId == project.Id && x.ClientId == 1002);

        // Assert
        Assert.That(result, Is.Not.Null, "FindFirst with filter on composite key entity should return a result");
        Assert.That(result.ClientId, Is.EqualTo(1002));
        Assert.That(result.ProjectId, Is.EqualTo(project.Id));
        NUnit.Framework.TestContext.WriteLine($"[CompositePK FindFirst] Returned ClientId={result.ClientId}, ProjectId={result.ProjectId}");
    }

    [Test]
    public void FindAll_CompositePK_FilterOnSingleColumn_ReturnsExpectedCount()
    {
        var project = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == projectId);

        // Act - filter only on one column of composite key (like: x => x.UserId == user.Id)
        var result = entityRepository.FindAll<ProjectParticipant>(x => x.ProjectId == project.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3), "FindAll filtering on single column of composite key should return all matching rows");
        NUnit.Framework.TestContext.WriteLine($"[CompositePK FilterOnSingleColumn] FindAll returned {result.Count} result(s)");
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

    // ──────────────────────────────────────────────────────────
    // COMPOSITE KEY KEYSET PAGINATION – COMPLETENESS TESTS
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts many rows with composite PK across multiple projects,
    /// then verifies FindAll returns every single row without omissions or duplicates.
    /// Uses ProjectParticipant (ClientId, ProjectId).
    /// </summary>
    [Test]
    public void FindAll_CompositeKey_NoOmissionsOrDuplicates_ProjectParticipant()
    {
        // Arrange – insert 5 projects x 50 participants = 250 composite key rows
        var projectIds = new List<long>();
        for (int p = 0; p < 5; p++)
        {
            var aggId = Guid.NewGuid();
            entityRepository.Insert(new ProjectReporting { AggregateRootId = aggId });
            var proj = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggId);
            projectIds.Add(proj.Id);

            for (int c = 1; c <= 50; c++)
            {
                entityRepository.Insert(new ProjectParticipant { ProjectId = proj.Id, ClientId = 10000 + (p * 100) + c });
            }
        }

        // Act – FindAll uses keyset pagination internally (fetchSize=2000, but we can verify correctness)
        var allParticipants = entityRepository.FindAll<ProjectParticipant>(
            x => projectIds.Contains(x.ProjectId));

        // Assert
        Assert.That(allParticipants.Count, Is.EqualTo(250), "Should return all 250 participants");

        var keys = allParticipants.Select(x => (x.ClientId, x.ProjectId)).ToList();
        var uniqueKeys = keys.Distinct().ToList();
        Assert.That(uniqueKeys.Count, Is.EqualTo(250), "No duplicate composite keys");

        NUnit.Framework.TestContext.WriteLine($"[ProjectParticipant] FindAll composite key: {allParticipants.Count} rows, {uniqueKeys.Count} unique keys, 0 duplicates");
    }

    /// <summary>
    /// Same as above but with FindAllAsync.
    /// </summary>
    [Test]
    public async Task FindAllAsync_CompositeKey_NoOmissionsOrDuplicates_ProjectParticipant()
    {
        // Arrange – insert 5 projects x 50 participants = 250 composite key rows
        var projectIds = new List<long>();
        for (int p = 0; p < 5; p++)
        {
            var aggId = Guid.NewGuid();
            entityRepository.Insert(new ProjectReporting { AggregateRootId = aggId });
            var proj = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggId);
            projectIds.Add(proj.Id);

            for (int c = 1; c <= 50; c++)
            {
                entityRepository.Insert(new ProjectParticipant { ProjectId = proj.Id, ClientId = 20000 + (p * 100) + c });
            }
        }

        // Act
        var allParticipants = await entityRepository.FindAllAsync<ProjectParticipant>(
            x => projectIds.Contains(x.ProjectId));

        // Assert
        Assert.That(allParticipants.Count, Is.EqualTo(250), "Should return all 250 participants");

        var keys = allParticipants.Select(x => (x.ClientId, x.ProjectId)).ToList();
        var uniqueKeys = keys.Distinct().ToList();
        Assert.That(uniqueKeys.Count, Is.EqualTo(250), "No duplicate composite keys");

        NUnit.Framework.TestContext.WriteLine($"[ProjectParticipant] FindAllAsync composite key: {allParticipants.Count} rows, {uniqueKeys.Count} unique keys, 0 duplicates");
    }

    /// <summary>
    /// Tests ProjectSupplier (SupplierId, ProjectId) composite key with FindAll.
    /// </summary>
    [Test]
    public void FindAll_CompositeKey_NoOmissionsOrDuplicates_ProjectSupplier()
    {
        // Arrange – insert 5 projects x 40 suppliers = 200 composite key rows
        var projectIds = new List<long>();
        for (int p = 0; p < 5; p++)
        {
            var aggId = Guid.NewGuid();
            entityRepository.Insert(new ProjectReporting { AggregateRootId = aggId });
            var proj = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggId);
            projectIds.Add(proj.Id);

            for (int s = 1; s <= 40; s++)
            {
                entityRepository.Insert(new ProjectSupplier { ProjectId = proj.Id, SupplierId = 30000 + (p * 100) + s });
            }
        }

        // Act
        var allSuppliers = entityRepository.FindAll<ProjectSupplier>(
            x => projectIds.Contains(x.ProjectId));

        // Assert
        Assert.That(allSuppliers.Count, Is.EqualTo(200), "Should return all 200 suppliers");

        var keys = allSuppliers.Select(x => (x.SupplierId, x.ProjectId)).ToList();
        var uniqueKeys = keys.Distinct().ToList();
        Assert.That(uniqueKeys.Count, Is.EqualTo(200), "No duplicate composite keys");

        NUnit.Framework.TestContext.WriteLine($"[ProjectSupplier] FindAll composite key: {allSuppliers.Count} rows, {uniqueKeys.Count} unique keys, 0 duplicates");
    }

    /// <summary>
    /// Stress test: inserts enough composite key rows to force multiple internal keyset pages
    /// (fetchSize=2000 internally), then verifies completeness.
    /// </summary>
    [Test]
    public async Task FindAllAsync_CompositeKey_MultiplePages_NoOmissionsOrDuplicates()
    {
        // Arrange – insert 10 projects x 500 participants = 5000 rows → 3 internal pages at fetchSize=2000
        var projectIds = new List<long>();
        for (int p = 0; p < 10; p++)
        {
            var aggId = Guid.NewGuid();
            entityRepository.Insert(new ProjectReporting { AggregateRootId = aggId });
            var proj = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggId);
            projectIds.Add(proj.Id);

            for (int c = 1; c <= 500; c++)
            {
                entityRepository.Insert(new ProjectParticipant { ProjectId = proj.Id, ClientId = 40000 + (p * 1000) + c });
            }
        }

        // Act
        var allParticipants = await entityRepository.FindAllAsync<ProjectParticipant>(
            x => projectIds.Contains(x.ProjectId));

        // Assert
        Assert.That(allParticipants.Count, Is.EqualTo(5000), "Should return all 5000 participants across 3+ keyset pages");

        var keys = allParticipants.Select(x => (x.ClientId, x.ProjectId)).ToList();
        var uniqueKeys = keys.Distinct().ToList();
        Assert.That(uniqueKeys.Count, Is.EqualTo(5000), "No duplicate composite keys");

        // Verify ordering (should be ordered by first PK column = ClientId, then ProjectId)
        for (int i = 1; i < allParticipants.Count; i++)
        {
            var prev = allParticipants[i - 1];
            var curr = allParticipants[i];
            var prevTuple = (prev.ClientId, prev.ProjectId);
            var currTuple = (curr.ClientId, curr.ProjectId);
            Assert.That(currTuple, Is.GreaterThan(prevTuple),
                $"Row {i} should be > row {i - 1}: ({curr.ClientId},{curr.ProjectId}) vs ({prev.ClientId},{prev.ProjectId})");
        }

        NUnit.Framework.TestContext.WriteLine($"[ProjectParticipant] FindAllAsync multi-page composite key: {allParticipants.Count} rows, {uniqueKeys.Count} unique, 0 duplicates, ordering verified");
    }
}

