using System;
using NUnit.Framework;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;
using System.Collections.Generic;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class BulkInsertOperationAsync
{
    private IEntityRepository entityRepository;
    private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

    [SetUp]
    public void SetUp()
    {
        entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
        using (var unitOfWork = entityRepository.GetUnitOfWork())
        {
            ((TestContext)unitOfWork.GetDbContext()).Database.EnsureCreated();
        }
    }

    [TestCase(1000)]
    [TestCase(2000)]
    public void Test(int listSize)
    {
        using var unitOfWork = entityRepository.GetUnitOfWork();

        var start = DateTime.Now;
        for (int i = 0; i < listSize; i++)
        {
            entityRepository.Insert(new LogEntryReporting { AggregateRootId = Guid.NewGuid() });
        }
        var elapsedClassic = DateTime.Now - start;
        Console.WriteLine($"Elapsed Classic : {elapsedClassic.TotalMilliseconds} ms");

        var list = new List<LogEntryReporting>();
        for (int i = 0; i < listSize; i++)
        {
            list.Add(new LogEntryReporting{AggregateRootId = Guid.NewGuid()});
        }
        var startBulk = DateTime.Now;
        var task = entityRepository.BulkInsertOperationAsync(unitOfWork, list);
        task.Wait();
        var elapsedBulk = DateTime.Now - startBulk;
        Console.WriteLine($"Elapsed Bulk : {elapsedBulk.TotalMilliseconds} ms");
    }
}