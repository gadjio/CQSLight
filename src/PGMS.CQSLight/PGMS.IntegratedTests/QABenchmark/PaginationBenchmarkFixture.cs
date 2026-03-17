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
}

