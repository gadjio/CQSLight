using System;
using NUnit.Framework;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;
using PGMS.FakeImpl.DataProvider.Context;
using TestContext = PGMS.FakeImpl.DataProvider.Context.TestContext;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class SetCurrentUnitOfWorkFixture
{
	private IEntityRepository entityRepository;
	private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

	private IUnitOfWork unitOfWork;
	private IUnitOfWorkTransaction transaction;

	[SetUp]
	public void SetUp()
	{
		entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
		
		unitOfWork = entityRepository.GetUnitOfWork();
		unitOfWork.KeepAlive = true;
		transaction = unitOfWork.GetTransaction();

		entityRepository.SetCurrentUnitOfWork(unitOfWork);
	}

	[TearDown]
	public void TearDown()
	{
		transaction.Rollback();
		unitOfWork.KeepAlive = false;
		unitOfWork.Dispose();
	}

	[Test]
	public void GetFromTransaction()
	{
		var aggregateRootId = Guid.NewGuid();
		entityRepository.InsertOperation(unitOfWork, new LocationReporting { AggregateRootId = aggregateRootId, Name = "UnitTests" });

		var result = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == aggregateRootId);
		Assert.That(result, Is.Not.Null);
	}

	[Test]
	public void GetFromTransaction_UnitOfWorkNotDisposed()
	{
		var aggregateRootId = Guid.NewGuid();
		entityRepository.InsertOperation(unitOfWork, new LocationReporting { AggregateRootId = aggregateRootId, Name = "UnitTests" });

		var result1 = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == aggregateRootId);
		var result2 = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == aggregateRootId);
		var result3 = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == aggregateRootId);

		Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result3, Is.Not.Null);
    }
}