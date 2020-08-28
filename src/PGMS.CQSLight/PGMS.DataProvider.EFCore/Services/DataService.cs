using System.Linq;
using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;

namespace PGMS.DataProvider.EFCore.Services
{  

    public class DataService<TDbContext> : IDataService where TDbContext : BaseDbContext
    {
        private static readonly object ConcurrencyLock = new object();
        private readonly int maxLo = 1000;        
        private long currentHi = -1;
        private int currentLo;

        private string ParameterName;

        private readonly BaseEntityRepository<TDbContext> entityRepository;
        private readonly string schema;

        public DataService(BaseEntityRepository<TDbContext> entityRepository, string appName = "Default",string schema = null)
        {
	        this.entityRepository = entityRepository;
	        this.schema = schema;
	        ParameterName = appName;
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
	        var tablename = string.IsNullOrEmpty(schema) ? "SequenceHiLo" : $"[{schema}].SequenceHiLo";

            var updateQuery = $"UPDATE {tablename} SET intval=intval+1 WHERE id_parametres='{ParameterName}';";
            var getValueQuery = $"SELECT id, intval, id_parametres FROM {tablename} where id_parametres='{ParameterName}';";

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