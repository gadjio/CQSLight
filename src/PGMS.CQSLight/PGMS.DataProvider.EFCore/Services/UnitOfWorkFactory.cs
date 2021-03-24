using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;

namespace PGMS.DataProvider.EFCore.Services
{
    public static class UnitOfWorkFactory<T> where T : BaseDbContext
    {
        public static IUnitOfWork GetUnitOfWork(string connectionString, ContextFactory<T> factory, bool autoFlush)
        {
            return new UnitOfWork<T>(connectionString, factory, autoFlush);
        }
    }
}