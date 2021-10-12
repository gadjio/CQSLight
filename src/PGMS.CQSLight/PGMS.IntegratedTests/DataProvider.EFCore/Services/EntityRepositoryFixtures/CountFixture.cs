using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;
using PGMS.IntegratedTests.DataProvider.EFCore.Services.UnitOfWorkFixtures;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures
{
	[TestFixture]
	public class ContainsWithTooMuchItems
	{
		private IEntityRepository entityRepository;
		private string connectionString = "Server=localhost;Database=SampleProject;Trusted_Connection=True;ConnectRetryCount=0";

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

	[TestFixture]
	public class CountFixture
	{
		private IEntityRepository entityRepository;
		private string connectionString = "Server=localhost;Database=SampleProject;Trusted_Connection=True;ConnectRetryCount=0";

		[SetUp]
		public void SetUp()
		{
			entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				((TestContext) unitOfWork.GetDbContext()).Database.EnsureCreated();
			}

			var existing = entityRepository.FindAll<DbSequenceHiLo>();

			if (existing.Any(x => x.id_parametres == "Test_10X") == false)
			{
				entityRepository.Insert(new DbSequenceHiLo { id_parametres = "Test_10X" });
				entityRepository.Insert(new DbSequenceHiLo { id_parametres = "Test_10X" });
				entityRepository.Insert(new DbSequenceHiLo { id_parametres = "Test_10X" });
			}

			if (existing.Any(x => x.id_parametres == "Test_20X") == false)
			{
				entityRepository.Insert(new DbSequenceHiLo { id_parametres = "Test_20X" });
			}
		}


		[Test]
		public void CountGroupBy()
		{

			var result = entityRepository.Count<DbSequenceHiLo, string>(null, x => x.id_parametres);

			Console.WriteLine(JsonConvert.SerializeObject(result).JsonPrettify());

			Assert.That(result.Keys.Contains("Test_10X"), Is.True, "Test_10X is missing");
			Assert.That(result.Keys.Contains("Test_20X"), Is.True, "Test_20X is missing");
			
			Assert.That(result["Test_10X"], Is.EqualTo(3));
			Assert.That(result["Test_20X"], Is.EqualTo(1));

			Assert.That(result.Count, Is.AtLeast(2));
		}
	}
}