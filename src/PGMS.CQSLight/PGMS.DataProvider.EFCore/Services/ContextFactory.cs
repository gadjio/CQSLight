using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;

namespace PGMS.DataProvider.EFCore.Services
{
    public abstract class ContextFactory<T> where T : DbContext, IDbContext
    {

        public virtual Task<T> Create(string connectionString)
        {
            return Task.FromResult(CreateSync(connectionString));
        }

        /// <summary>
        /// Synchronous context creation. Used by the synchronous GetUnitOfWork path
        /// to avoid sync-over-async deadlocks. Override this if you need custom context creation logic.
        /// </summary>
        public virtual T CreateSync(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseSqlServer(connectionString);

            return CreateContext(optionsBuilder.Options);
        }

        public abstract T CreateContext(DbContextOptions<T> options);
    }


    public interface IConnectionStringProvider
    {
	    string GetConnectionString();
    }

    public class ConnectionStringProvider : IConnectionStringProvider
    {
	    private readonly string connectionString;

	    public ConnectionStringProvider(string connectionString)
	    {
		    this.connectionString = connectionString;
	    }

	    public string GetConnectionString()
	    {
		    return connectionString;
	    }
    }
}