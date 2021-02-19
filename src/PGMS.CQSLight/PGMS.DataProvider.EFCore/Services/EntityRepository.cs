using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;

namespace PGMS.DataProvider.EFCore.Services
{
    public class BaseOperationEntityRepository<T> where T : BaseDbContext
    {
        protected static DbSet<TEntity> GetDbSet<TEntity>(IUnitOfWork unitOfWork) where TEntity : class
        {
            return ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
        }

        public T GetContext(IUnitOfWork unitOfWork)
        {
            return ((UnitOfWork<T>)unitOfWork).GetContext();
        }

        public IList<TEntity> GetOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
	        var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
	        IQueryable<TEntity> query = dbSet;


	        if (HasLazyLoading(typeof(TEntity)))
	        {
		        var context = ((UnitOfWork<T>) unitOfWork).GetContext();
		        foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
		        {
			        query = query.Include(property.Name);
		        }
	        }

	        if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return orderBy(query).Skip(offset).Take(fetchSize).ToList();
            }

            return query.Skip(offset).Take(fetchSize).ToList();
        }

        private bool HasLazyLoading(Type type)
        {
	        foreach (var property in type.GetProperties())
	        {
		        var hasIsLazyLoading = Attribute.IsDefined(property, typeof(IsLazyLoadingAttribute));
		        if (hasIsLazyLoading)
		        {
			        return true;
		        }
            }

	        return false;
        }

        public TEntity FindFirstOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;

            if (HasLazyLoading(typeof(TEntity)))
            {
	            var context = ((UnitOfWork<T>)unitOfWork).GetContext();
	            foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
	            {
		            query = query.Include(property.Name);
	            }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.FirstOrDefault();

        }

        public int CountOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.Count();

        }

        public Dictionary<TKey, int> CountOperation<TEntity, TKey>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
	        var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
	        IQueryable<TEntity> query = dbSet;

	        if (filter != null)
	        {
		        query = query.Where(filter);
	        }

	        return query.GroupBy(groupBy).Select(g => new { key = g.Key, count = g.Count() }).ToDictionary(k => k.key, i => i.count); ;
        }

        public virtual void InsertOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            dbSet.Add(entity);

            var context = GetContext(unitOfWork);
            context.SaveChanges();
        }

        public virtual void DeleteOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class
        {
            var dbSet = GetDbSet<TEntity>(unitOfWork);
            var context = GetContext(unitOfWork);

            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);

            context.SaveChanges();
        }



        public virtual void UpdateOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class
        {
            var dbSet = GetDbSet<TEntity>(unitOfWork);
            var context = GetContext(unitOfWork);

            //dbSet.Update(entityToUpdate);
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;

            var props = entityToUpdate.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(IsComplexTypeAttribute)));
            foreach (var prop in props)
            {
	            var nestedComplexObject = context.Entry(entityToUpdate).Reference(prop.Name).TargetEntry;
	            nestedComplexObject.State = EntityState.Modified;
            }

            context.SaveChanges();
        }
    }


    public class BaseEntityRepository<T> : BaseOperationEntityRepository<T>, IEntityRepository where T : BaseDbContext
    {
        protected string ConnectionsString { get; set; }
        private readonly ContextFactory<T> factory;


        public BaseEntityRepository(IConnectionStringProvider connectionStringProvider, ContextFactory<T> factory)
        {
	        ConnectionsString = connectionStringProvider.GetConnectionString();
            this.factory = factory;            
        }

        public IUnitOfWork GetUnitOfWork()
        {
            return UnitOfWorkFactory<T>.GetUnitOfWork(ConnectionsString, factory);
        }

        public string GetConnectionString()
        {
            return ConnectionsString;
        }

        public virtual IList<TEntity> GetListWithRawSql<TEntity>(string query, params object[] parameters) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var dbSet = GetDbSet<TEntity>(unitOfWork);
                return dbSet.FromSqlRaw(query, parameters).ToList();
            }
        }

        public virtual IList<TOutput> GetNonEntityWithRawSql<TEntity, TOutput>(string query, Expression<Func<TEntity, TOutput>> transformer = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var dbSet = GetDbSet<TEntity>(unitOfWork);
                return dbSet.FromSqlRaw(query, Array.Empty<Object>()).Select(transformer).ToList();
            }
        }

        public IList<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return GetOperation(unitOfWork, filter, orderBy, fetchSize, offset);
            }
        }

        public IList<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                int fetchSize = 2000;

                var result = new List<TEntity>();
                IList<TEntity> subList;
                int offset = 0;

                do
                {
                    subList = GetOperation(unitOfWork, filter, orderBy, fetchSize, offset); 
                    offset = offset + fetchSize;
                    result.AddRange(subList);
                } while (subList.Any());

                return result;
            }
        }

        public TEntity FindFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return FindFirstOperation(unitOfWork, filter);
            }
        }

        public int Count<TEntity>( Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return CountOperation(unitOfWork, filter);
            }
        }

        public Dictionary<TKey, int> Count<TEntity, TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
	        using (var unitOfWork = GetUnitOfWork())
	        {
		        return CountOperation(unitOfWork, filter, groupBy);
	        }
        }

        public virtual void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                InsertOperation(unitOfWork, entity);
            }
        }

        public virtual void Delete<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var entities = GetOperation(unitOfWork, filter);
                foreach (var entity in entities)
                {
                    DeleteOperation(unitOfWork, entity);
                }
            }
        }

        public virtual void Delete<TEntity>(TEntity entityToDelete) where TEntity : class
        {
            if (entityToDelete == null)
            {
                return;
            }

            using (var unitOfWork = GetUnitOfWork())
            {
                DeleteOperation(unitOfWork, entityToDelete);
            }
        }

        public virtual void Update<TEntity>(TEntity entityToUpdate) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                UpdateOperation(unitOfWork, entityToUpdate);
            }
        }

        public void ExecuteInTransaction(Action<IUnitOfWork> action)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            {
                using (var transaction = GetContext(unitOfWork).Database.BeginTransaction())
                {
                    try
                    {
                        action.Invoke(unitOfWork);
                        var context = GetContext(unitOfWork);
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
        }

        public void ExecuteSqlCommand(IUnitOfWork unitOfWork, string query)
        {            
             RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(GetContext(unitOfWork).Database, query);            
        }

    }
}