using System;
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