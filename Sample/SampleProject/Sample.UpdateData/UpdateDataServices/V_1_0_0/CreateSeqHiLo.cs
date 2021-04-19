using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;

namespace Sample.UpdateData.UpdateDataServices.V_1_0_0
{
	public class CreateSeqHiLo : AbstractPlayOnceUpdateDataService
	{
		public CreateSeqHiLo(IEntityRepository entityRepository, ILogger<IUpdateDataService> logger) : base(entityRepository, logger)
		{
		}

		public override Task RunUpdate(IUnitOfWork unitOfWork)
		{
			var exist = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == "Default");
			if (exist == null)
			{
				entityRepository.InsertOperation(unitOfWork, new DbSequenceHiLo { id_parametres = "Default", intval = 10 });
			}
			return Task.CompletedTask;
		}
	}
}