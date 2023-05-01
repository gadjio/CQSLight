using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Helpers;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures
{
    [TestFixture]
	public class JoinFixture
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
				entityRepository.Insert(new PersonReporting { AggregateRootId = QADataProvider.AngelaMartinId, Name = "Angela"});
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
		public void With_Values()
		{

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var result = entityRepository.GetJoinOperation<PersonReporting, LogEntryReporting, Guid>(unitOfWork,
					null,
					i => i.PersonId != null && i.UnixTimeStampMs == unixTimeStampMs,
					x => x.AggregateRootId, i => i.PersonId,
					x => x.OrderByDescending(i => i.Name),
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void Without_Values()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var result = entityRepository.GetJoinOperation<PersonReporting, LogEntryReporting, Guid>(unitOfWork,
					null,
					i => i.PersonId != null &&  i.UnixTimeStampMs == 101,
					x => x.AggregateRootId, i => i.PersonId,
					x => x.OrderByDescending(i => i.Name),
					2, 0);

				Assert.That(result.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public void QueryBuilder_With_Values()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var inner1 = entityRepository.GetQuery<LogEntryReporting>(unitOfWork, i =>  i.UnixTimeStampMs == unixTimeStampMs);
				var joined = entityRepository.JoinQueries<PersonReporting, LogEntryReporting, Guid>(query, inner1,
					x => x.AggregateRootId, i => i.PersonId);

				Console.WriteLine(EfCoreHelper.ToSql(joined));

				//Act
				var innerResult = entityRepository.FetchQuery(inner1);
				var result = entityRepository.FetchQuery<PersonReporting>(joined);

				//Assert
				Assert.That(innerResult.Count, Is.EqualTo(3));
				Assert.That(result.Count, Is.EqualTo(3));
			}
		}

		[Test]
		public void TResult_SingleJoin()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var join = entityRepository.GetJoinQuery<PersonReporting, LogEntryReporting, Guid>(unitOfWork, query,
					i => i.PersonId != null  && i.UnixTimeStampMs == unixTimeStampMs,
					x => x.AggregateRootId, i => i.PersonId);


				var result = entityRepository.FetchQuery<TResult<PersonReporting, LogEntryReporting>>(
					join, null, 2, 0);


				Console.WriteLine(JsonConvert.SerializeObject(result).JsonPrettify());
				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void SingleJoin()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var personQuery = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var logQuery = entityRepository.GetQuery<LogEntryReporting>(unitOfWork, i => i.PersonId != null && i.UnixTimeStampMs == unixTimeStampMs);
				var join = entityRepository.JoinQueries<PersonReporting, LogEntryReporting, Guid>(personQuery, logQuery,
					x => x.AggregateRootId, i => i.PersonId);


				var result = entityRepository.FetchQuery<PersonReporting>(join, null, 2, 0);


				Console.WriteLine(JsonConvert.SerializeObject(result).JsonPrettify());
				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		[Ignore("Order by is not working")]
		public void TResult_SingleJoin_With_orderBy()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var join = entityRepository.GetJoinQuery<PersonReporting, LogEntryReporting, Guid>(unitOfWork, query,
					i => i.PersonId != null && i.UnixTimeStampMs == unixTimeStampMs,
					x => x.AggregateRootId, i => i.PersonId);


				var result = entityRepository.FetchQuery<TResult<PersonReporting, LogEntryReporting>>(
					join,
					x => x.OrderByDescending(r => r.E.Name),
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}


		[Test]
	    [Ignore("Not Working: joins need to be done at the same level; will need to use type instead of generics - MakeGeneric from type")]
		public void TResult_MultipleJoin()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var join1 = entityRepository.GetJoinQuery<PersonReporting, LogEntryReporting, Guid>(unitOfWork, query,
					i => i.PersonId != null && i.UnixTimeStampMs == 101,
					x => x.AggregateRootId, i => i.PersonId);
				var join2 = entityRepository.GetJoinQuery<TResult<PersonReporting, LogEntryReporting>, LocationReporting, Guid>(unitOfWork, join1,
					null, j1 => j1.I.LocationId, i => i.AggregateRootId);

				var result = entityRepository.FetchQuery<TResult<TResult<PersonReporting, LogEntryReporting>, LocationReporting>>(
					join2,
					null,
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		[Ignore("Not Working: joins need to be done at the same level; will need to use type instead of generics - MakeGeneric from type")]
		public void TResult_MultipleJoin_With_order_by()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var join1 = entityRepository.GetJoinQuery<PersonReporting, LogEntryReporting, Guid>(unitOfWork, query,
					i => i.PersonId != null &&  i.UnixTimeStampMs == 101,
					x => x.AggregateRootId, i => i.PersonId);
				var join2 = entityRepository.GetJoinQuery<TResult<PersonReporting, LogEntryReporting>, LocationReporting, Guid>(unitOfWork, join1,
					null, j1 => j1.I.LocationId, i => i.AggregateRootId);

				var result = entityRepository.FetchQuery<TResult<TResult<PersonReporting, LogEntryReporting>, LocationReporting>>(
					join2,
					x => x.OrderByDescending(r => r.E.E.Name),
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void MultipleJoin_WithJoinQuery()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var inner1 = entityRepository.GetQuery<LogEntryReporting>(unitOfWork, i => i.PersonId != null && i.UnixTimeStampMs == unixTimeStampMs);
				var inner2 = entityRepository.GetQuery<LocationReporting>(unitOfWork);

				var joinedLiveStreamAndLocation = entityRepository.JoinQueries(inner1, inner2,
					ls => ls.LocationId, loc => loc.AggregateRootId);

				var joined = entityRepository.JoinQueries(query, joinedLiveStreamAndLocation,
					person => person.AggregateRootId, joinedLiveStream => joinedLiveStream.PersonId);

				Console.WriteLine(EfCoreHelper.ToSql(joined));

				var result = entityRepository.FetchQuery<PersonReporting>(
					joined,
					x => x.OrderByDescending(r => r.Name),
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}

		[Test]
		public void JoinLogsWithLocation()
		{
			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				//Act
				var query = entityRepository.GetQuery<PersonReporting>(unitOfWork);
				var inner1 = entityRepository.GetQuery<LogEntryReporting>(unitOfWork, i => i.PersonId != null  && i.UnixTimeStampMs == unixTimeStampMs);
				var inner2 = entityRepository.GetQuery<LocationReporting>(unitOfWork);

				var joinedLiveStreamAndLocation = entityRepository.JoinQueries(inner1, inner2,
					ls => ls.LocationId, loc => loc.AggregateRootId);

				Console.WriteLine(EfCoreHelper.ToSql(joinedLiveStreamAndLocation));

				var result = entityRepository.FetchQuery<LogEntryReporting>(
					joinedLiveStreamAndLocation, null,
					2, 0);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}
	}
}