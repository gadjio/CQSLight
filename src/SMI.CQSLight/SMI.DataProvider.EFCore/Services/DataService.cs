using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMI.Data.Services;
using SMI.DataProvider.EFCore.Contexts;

namespace SMI.DataProvider.EFCore.Services
{  
    public class DataService<TDbContext> : IDataService where TDbContext : BaseDbContext
    {
        private static readonly object ConcurrencyLock = new object();
        private readonly int maxLo = 1000;        
        private long currentHi = -1;
        private int currentLo;

        private string ParameterName = "Workplan_webapp";

        private readonly BaseEntityRepository<TDbContext> entityRepository;

        public DataService(BaseEntityRepository<TDbContext> entityRepository)
        {
            this.entityRepository = entityRepository;
        }

        

        public long GenerateId()
        {
            long result;
            lock (ConcurrencyLock)
            {
                if (currentHi == -1)
                {
                    MoveNextHi();
                }
                if (currentLo == maxLo)
                {
                    currentLo = 0;
                    MoveNextHi();
                }
                result = (currentHi * maxLo) + currentLo;
                currentLo++;
            }
            return result;
        }



        private void MoveNextHi()
        {
            var updateQuery = $"UPDATE Workplan.SequenceHiLo SET intval=intval+1 WHERE id_parametres='{ParameterName}';";
            var getValueQuery = $"SELECT id, intval, id_parametres FROM Workplan.SequenceHiLo where id_parametres='{ParameterName}';";

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using (var transaction = unitOfWork.GetTransaction())
                {
                    entityRepository.ExecuteSqlCommand(unitOfWork, updateQuery);

                    var nextId = ((TDbContext)unitOfWork.GetDbContext()).sequenceHiLo.FromSqlRaw(getValueQuery).AsNoTracking().ToList();

                    transaction.Commit();

                    currentHi = nextId.First().intval;
                }
            }
        }

    }
   
    
  
    
}