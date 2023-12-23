using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PGMS.Data.Services;

namespace PGMS.DataProvider.EFCore.Services
{
    public class UnitOfWork<T> : IUnitOfWork where T : DbContext, IDbContext
    {
        /// <summary>
        /// Once set to keep alive you will need to remove the keep alive to be able to dispose
        /// </summary>
		public bool KeepAlive { get; set; }

		private readonly bool autoFlush;
        private T context;


        public UnitOfWork(string connectionString, T context, bool autoFlush)
        {
            this.context = context;
            this.autoFlush = autoFlush;
            
	      //  context = factory.Create(connectionString);
        }

        public void Initialise()
        {

        }

        public void ExecuteInTransaction(Action<IUnitOfWork> action)
        {
            using (var transaction = this.GetTransaction())
            {
                try
                {
                    action.Invoke(this);
                    context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                
            }
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
	        if (!KeepAlive)
	        {
		        context.Dispose();
	        }
        }

        public async ValueTask DisposeAsync()
        {
	        if (!KeepAlive)
	        {
		        await context.DisposeAsync();
	        }
        }

        public IUnitOfWorkTransaction GetTransaction()
        {
            return new DbTransaction(context.Database.BeginTransaction());
        }

        public async Task<IUnitOfWorkTransaction> GetTransactionAsync()
        {
	        return new DbTransaction(await context.Database.BeginTransactionAsync());
        }

        public IDbContext GetDbContext()
        {
            return context;
        }

        public bool IsAutoFlush()
        {
	        return autoFlush;
        }

        public void Save()
        {
	        context.SaveChanges();
        }

        public async Task SaveAsync()
        {
	        await context.SaveChangesAsync();
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

        public async Task CommitAsync()
        {
	        await transaction.CommitAsync();
        }


        public void Rollback()
        {
            transaction.Rollback();
        }

        public async Task RollbackAsync()
        {
	        await transaction.RollbackAsync();
        }

        public void Dispose()
        {
            transaction.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
	        await transaction.DisposeAsync();
        }
    }
}