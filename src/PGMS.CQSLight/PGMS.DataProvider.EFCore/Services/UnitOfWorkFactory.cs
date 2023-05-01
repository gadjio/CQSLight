using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;

namespace PGMS.DataProvider.EFCore.Services
{
    public static class UnitOfWorkFactory<T> where T : DbContext, IDbContext
    {
        public static async Task<IUnitOfWork> GetUnitOfWork(string connectionString, ContextFactory<T> factory, bool autoFlush)
        {
            var context = await factory.Create(connectionString);
            return new UnitOfWork<T>(connectionString, context, autoFlush);
        }
    }
}