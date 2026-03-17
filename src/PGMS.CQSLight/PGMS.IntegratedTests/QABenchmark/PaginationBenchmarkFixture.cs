using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.QABenchmark;

/// <summary>
/// Benchmark tests against real Azure SQL data (5.4M rows DomainEventReporting, 1.2M rows CQSDomainEvents).
/// These tests are skipped automatically when no QA_CONNECTION_STRING is configured.
/// 
/// Setup: copy .env.example to .env at repo root and fill in the connection string.
/// Or set QA_CONNECTION_STRING as an environment variable.
/// </summary>
[TestFixture]
[Category("Benchmark")]
public class PaginationBenchmarkFixture
{
    private BaseEntityRepository<QABenchmarkContext> entityRepository;

    [SetUp]
    public void SetUp()
    {
        var connStr = EnvConnectionStringHelper.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connStr))
        {
            Assert.Ignore("QA_CONNECTION_STRING not configured. Skipping benchmark tests. See .env.example.");
        }

        entityRepository = new BaseEntityRepository<QABenchmarkContext>(
            new ConnectionStringProvider(connStr),
            new QABenchmarkContextFactory());
    }

    // ──────────────────────────────────────────────────────────
    // PERFORMANCE BENCHMARKS
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_GetAsync_DomainEventReporting_WithPKOrderBy_Performance()
    {
        const int fetchSize = 5000;
        var sw = Stopwatch.StartNew();

        // No orderBy => falls back to PK OrderBy (Id)
        var page1 = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, fetchSize, 0);
        var time1 = sw.ElapsedMilliseconds;
        sw.Restart();

        var page2 = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, fetchSize, fetchSize);
        var time2 = sw.ElapsedMilliseconds;
        sw.Restart();

        var page3 = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, fetchSize, fetchSize * 2);
        var time3 = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"[DomainEventReporting ~5.4M rows] PK OrderBy (Id) - fetchSize={fetchSize}");
        TestContext.WriteLine($"  Page 1 (offset 0):          {time1} ms  ({page1.Count} rows)");
        TestContext.WriteLine($"  Page 2 (offset {fetchSize}):     {time2} ms  ({page2.Count} rows)");
        TestContext.WriteLine($"  Page 3 (offset {fetchSize * 2}): {time3} ms  ({page3.Count} rows)");

        Assert.That(page1.Count, Is.EqualTo(fetchSize));
        Assert.That(page2.Count, Is.EqualTo(fetchSize));
        Assert.That(page3.Count, Is.EqualTo(fetchSize));
    }

    [Test]
    public async Task Benchmark_GetAsync_CQSDomainEvents_WithPKOrderBy_Performance()
    {
        const int fetchSize = 5000;
        var sw = Stopwatch.StartNew();

        var page1 = await entityRepository.GetAsync<CQSDomainEventQA>(null, null, fetchSize, 0);
        var time1 = sw.ElapsedMilliseconds;
        sw.Restart();

        var page2 = await entityRepository.GetAsync<CQSDomainEventQA>(null, null, fetchSize, fetchSize);
        var time2 = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"[CQSDomainEvents ~1.2M rows] PK OrderBy (Id) - fetchSize={fetchSize}");
        TestContext.WriteLine($"  Page 1 (offset 0):      {time1} ms  ({page1.Count} rows)");
        TestContext.WriteLine($"  Page 2 (offset {fetchSize}): {time2} ms  ({page2.Count} rows)");

        Assert.That(page1.Count, Is.EqualTo(fetchSize));
        Assert.That(page2.Count, Is.EqualTo(fetchSize));
    }

    [Test]
    public async Task Benchmark_GetAsync_DomainEventReporting_WithExplicitOrderBy_Comparison()
    {
        // Use smaller fetch size to avoid timeout on non-indexed column sort
        const int fetchSize = 500;

        // With explicit OrderBy on Timestamp (non-indexed column)
        // This may timeout on large tables — that itself proves PK ordering is better
        string timestampResult;
        var sw = Stopwatch.StartNew();
        try
        {
            var pageTimestamp = await entityRepository.GetAsync<DomainEventReportingQA>(
                null, q => q.OrderBy(e => e.Timestamp), fetchSize, 0);
            timestampResult = $"{sw.ElapsedMilliseconds} ms ({pageTimestamp.Count} rows)";
        }
        catch (Exception ex)
        {
            timestampResult = $"TIMEOUT/ERROR after {sw.ElapsedMilliseconds} ms — {ex.GetType().Name}";
        }

        // With PK OrderBy (default - indexed)
        sw.Restart();
        var pagePK = await entityRepository.GetAsync<DomainEventReportingQA>(
            null, null, fetchSize, 0);
        var timePK = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"[DomainEventReporting] Performance comparison - fetchSize={fetchSize}");
        TestContext.WriteLine($"  OrderBy Timestamp (non-PK): {timestampResult}");
        TestContext.WriteLine($"  OrderBy Id (PK, default):   {timePK} ms ({pagePK.Count} rows)");

        // The PK query should always succeed
        Assert.That(pagePK.Count, Is.EqualTo(fetchSize));
    }

    // ──────────────────────────────────────────────────────────
    // DEEP PAGINATION BENCHMARK (high offset)
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_GetAsync_DomainEventReporting_DeepPagination()
    {
        const int fetchSize = 2000;
        const int deepOffset = 100_000;

        var sw = Stopwatch.StartNew();
        var page = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, fetchSize, deepOffset);
        var elapsed = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"[DomainEventReporting] Deep pagination offset={deepOffset}, fetchSize={fetchSize}");
        TestContext.WriteLine($"  Time: {elapsed} ms  ({page.Count} rows)");

        Assert.That(page.Count, Is.EqualTo(fetchSize));
    }

    // ──────────────────────────────────────────────────────────
    // FindAllAsync — large subset (internal loop of 2000-row pages)
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_FindAllAsync_DomainEventReporting_LargeSubset()
    {
        // First, find a valid starting ID
        var firstPage = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, 1, 0);
        Assert.That(firstPage.Count, Is.GreaterThan(0), "Table must have data");
        var minId = firstPage.First().Id;

        // FindAllAsync internally loops with pages of 2000 using GetOperationAsync
        // Use a range that should return ~50K rows to exercise multiple internal pages
        const long idRange = 500_000;
        var sw = Stopwatch.StartNew();
        var result = await entityRepository.FindAllAsync<DomainEventReportingQA>(
            x => x.Id >= minId && x.Id < minId + idRange);
        var elapsed = sw.ElapsedMilliseconds;

        var ids = result.Select(x => x.Id).ToList();
        var uniqueCount = ids.Distinct().Count();
        var internalPages = (int)Math.Ceiling(result.Count / 2000.0);

        TestContext.WriteLine($"[DomainEventReporting] FindAllAsync large subset (Id {minId} to {minId + idRange})");
        TestContext.WriteLine($"  Total rows fetched: {result.Count}");
        TestContext.WriteLine($"  Internal pages (2000/page): {internalPages}");
        TestContext.WriteLine($"  Unique IDs: {uniqueCount}");
        TestContext.WriteLine($"  Duplicates: {result.Count - uniqueCount}");
        TestContext.WriteLine($"  Time: {elapsed} ms");
        TestContext.WriteLine($"  Avg per internal page: {(internalPages > 0 ? elapsed / internalPages : 0)} ms");

        Assert.That(result.Count, Is.GreaterThan(0), "Should find rows in the range");
        Assert.That(ids.Count, Is.EqualTo(uniqueCount), "FindAllAsync should return no duplicate IDs");
        Assert.That(ids, Is.Ordered, "FindAllAsync results should be ordered by PK");
    }

    // ──────────────────────────────────────────────────────────
    // FindTopAsync — large fetch size + high offset
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_FindTopAsync_DomainEventReporting_LargeFetchAndOffset()
    {
        const int fetchSize = 10_000;
        const int offset = 50_000;

        var sw = Stopwatch.StartNew();
        var result = await entityRepository.FindTopAsync<DomainEventReportingQA>(
            null, null, fetchSize, offset);
        var elapsed = sw.ElapsedMilliseconds;

        var ids = result.Select(x => x.Id).ToList();
        var uniqueCount = ids.Distinct().Count();

        TestContext.WriteLine($"[DomainEventReporting] FindTopAsync fetchSize={fetchSize}, offset={offset}");
        TestContext.WriteLine($"  Rows: {result.Count}");
        TestContext.WriteLine($"  Unique IDs: {uniqueCount}");
        TestContext.WriteLine($"  Duplicates: {result.Count - uniqueCount}");
        TestContext.WriteLine($"  Time: {elapsed} ms");

        Assert.That(result.Count, Is.EqualTo(fetchSize));
        Assert.That(ids.Count, Is.EqualTo(uniqueCount), "FindTopAsync should return no duplicate IDs");
        Assert.That(ids, Is.Ordered, "FindTopAsync results should be ordered by PK");
    }

    [Test]
    public async Task Benchmark_FindTopAsync_DomainEventReporting_VeryDeepOffset()
    {
        const int fetchSize = 5_000;
        const int offset = 500_000;

        string offsetResult;
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await entityRepository.FindTopAsync<DomainEventReportingQA>(
                null, null, fetchSize, offset);
            var elapsed = sw.ElapsedMilliseconds;

            var ids = result.Select(x => x.Id).ToList();
            var uniqueCount = ids.Distinct().Count();

            offsetResult = $"{elapsed} ms ({result.Count} rows, {uniqueCount} unique)";
            TestContext.WriteLine($"[DomainEventReporting] FindTopAsync DEEP OFFSET fetchSize={fetchSize}, offset={offset}");
            TestContext.WriteLine($"  {offsetResult}");
        }
        catch (Exception ex)
        {
            offsetResult = $"TIMEOUT/ERROR after {sw.ElapsedMilliseconds} ms — {ex.GetType().Name}";
            TestContext.WriteLine($"[DomainEventReporting] FindTopAsync DEEP OFFSET fetchSize={fetchSize}, offset={offset}");
            TestContext.WriteLine($"  {offsetResult}");
        }

        // Compare: keyset pagination to reach the same depth
        TestContext.WriteLine($"  (See Benchmark_KeysetPagination_DeepTraversal for keyset alternative)");
    }

    // ──────────────────────────────────────────────────────────
    // KEYSET PAGINATION — sequential deep traversal
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_KeysetPagination_DomainEventReporting_DeepTraversal()
    {
        // Traverse to the equivalent of offset 500K using keyset pagination (WHERE Id > lastId)
        // Each page uses offset=0, so SQL only needs to scan fetchSize rows per page
        const int fetchSize = 5_000;
        const int targetRows = 500_000; // equivalent to reaching offset 500K
        var totalPages = targetRows / fetchSize;

        var sw = Stopwatch.StartNew();
        var allIds = new HashSet<long>();
        long lastId = 0;
        int pagesRead = 0;

        for (int i = 0; i < totalPages; i++)
        {
            // Keyset filter: WHERE Id > lastId ORDER BY Id FETCH 5000
            var page = await entityRepository.GetAsync<DomainEventReportingQA>(
                x => x.Id > lastId, null, fetchSize, 0);

            if (page.Count == 0)
                break;

            foreach (var row in page)
                allIds.Add(row.Id);

            lastId = page[page.Count - 1].Id;
            pagesRead++;
        }

        var elapsed = sw.ElapsedMilliseconds;

        // Now fetch the final page (equivalent to "the page at offset 500K")
        sw.Restart();
        var finalPage = await entityRepository.GetAsync<DomainEventReportingQA>(
            x => x.Id > lastId, null, fetchSize, 0);
        var finalPageTime = sw.ElapsedMilliseconds;

        var finalIds = finalPage.Select(x => x.Id).ToList();
        var finalUniqueCount = finalIds.Distinct().Count();

        TestContext.WriteLine($"[DomainEventReporting] KEYSET pagination to depth {targetRows}");
        TestContext.WriteLine($"  Pages traversed: {pagesRead} x {fetchSize} rows");
        TestContext.WriteLine($"  Total traversal time: {elapsed} ms");
        TestContext.WriteLine($"  Avg per page: {(pagesRead > 0 ? elapsed / pagesRead : 0)} ms");
        TestContext.WriteLine($"  Total unique IDs seen: {allIds.Count}");
        TestContext.WriteLine($"  --- Final page (at depth ~{targetRows}) ---");
        TestContext.WriteLine($"  Rows: {finalPage.Count}, Unique: {finalUniqueCount}");
        TestContext.WriteLine($"  Time for final page: {finalPageTime} ms");

        Assert.That(finalPage.Count, Is.EqualTo(fetchSize), "Final keyset page should return full fetch size");
        Assert.That(finalIds.Count, Is.EqualTo(finalUniqueCount), "No duplicates in final page");
        Assert.That(finalIds, Is.Ordered, "Final page should be ordered by PK");
    }

    // ──────────────────────────────────────────────────────────
    // FindAllAsync with KEYSET — large subset performance
    // ──────────────────────────────────────────────────────────

    [Test]
    public async Task Benchmark_FindAllAsync_DomainEventReporting_LargeSubset_WithKeyset()
    {
        // FindAllAsync now uses keyset pagination internally (WHERE Id > lastId)
        // This should be significantly faster than offset-based for large result sets
        var firstPage = await entityRepository.GetAsync<DomainEventReportingQA>(null, null, 1, 0);
        Assert.That(firstPage.Count, Is.GreaterThan(0), "Table must have data");
        var minId = firstPage.First().Id;

        const long idRange = 500_000;
        var sw = Stopwatch.StartNew();
        var result = await entityRepository.FindAllAsync<DomainEventReportingQA>(
            x => x.Id >= minId && x.Id < minId + idRange);
        var elapsed = sw.ElapsedMilliseconds;

        var ids = result.Select(x => x.Id).ToList();
        var uniqueCount = ids.Distinct().Count();
        var internalPages = (int)Math.Ceiling(result.Count / 2000.0);

        TestContext.WriteLine($"[DomainEventReporting] FindAllAsync KEYSET subset (Id {minId} to {minId + idRange})");
        TestContext.WriteLine($"  Total rows fetched: {result.Count}");
        TestContext.WriteLine($"  Internal pages (2000/page): {internalPages}");
        TestContext.WriteLine($"  Unique IDs: {uniqueCount}");
        TestContext.WriteLine($"  Duplicates: {result.Count - uniqueCount}");
        TestContext.WriteLine($"  Time: {elapsed} ms");
        TestContext.WriteLine($"  Avg per internal page: {(internalPages > 0 ? elapsed / internalPages : 0)} ms");

        Assert.That(result.Count, Is.GreaterThan(0), "Should find rows in the range");
        Assert.That(ids.Count, Is.EqualTo(uniqueCount), "FindAllAsync keyset should return no duplicate IDs");
        Assert.That(ids, Is.Ordered, "FindAllAsync keyset results should be ordered by PK");
    }
}

