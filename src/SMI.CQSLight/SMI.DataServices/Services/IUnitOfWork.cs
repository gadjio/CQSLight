using System;

namespace SMI.Data.Services
{
    public interface IUnitOfWork : IDisposable
    {
        void Save();
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