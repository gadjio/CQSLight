using System;
using System.Collections.Generic;
using NUnit.Framework;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;
using PGMS.FakeImpl.DataProvider.Context;
using TestContext = PGMS.FakeImpl.DataProvider.Context.TestContext;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class ContainsWithTooMuchItems
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

	[TestCase(3000)]
	[TestCase(5000)]
	[TestCase(10000)]
	public void Test_contains_with_many_items(int listSize)
	{
		var list = new List<Guid>();

		for (int i = 0; i < listSize; i++)
		{
			list.Add(Guid.NewGuid());
		}


		var result = entityRepository.FindAll<PersonReporting>(x => list.Contains(x.AggregateRootId));

		Assert.That(result.Count, Is.EqualTo(0));
	}
}