using SMI.Data.Services;
using SMI.DataProvider.EFCore.Contexts;

namespace SMI.DataProvider.EFCore.Services
{
    public static class UnitOfWorkFactory<T> where T : BaseDbContext
    {
        public static IUnitOfWork GetUnitOfWork(string connectionString, ContextFactory<T> factory)
        {
            return new UnitOfWork<T>(connectionString, factory);
        }
    }
}