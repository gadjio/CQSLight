﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMI.Data.Services;
using SMI.DataProvider.EFCore.Contexts;

namespace SMI.DataProvider.EFCore.Services
{
    public class UnitOfWork<T> : IUnitOfWork where T : BaseDbContext
    {
        private T context;

        public UnitOfWork(string connectionString, ContextFactory<T> factory)
        {
            context = factory.Create(connectionString);
        }

        public void ExecuteInTransaction(Action<IUnitOfWork> action)
        {
            using (var transaction = this.GetTransaction())
            {
                try
                {
                    action.Invoke(this);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                this.Save();
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //if (disposing)
                //{
                //    context.Dispose();
                //}
            }
            this.disposed = true;
        }

        public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class
        {
            return context.Set<TEntity>();
        }

        public T GetContext()
        {
            return context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IUnitOfWorkTransaction GetTransaction()
        {
            return new DbTransaction(context.Database.BeginTransaction());
        }       

        public IDbContext GetDbContext()
        {
            return context;
        }
    }

    public class DbTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction transaction;

        public DbTransaction(IDbContextTransaction transaction)
        {
            this.transaction = transaction;
        }


        public void Commit()
        {
            transaction.Commit();
        }


        public void Rollback()
        {
            transaction.Rollback();
        }

        public void Dispose()
        {
            transaction.Dispose();
        }
    }
}