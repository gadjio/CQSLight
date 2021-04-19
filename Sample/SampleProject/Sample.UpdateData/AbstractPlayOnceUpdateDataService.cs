using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PGMS.Data.Services;
using Sample.Data.Models.App;

namespace Sample.UpdateData
{
	public interface IUpdateDataService
	{
		Task Update();
	}

	public abstract class AbstractPlayOnceUpdateDataService : IUpdateDataService
	{
		protected readonly IEntityRepository entityRepository;
		protected readonly ILogger<IUpdateDataService> logger;


		public AbstractPlayOnceUpdateDataService(IEntityRepository entityRepository, ILogger<IUpdateDataService> logger)
		{
			this.entityRepository = entityRepository;
			this.logger = logger;
		}

		public virtual async Task Update()
		{
			if (!CanRun())
			{
				logger.LogInformation("Service has already been played; not running.");
				return;
			}

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				await RunUpdate(unitOfWork);
				entityRepository.InsertOperation(unitOfWork, new UpdateServiceLookup { LastRunTime = DateTime.Now, ServiceName = GetServiceName() });
			}
		}

		public virtual string GetServiceName()
		{
			return GetType().Name;
		}

		public virtual bool CanRun()
		{
			var existing = entityRepository.FindFirst<UpdateServiceLookup>(x => x.ServiceName == GetServiceName());
			return existing == null;
		}

		public abstract Task RunUpdate(IUnitOfWork unitOfWork);
	}
}
