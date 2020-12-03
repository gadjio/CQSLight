using System;

namespace PGMS.Data.Services
{
    public interface IUnitOfWork : IDisposable
    {
	    IUnitOfWorkTransaction GetTransaction();
        IDbContext GetDbContext();
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