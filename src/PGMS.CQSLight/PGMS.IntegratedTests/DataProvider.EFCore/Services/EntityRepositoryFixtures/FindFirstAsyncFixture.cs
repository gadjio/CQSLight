using System;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class FindFirstAsyncFixture
{
    private IEntityRepository entityRepository;
    private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

    private long unixTimeStampMs;

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

        if (entityRepository.FindFirst<PersonReporting>(x => x.AggregateRootId == QADataProvider.AngelaMartinId) == null)
        {
            entityRepository.Insert(new PersonReporting { AggregateRootId = QADataProvider.AngelaMartinId, Name = "Angela" });
        }

        if (entityRepository.FindFirst<PersonReporting>(x => x.AggregateRootId == QADataProvider.CreedBrattonId) == null)
        {
            entityRepository.Insert(new PersonReporting { AggregateRootId = QADataProvider.CreedBrattonId, Name = "Creed" });
        }

        if (entityRepository.FindFirst<PersonReporting>(x => x.AggregateRootId == QADataProvider.DwightSchruteId) == null)
        {
            entityRepository.Insert(new PersonReporting { AggregateRootId = QADataProvider.DwightSchruteId, Name = "Dwight" });
        }

        unixTimeStampMs = DateTime.Now.ToEpochInMilliseconds();
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.AngelaMartinId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.CreedBrattonId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });
        entityRepository.Insert(new LogEntryReporting { PersonId = QADataProvider.DwightSchruteId, UnixTimeStampMs = unixTimeStampMs, LocationId = QADataProvider.LocationId });

    }

    [Test]
    public void Test()
    {
        var task = entityRepository.FindFirstAsync<LogEntryReporting>(x => x.PersonId == QADataProvider.CreedBrattonId);
        task.Wait();
        Assert.That(task.Result, Is.Not.Null);
    }
}