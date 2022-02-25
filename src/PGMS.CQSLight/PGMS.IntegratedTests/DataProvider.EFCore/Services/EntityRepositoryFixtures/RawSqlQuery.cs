using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures
{
	[TestFixture]
	public class RawSqlQuery
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

			var location = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == QADataProvider.LocationId);
			if (location == null)
			{
				entityRepository.Insert(new LocationReporting { AggregateRootId = QADataProvider.LocationId, Name = "UnitTests" });
			}

			var location2 = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == QADataProvider.Location2Id);
			if (location2 == null)
			{
				entityRepository.Insert(new LocationReporting { AggregateRootId = QADataProvider.Location2Id, Name = "Moon Base 2" });
			}

			var location3 = entityRepository.FindFirst<LocationReporting>(x => x.AggregateRootId == QADataProvider.Location3Id);
			if (location3 == null)
			{
				entityRepository.Insert(new LocationReporting { AggregateRootId = QADataProvider.Location3Id, Name = "Moon Base 3 " });
			}
		}

		[Test]
		public void Test()
		{
			//Act
			using var unitOfWork = entityRepository.GetUnitOfWork();
			var result = entityRepository.RawSqlQuery(unitOfWork, "SELECT AggregateRootId, Name from LOCATIONS", x => new LocationSummary { Name = (string)x[1], Id = (Guid)x[0] });

			//assert
			Console.WriteLine(JsonConvert.SerializeObject(result).JsonPrettify());
			Assert.That(result.Any(x => x.Id == QADataProvider.LocationId));
			Assert.That(result.Any(x => x.Id == QADataProvider.Location2Id));
			Assert.That(result.Any(x => x.Id == QADataProvider.Location3Id));

			Assert.That(result.Count, Is.AtLeast(3));
		}
	}

	public class LocationSummary
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}