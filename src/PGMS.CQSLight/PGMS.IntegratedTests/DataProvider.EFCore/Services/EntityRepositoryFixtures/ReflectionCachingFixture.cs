using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;
using PGMS.FakeImpl.DataProvider.Context;
using TestContext = PGMS.FakeImpl.DataProvider.Context.TestContext;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures;

[TestFixture]
public class ReflectionCachingFixture
{
	private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

	private IEntityRepository entityRepository;

	[SetUp]
	public void SetUp()
	{
		entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
		using (var unitOfWork = entityRepository.GetUnitOfWork())
		{
			((TestContext)unitOfWork.GetDbContext()).Database.EnsureCreated();
		}
	}

	[Test]
	public void LazyLoadAttribute_CachedAcrossMultipleCalls()
	{
		var aggregateRootId = Guid.NewGuid();
		var project = new ProjectReporting { AggregateRootId = aggregateRootId };
		project.Participants.Add(new ProjectParticipant { ClientId = 301 });
		project.Participants.Add(new ProjectParticipant { ClientId = 302 });
		project.Suppliers.Add(new ProjectSupplier { SupplierId = 401 });

		entityRepository.Insert(project);

		// Call FindFirst multiple times — each call hits IsLazyLoading / GetLazyLoadingProperties cache
		for (int i = 0; i < 5; i++)
		{
			var reloaded = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded, Is.Not.Null, $"Iteration {i}: reloaded should not be null");
			Assert.That(reloaded.Participants.Count, Is.EqualTo(2), $"Iteration {i}: LazyLoad participants should be loaded");
			Assert.That(reloaded.Suppliers.Count, Is.EqualTo(0), $"Iteration {i}: Non-lazy suppliers should NOT be loaded");
		}
	}

	[Test]
	public void IsLazyLoadingAttribute_CachedAcrossMultipleCalls()
	{
		var aggregateRootId = Guid.NewGuid();
		var project = new ProjectReportingFullLazy { AggregateRootId = aggregateRootId };
		project.Participants.Add(new ProjectParticipantFullLazy { ClientId = 501 });
		project.Suppliers.Add(new ProjectSupplierFullLazy { SupplierId = 601 });
		project.Suppliers.Add(new ProjectSupplierFullLazy { SupplierId = 602 });

		entityRepository.Insert(project);

		for (int i = 0; i < 5; i++)
		{
			var reloaded = entityRepository.FindFirst<ProjectReportingFullLazy>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded, Is.Not.Null, $"Iteration {i}: reloaded should not be null");
			Assert.That(reloaded.Participants.Count, Is.EqualTo(1), $"Iteration {i}: Full lazy participants should be loaded");
			Assert.That(reloaded.Suppliers.Count, Is.EqualTo(2), $"Iteration {i}: Full lazy suppliers should be loaded");
		}
	}

	[Test]
	public void CustomLazyLoadingAttribute_CachedAcrossMultipleCalls()
	{
		var aggregateRootId = Guid.NewGuid();
		var project = new ProjectReporting_Custom { AggregateRootId = aggregateRootId };
		project.Participants.Add(new ProjectParticipant_Custom { ClientId = 701 });
		project.Participants.Add(new ProjectParticipant_Custom { ClientId = 702 });

		entityRepository.Insert(project);

		for (int i = 0; i < 5; i++)
		{
			var reloaded = entityRepository.FindFirst<ProjectReporting_Custom>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded, Is.Not.Null, $"Iteration {i}: reloaded should not be null");
			Assert.That(reloaded.Participants.Count, Is.EqualTo(2), $"Iteration {i}: Custom lazy participants should be loaded");
		}
	}

	[Test]
	public async Task ConcurrentFindFirstAsync_DoesNotThrow()
	{
		// Insert test data
		var aggregateRootId = Guid.NewGuid();
		var project = new ProjectReporting { AggregateRootId = aggregateRootId };
		project.Participants.Add(new ProjectParticipant { ClientId = 801 });

		entityRepository.Insert(project);

		// Simulate concurrent access — the scenario that caused BadImageFormatException
		var exceptions = new ConcurrentBag<Exception>();
		var tasks = new List<Task>();
		const int concurrentCalls = 20;

		for (int i = 0; i < concurrentCalls; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					var repo = new BaseEntityRepository<TestContext>(
						new ConnectionStringProvider(connectionString),
						new IntegratedTestContextFactory());
					var result = await repo.FindFirstAsync<ProjectReporting>(x => x.AggregateRootId == aggregateRootId);
					Assert.That(result, Is.Not.Null);
					Assert.That(result.Participants.Count, Is.EqualTo(1));
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}));
		}

		await Task.WhenAll(tasks);

		Assert.That(exceptions, Is.Empty,
			$"Concurrent FindFirstAsync threw {exceptions.Count} exception(s): {string.Join("; ", exceptions.Select(e => e.Message))}");
	}

	[Test]
	public async Task ConcurrentFindFirstAsync_MixedEntityTypes_DoesNotThrow()
	{
		var id1 = Guid.NewGuid();
		var id2 = Guid.NewGuid();
		var id3 = Guid.NewGuid();

		entityRepository.Insert(new ProjectReporting { AggregateRootId = id1 });
		entityRepository.Insert(new ProjectReportingFullLazy { AggregateRootId = id2 });
		entityRepository.Insert(new ProjectReporting_Custom { AggregateRootId = id3 });

		var exceptions = new ConcurrentBag<Exception>();
		var tasks = new List<Task>();

		for (int i = 0; i < 10; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					var repo = new BaseEntityRepository<TestContext>(
						new ConnectionStringProvider(connectionString),
						new IntegratedTestContextFactory());
					await repo.FindFirstAsync<ProjectReporting>(x => x.AggregateRootId == id1);
					await repo.FindFirstAsync<ProjectReportingFullLazy>(x => x.AggregateRootId == id2);
					await repo.FindFirstAsync<ProjectReporting_Custom>(x => x.AggregateRootId == id3);
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}));
		}

		await Task.WhenAll(tasks);

		Assert.That(exceptions, Is.Empty,
			$"Concurrent mixed-type FindFirstAsync threw {exceptions.Count} exception(s): {string.Join("; ", exceptions.Select(e => e.Message))}");
	}
}

