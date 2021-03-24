using System;

namespace PGMS.Data.Services
{
    public interface IUnitOfWork : IDisposable
    {
	    IUnitOfWorkTransaction GetTransaction();
        IDbContext GetDbContext();

        bool IsAutoFlush();
        void Save();
    }

    public interface IUnitOfWorkTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }

    public interface IDbContext
    {     
        
    }
}