using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;

namespace PGMS.DataProvider.EFCore.Services
{
    public class BaseOperationEntityRepository<T> where T : DbContext, IDbContext
    {
        protected static DbSet<TEntity> GetDbSet<TEntity>(IUnitOfWork unitOfWork) where TEntity : class
        {
            return ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
        }

        public T GetContext(IUnitOfWork unitOfWork)
        {
            return ((UnitOfWork<T>)unitOfWork).GetContext();
        }

        public IQueryable<TEntity> GetQuery<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null)
           where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;


            if (HasLazyLoading(typeof(TEntity)) || IsLazyLoading(typeof(TEntity)))
            {
	            var context = ((UnitOfWork<T>)unitOfWork).GetContext();
	            foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
	            {
		            query = query.Include(property.Name);
	            }
            }
            else
            {
	            var lazyLoadingProperties = GetLazyLoadingProperties(typeof(TEntity));
	            var context = ((UnitOfWork<T>)unitOfWork).GetContext();
	            foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
	            {
		            if (lazyLoadingProperties.Contains(property.PropertyInfo))
		            {
			            query = query.Include(property.Name);
		            }
	            }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }



            return query;
        }


        public IQueryable<TEntity> JoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query,
            IQueryable<TInner> innerQuery, Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null)
            where TEntity : class
            where TInner : class
        {
            var queryWithJoin = query.Join(innerQuery, outerKeySelector, innerKeySelector, (e, i) => e);

            return queryWithJoin;
        }


        public IQueryable<TEntity> LeftJoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query, IQueryable<TInner> innerQuery,
            Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null) where TEntity : class where TInner : class
        {
            var queryWithJoin = query.GroupJoin(innerQuery, outerKeySelector, innerKeySelector, (e, i) => new { e, i }).SelectMany(temp => temp.i.DefaultIfEmpty(),
                (temp, p) =>
                    new
                    {
                        e = temp.e,
                        p = p
                    }).Where(x => x.p == null).Select(x => x.e).AsQueryable();

            return queryWithJoin;
        }

        public List<TEntity> FetchAll<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
        {
            if (orderBy != null)
            {
                return orderBy(query.Distinct()).ToList();
            }

            return query.ToList();
        }

        public async Task<List<TEntity>> FetchAllAsync<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
        {
            if (orderBy != null)
            {
                return await orderBy(query.Distinct()).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public List<TEntity> FetchQuery<TEntity>(IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
        {
            if (orderBy != null)
            {
                return orderBy(query.Distinct()).Skip(offset).Take(fetchSize).ToList();
            }

            return query.Distinct().Skip(offset).Take(fetchSize).ToList();
        }

        public async Task<List<TEntity>> FetchQueryAsync<TEntity>(IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
        {
            if (orderBy != null)
            {
                return await orderBy(query.Distinct()).Skip(offset).Take(fetchSize).ToListAsync();
            }

            return await query.Distinct().Skip(offset).Take(fetchSize).ToListAsync();
        }

        public IQueryable<TResult<TEntity, TInner>> GetJoinQuery<TEntity, TInner, TKey>(IUnitOfWork unitOfWork, IQueryable<TEntity> query,
            Expression<Func<TInner, bool>> innerFilter = null, Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null)
            where TEntity : class
            where TInner : class
        {
            var innerDbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TInner>();
            IQueryable<TInner> subQuery = innerDbSet;
            if (innerFilter != null)
            {
                subQuery = subQuery.Where(innerFilter);
            }

            var queryWithJoin = query.Join(subQuery, outerKeySelector, innerKeySelector, (e, i) => new TResult<TEntity, TInner>(e, i));

            return queryWithJoin;
        }


        public IList<TEntity> GetJoinOperation<TEntity, TInner, TKey>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TInner, bool>> innerFilter = null, Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
            where TEntity : class
            where TInner : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;


            if (HasLazyLoading(typeof(TEntity)) || IsLazyLoading(typeof(TEntity)))
            {
	            var context = ((UnitOfWork<T>)unitOfWork).GetContext();
	            foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
	            {
		            query = query.Include(property.Name);
	            }
            }
            else
            {
	            var lazyLoadingProperties = GetLazyLoadingProperties(typeof(TEntity));
	            var context = ((UnitOfWork<T>)unitOfWork).GetContext();
	            foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
	            {
		            if (lazyLoadingProperties.Contains(property.PropertyInfo))
		            {
			            query = query.Include(property.Name);
		            }
	            }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var innerDbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TInner>();
            IQueryable<TInner> subQuery = innerDbSet;
            if (innerFilter != null)
            {
                subQuery = subQuery.Where(innerFilter);
            }

            var queryWithJoin = query.Join(subQuery, outerKeySelector, innerKeySelector, (e, i) => e);



            if (orderBy != null)
            {
                return orderBy(queryWithJoin).Skip(offset).Take(fetchSize).ToList();
            }

            return queryWithJoin.Skip(offset).Take(fetchSize).ToList();
        }

        public List<TEntity> FindAllOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
        {
            var query = GetOperationQuery(unitOfWork, filter);

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            return query.ToList();
		}

        public async Task<List<TEntity>> FindAllAsyncOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
        {
            var query = GetOperationQuery(unitOfWork, filter);

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }

            return await query.ToListAsync();
		}


		public IList<TEntity> GetOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
            var query = GetOperationQuery(unitOfWork, filter);

            if (orderBy != null)
            {
                return orderBy(query).Skip(offset).Take(fetchSize).ToList();
            }
            

            return query.Skip(offset).Take(fetchSize).ToList();
        }


        public async Task<List<TEntity>> GetOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
            var query = GetOperationQuery(unitOfWork, filter);

            if (orderBy != null)
            {
                return await orderBy(query).Skip(offset).Take(fetchSize).ToListAsync();
            }


            return await query.Skip(offset).Take(fetchSize).ToListAsync();
        }

        public async Task<List<TEntity>> GetDistinctOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
	        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
	        var query = GetOperationQuery(unitOfWork, filter);

            query = query.Distinct().AsQueryable();

            if (orderBy != null)
	        {
		        return await orderBy(query).Skip(offset).Take(fetchSize).ToListAsync();
	        }


	        return await query.Skip(offset).Take(fetchSize).ToListAsync();
        }

        private IQueryable<TEntity> GetOperationQuery<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;


            if (HasLazyLoading(typeof(TEntity)) || IsLazyLoading(typeof(TEntity)))
            {
                var context = ((UnitOfWork<T>)unitOfWork).GetContext();
                foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
                {
                    query = query.Include(property.Name);
                }
            }
            else
            {
                var lazyLoadingProperties = GetLazyLoadingProperties(typeof(TEntity));
                var context = ((UnitOfWork<T>)unitOfWork).GetContext();
                foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
                {
                    if (lazyLoadingProperties.Contains(property.PropertyInfo))
                    {
                        query = query.Include(property.Name);
                    }
                }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        private List<PropertyInfo> GetLazyLoadingProperties(Type type)
        {
	        var result = new List<PropertyInfo>();

	        foreach (var property in type.GetProperties())
	        {
		        var hasIsLazyLoading = Attribute.IsDefined(property, typeof(LazyLoadAttribute));
		        if (hasIsLazyLoading)
		        {
			        result.Add(property);
                    continue;
		        }

		        if (Attribute.GetCustomAttributes(property).Any(x => x.GetType().Name.ToLower() == "islazyloadingattribute"))
		        {
			        result.Add(property);
			        continue;
                }
	        }

	        return result;
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

        private bool IsLazyLoading(Type type)
        {
	        return Attribute.IsDefined(type, typeof(LazyLoadAttribute)) ||
	               Attribute.GetCustomAttributes(type).Any(x => x.GetType().Name.ToLower() == "islazyloadingattribute");
        }

        public TEntity FindFirstOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var query = GetFindFirstOperationQuery(unitOfWork, filter);

            return query.FirstOrDefault();
        }

        public async Task<TEntity> FindFirstOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var query = GetFindFirstOperationQuery(unitOfWork, filter);

            return await query.FirstOrDefaultAsync();
        }

        private IQueryable<TEntity> GetFindFirstOperationQuery<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;

            if (HasLazyLoading(typeof(TEntity)) || IsLazyLoading(typeof(TEntity)))
            {
                var context = ((UnitOfWork<T>)unitOfWork).GetContext();
                foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
                {
                    query = query.Include(property.Name);
                }
            }
            else
            {
                var lazyLoadingProperties = GetLazyLoadingProperties(typeof(TEntity));
                var context = ((UnitOfWork<T>)unitOfWork).GetContext();
                foreach (var property in context.Model.FindEntityType(typeof(TEntity)).GetNavigations())
                {
                    if (lazyLoadingProperties.Contains(property.PropertyInfo))
                    {
                        query = query.Include(property.Name);
                    }
                }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
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

        public async Task<int> CountOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync();

        }

        public async Task<int> CountDistinctOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
	        var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
	        IQueryable<TEntity> query = dbSet;

	        if (filter != null)
	        {
		        query = query.Where(filter);
	        }

	        return await query.Distinct().CountAsync();

        }

        public Dictionary<TKey, int> CountOperation<TEntity, TKey>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
	        var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
	        IQueryable<TEntity> query = dbSet;

	        if (filter != null)
	        {
		        query = query.Where(filter);
	        }

	        return query.GroupBy(groupBy).Select(g => new { key = g.Key, count = g.Count() }).ToDictionary(k => k.key, i => i.count);
        }

        public async Task<Dictionary<TKey, int>> CountOperationAsync<TEntity, TKey>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
            var dbSet = ((UnitOfWork<T>)unitOfWork).GetDbSet<TEntity>();
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.GroupBy(groupBy).Select(g => new { key = g.Key, count = g.Count() }).ToDictionaryAsync(k => k.key, i => i.count);
        }

        public virtual void InsertOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class
        {
	        var context = GetContext(unitOfWork);
            context.Add(entity);
            if (unitOfWork.IsAutoFlush())
            {
	            context.SaveChanges();
            }
            
        }

        public virtual async Task InsertOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class
        {
            var context = GetContext(unitOfWork);
            context.Add(entity);
            if (unitOfWork.IsAutoFlush())
            {
                await context.SaveChangesAsync();
            }

        }

        public virtual void DeleteOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class
        {
	        var context = GetContext(unitOfWork);
	        context.Remove(entityToDelete);

	        if (unitOfWork.IsAutoFlush())
	        {
		        context.SaveChanges();
	        }
        }

        public virtual async Task DeleteOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class
        {
            var context = GetContext(unitOfWork);
            context.Remove(entityToDelete);

            if (unitOfWork.IsAutoFlush())
            {
                await context.SaveChangesAsync();
            }
        }


        public virtual void DeleteManyOperation<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class
        {
	        var context = GetContext(unitOfWork);

	        foreach (var entity in entitiesToDelete)
	        {
		        context.Remove(entity);
	        }

	        if (unitOfWork.IsAutoFlush())
	        {
		        context.SaveChanges();
	        }
        }

        public virtual async Task DeleteManyOperationAsync<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class
        {
            var context = GetContext(unitOfWork);

            foreach (var entity in entitiesToDelete)
            {
                context.Remove(entity);
            }

            if (unitOfWork.IsAutoFlush())
            {
                await context.SaveChangesAsync();
            }
        }


        public virtual void UpdateOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class
        {
	        var context = GetContext(unitOfWork);

            context.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;

            var props = entityToUpdate.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(IsComplexTypeAttribute)));
            foreach (var prop in props)
            {
	            var nestedComplexObject = context.Entry(entityToUpdate).Reference(prop.Name).TargetEntry;
	            nestedComplexObject.State = EntityState.Modified;
            }

            if (unitOfWork.IsAutoFlush())
            {
	            context.SaveChanges();
            }
        }

        public virtual async Task UpdateOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class
        {
            var context = GetContext(unitOfWork);

            context.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;

            var props = entityToUpdate.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(IsComplexTypeAttribute)));
            foreach (var prop in props)
            {
                var nestedComplexObject = context.Entry(entityToUpdate).Reference(prop.Name).TargetEntry;
                nestedComplexObject.State = EntityState.Modified;
            }

            if (unitOfWork.IsAutoFlush())
            {
                await context.SaveChangesAsync();
            }
        }

        public void ExecuteSqlCommand(IUnitOfWork unitOfWork, string query)
        {
	        RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(GetContext(unitOfWork).Database, query);
        }

        public async Task ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query)
        {
            await RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(GetContext(unitOfWork).Database, query);
        }

        public int ExecuteSqlCommand(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters)
        {
	        var context = GetContext(unitOfWork);
	        return context.Database.ExecuteSqlRaw(query, parameters);
        }

        public async Task<int> ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters)
        {
            var context = GetContext(unitOfWork);
            return await context.Database.ExecuteSqlRawAsync(query, parameters);
        }

        public List<TQueryResult> RawSqlQuery<TQueryResult>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, TQueryResult> map)
        {
	        var context = GetContext(unitOfWork);
            using (var command = context.Database.GetDbConnection().CreateCommand())
		    {
			    command.CommandText = query;
			    command.CommandType = CommandType.Text;

			    context.Database.OpenConnection();

			    using (var result = command.ExecuteReader())
			    {
				    var entities = new List<TQueryResult>();

				    while (result.Read())
				    {
					    entities.Add(map(result));
				    }

				    return entities;
			    }
		    }
	        
        }

        public async Task<List<TQueryResult>> RawSqlQueryAsync<TQueryResult>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, TQueryResult> map)
        {
            var context = GetContext(unitOfWork);
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                await context.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    var entities = new List<TQueryResult>();

                    while (await result.ReadAsync())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }

        }
    }


   

    public class BaseEntityRepository<T> : BaseOperationEntityRepository<T>, IEntityRepository where T : DbContext, IDbContext
    {
        protected string ConnectionsString { get; set; }
        private readonly ContextFactory<T> factory;


        public BaseEntityRepository(IConnectionStringProvider connectionStringProvider, ContextFactory<T> factory)
        {
	        ConnectionsString = connectionStringProvider.GetConnectionString();
            this.factory = factory;            
        }

        public IUnitOfWork GetUnitOfWork(bool autoFlush = true)
        {
            return UnitOfWorkFactory<T>.GetUnitOfWork(ConnectionsString, factory, autoFlush);
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

        public virtual async Task<List<TEntity>> GetListWithRawSqlAsync<TEntity>(string query, params object[] parameters) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var dbSet = GetDbSet<TEntity>(unitOfWork);
                return await dbSet.FromSqlRaw(query, parameters).ToListAsync();
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

        public virtual async Task<List<TOutput>> GetNonEntityWithRawSqlAsync<TEntity, TOutput>(string query, Expression<Func<TEntity, TOutput>> transformer = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var dbSet = GetDbSet<TEntity>(unitOfWork);
                return await dbSet.FromSqlRaw(query, Array.Empty<Object>()).Select(transformer).ToListAsync();
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

        public async Task<List<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return await GetOperationAsync(unitOfWork, filter, orderBy, fetchSize, offset);
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

        public async Task<List<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                int fetchSize = 2000;

                var result = new List<TEntity>();
                IList<TEntity> subList;
                int offset = 0;

                do
                {
                    subList = await GetOperationAsync(unitOfWork, filter, orderBy, fetchSize, offset);
                    offset = offset + fetchSize;
                    result.AddRange(subList);
                } while (subList.Any());

                return result;
            }
        }



        public List<TEntity> FindTop<TEntity>( Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int fetchSize = 200,
            int offset = 0) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return GetOperation(unitOfWork, filter, orderBy, fetchSize, offset).ToList();
            }
        }

        public async Task<List<TEntity>> FindTopAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int fetchSize = 200,
            int offset = 0) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return await GetOperationAsync(unitOfWork, filter, orderBy, fetchSize, offset);
            }
        }

        public async Task<List<TEntity>> FindTopDistinctAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
	        int fetchSize = 200,
	        int offset = 0) where TEntity : class
        {
	        using (var unitOfWork = GetUnitOfWork())
	        {
		        return await GetDistinctOperationAsync(unitOfWork, filter, orderBy, fetchSize, offset);
	        }
        }

        public TEntity FindFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return FindFirstOperation(unitOfWork, filter);
            }
        }
        public async Task<TEntity> FindFirstAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return await FindFirstOperationAsync(unitOfWork, filter);
            }
        }


        public int Count<TEntity>( Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return CountOperation(unitOfWork, filter);
            }
        }
        public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return await CountOperationAsync(unitOfWork, filter);
            }
        }
        public async Task<int> CountDistinctAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
	        using (var unitOfWork = GetUnitOfWork())
	        {
		        return await CountDistinctOperationAsync(unitOfWork, filter);
	        }
        }

        public Dictionary<TKey, int> Count<TEntity, TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
	        using (var unitOfWork = GetUnitOfWork())
	        {
		        return CountOperation(unitOfWork, filter, groupBy);
	        }
        }

        public async Task<Dictionary<TKey, int>> CountAsync<TEntity, TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                return await CountOperationAsync(unitOfWork, filter, groupBy);
            }
        }

        public virtual void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                InsertOperation(unitOfWork, entity);
            }
        }

        public virtual async Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                await InsertOperationAsync(unitOfWork, entity);
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

        public virtual async Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                var entities = await GetOperationAsync(unitOfWork, filter);
                foreach (var entity in entities)
                {
                    await DeleteOperationAsync(unitOfWork, entity);
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

        public virtual async Task DeleteAsync<TEntity>(TEntity entityToDelete) where TEntity : class
        {
            if (entityToDelete == null)
            {
                return;
            }

            using (var unitOfWork = GetUnitOfWork())
            {
                await DeleteOperationAsync(unitOfWork, entityToDelete);
            }
        }

        public virtual void Update<TEntity>(TEntity entityToUpdate) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                UpdateOperation(unitOfWork, entityToUpdate);
            }
        }

        public virtual async Task UpdateAsync<TEntity>(TEntity entityToUpdate) where TEntity : class
        {
            using (var unitOfWork = GetUnitOfWork())
            {
                await UpdateOperationAsync(unitOfWork, entityToUpdate);
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
                        unitOfWork.Save();

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

        public async Task ExecuteInTransactionAsync(Action<IUnitOfWork> action)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            {
                using (var transaction = await GetContext(unitOfWork).Database.BeginTransactionAsync())
                {
                    try
                    {
                        action.Invoke(unitOfWork);
                        unitOfWork.Save();

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }

                }

            }
        }

    }
}