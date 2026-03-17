using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.QABenchmark;

/// <summary>
/// Uniqueness/regression tests — verifies that paginated results contain no duplicates
/// and no missing rows when using the PK-based default OrderBy on large real tables.
/// 
/// These tests are skipped automatically when no QA_CONNECTION_STRING is configured.
/// </summary>
[TestFixture]
[Category("Benchmark")]
public class PaginationUniquenessFixture
{
    private BaseEntityRepository<QABenchmarkContext> entityRepository;

    [SetUp]
    public void SetUp()
    {
        var connStr = EnvConnectionStringHelper.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connStr))
        {
            Assert.Ignore("QA_CONNECTION_STRING not configured. Skipping uniqueness tests. See .env.example.");
        }

        entityRepository = new BaseEntityRepository<QABenchmarkContext>(
            new ConnectionStringProvider(connStr),
            new QABenchmarkContextFactory());
    }

    // ──────────────────────────────────────────────────────────
    // UNIQUENESS: No duplicates across pages
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Uniqueness_DomainEventReporting_NoDuplicatesAcrossPages()
    {
        const int fetchSize = 2000;
        const int totalPages = 5;

        var allIds = new HashSet<long>();
        var duplicates = new List<long>();
        long? previousMaxId = null;

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < totalPages; i++)
        {
            var page = await entityRepository.GetAsync<DomainEventReportingQA>(
                null, null, fetchSize, i * fetchSize);

            Assert.That(page.Count, Is.EqualTo(fetchSize), $"Page {i + 1} should return {fetchSize} rows");

            // Verify ordering within page
            var ids = page.Select(x => x.Id).ToList();
            Assert.That(ids, Is.Ordered, $"Page {i + 1} should have IDs in ascending order");

            // Verify no overlap with previous page
            if (previousMaxId.HasValue)
            {
                Assert.That(ids.First(), Is.GreaterThan(previousMaxId.Value),
                    $"Page {i + 1} first ID ({ids.First()}) should be greater than previous page max ID ({previousMaxId.Value})");
            }
            previousMaxId = ids.Last();

            // Track duplicates
            foreach (var id in ids)
            {
                if (!allIds.Add(id))
                    duplicates.Add(id);
            }
        }

        var elapsed = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"[DomainEventReporting] Uniqueness check: {totalPages} pages x {fetchSize} rows");
        TestContext.WriteLine($"  Total unique IDs: {allIds.Count}");
        TestContext.WriteLine($"  Duplicates: {duplicates.Count}");
        TestContext.WriteLine($"  Time: {elapsed} ms");

        Assert.That(duplicates, Is.Empty, $"Found {duplicates.Count} duplicate IDs across pages");
        Assert.That(allIds.Count, Is.EqualTo(totalPages * fetchSize));
    }

    [Test]
    public async Task Uniqueness_CQSDomainEvents_NoDuplicatesAcrossPages()
    {
        const int fetchSize = 2000;
        const int totalPages = 5;

        var allIds = new HashSet<long>();
        var duplicates = new List<long>();
        long? previousMaxId = null;

        for (int i = 0; i < totalPages; i++)
        {
            var page = await entityRepository.GetAsync<CQSDomainEventQA>(
                null, null, fetchSize, i * fetchSize);

            Assert.That(page.Count, Is.EqualTo(fetchSize), $"Page {i + 1} should return {fetchSize} rows");

            var ids = page.Select(x => x.Id).ToList();
            Assert.That(ids, Is.Ordered, $"Page {i + 1} should have IDs in ascending order");

            if (previousMaxId.HasValue)
            {
                Assert.That(ids.First(), Is.GreaterThan(previousMaxId.Value),
                    $"Page {i + 1} first ID should be > previous page max ID");
            }
            previousMaxId = ids.Last();

            foreach (var id in ids)
            {
                if (!allIds.Add(id))
                    duplicates.Add(id);
            }
        }

        TestContext.WriteLine($"[CQSDomainEvents] Uniqueness check: {totalPages} pages x {fetchSize} rows");
        TestContext.WriteLine($"  Total unique IDs: {allIds.Count}");
        TestContext.WriteLine($"  Duplicates: {duplicates.Count}");

        Assert.That(duplicates, Is.Empty);
        Assert.That(allIds.Count, Is.EqualTo(totalPages * fetchSize));
    }

    [Test]
    public async Task Uniqueness_FindAllAsync_DomainEventReporting_SubsetConsistency()
    {
        // First, find a valid ID range by fetching the first page
        var firstPage = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, 1, 0);
        if (firstPage.Count == 0)
        {
            Assert.Ignore("No data in DomainEventReporting table");
            return;
        }
        var minId = firstPage.First().Id;

        // FindAll uses internal loop of GetOperation with Skip/Take
        // Verify that a subset of rows has no duplicates
        var sw = Stopwatch.StartNew();
        var result = await entityRepository.FindAllAsync<DomainEventReportingQA>(
            x => x.Id >= minId && x.Id < minId + 10000);
        var elapsed = sw.ElapsedMilliseconds;

        var ids = result.Select(x => x.Id).ToList();
        var uniqueIds = ids.Distinct().ToList();

        TestContext.WriteLine($"[DomainEventReporting] FindAllAsync subset (Id {minId} to {minId + 10000})");
        TestContext.WriteLine($"  Rows: {result.Count}, Unique IDs: {uniqueIds.Count}");
        TestContext.WriteLine($"  Time: {elapsed} ms");

        Assert.That(result.Count, Is.GreaterThan(0), "Should find some rows in the range");
        Assert.That(ids.Count, Is.EqualTo(uniqueIds.Count), "FindAllAsync should return no duplicate IDs");
        Assert.That(ids, Is.Ordered, "FindAllAsync results should be ordered by PK");
    }
}

