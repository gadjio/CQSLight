using System;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.EntityRepositoryFixtures
{
	[TestFixture]
	public class LazyLoadingFixture
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

		[Test]
		public void LazyLoadAttributeOnly()
		{
			var aggregateRootId = Guid.NewGuid();
			var project = new ProjectReporting { AggregateRootId = aggregateRootId };
			project.Participants.Add(new ProjectParticipant { ClientId = 101 });
			project.Participants.Add(new ProjectParticipant { ClientId = 102 });
			project.Suppliers.Add(new ProjectSupplier { SupplierId = 201 });
			project.Suppliers.Add(new ProjectSupplier { SupplierId = 202 });

			entityRepository.Insert(project);

			var reloaded = entityRepository.FindFirst<ProjectReporting>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded.Participants.Count, Is.EqualTo(2));
			Assert.That(reloaded.Suppliers.Count, Is.EqualTo(0));

		}


		[Test]
		public void FullLazyLoadingAttributeOnly()
		{
			var aggregateRootId = Guid.NewGuid();
			var project = new ProjectReportingFullLazy { AggregateRootId = aggregateRootId };
			project.Participants.Add(new ProjectParticipantFullLazy { ClientId = 101 });
			project.Participants.Add(new ProjectParticipantFullLazy { ClientId = 102 });
			project.Suppliers.Add(new ProjectSupplierFullLazy { SupplierId = 201 });
			project.Suppliers.Add(new ProjectSupplierFullLazy { SupplierId = 202 });

			entityRepository.Insert(project);

			var reloaded = entityRepository.FindFirst<ProjectReportingFullLazy>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded.Participants.Count, Is.EqualTo(2));
			Assert.That(reloaded.Suppliers.Count, Is.EqualTo(2));
		}

		[Test]
		public void LazyLoading_Custom_Attribute()
		{
			var aggregateRootId = Guid.NewGuid();
			var project = new ProjectReporting_Custom { AggregateRootId = aggregateRootId };
			project.Participants.Add(new ProjectParticipant_Custom { ClientId = 101 });
			project.Participants.Add(new ProjectParticipant_Custom { ClientId = 102 });

			entityRepository.Insert(project);

			var reloaded = entityRepository.FindFirst<ProjectReporting_Custom>(x => x.AggregateRootId == aggregateRootId);
			Assert.That(reloaded.Participants.Count, Is.EqualTo(2));
			
		}
	}
}