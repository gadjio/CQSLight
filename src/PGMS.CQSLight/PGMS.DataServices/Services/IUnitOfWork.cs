using System;
using System.Threading.Tasks;

namespace PGMS.Data.Services
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
	    IUnitOfWorkTransaction GetTransaction();
	    Task<IUnitOfWorkTransaction> GetTransactionAsync();
        IDbContext GetDbContext();

        bool IsAutoFlush();
        void Save();
        Task SaveAsync();

        bool KeepAlive { get; set; }
    }

    public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
    {
	    void Commit();
	    Task CommitAsync();

        void Rollback();
        Task RollbackAsync();
    }

    public interface IDbContext
    {     
        
    }
}