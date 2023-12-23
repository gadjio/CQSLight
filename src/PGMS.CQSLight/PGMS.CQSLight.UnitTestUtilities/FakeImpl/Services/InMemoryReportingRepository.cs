using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using PGMS.Data.Services;

namespace PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services
{
	public class InMemoryReportingRepository : InMemoryEntityRepository
	{

	}

	public class InMemoryEntityRepository : IEntityRepository
	{
		private IDictionary<Type, List<object>> inMemoryMap = new Dictionary<Type, List<object>>();

		public void SetCurrentUnitOfWork(IUnitOfWork unitOfWork)
		{
			throw new NotImplementedException();
		}

		public IList<TEntity> GetListWithRawSql<TEntity>(string query, params object[] parameters) where TEntity : class
		{
			throw new NotImplementedException("GetListWithRawSql is not implemented in InMemoryReportingRepository");
		}

		public Task<List<TEntity>> GetListWithRawSqlAsync<TEntity>(string query, params object[] parameters) where TEntity : class
		{
			throw new NotImplementedException("GetListWithRawSqlAsync is not implemented in InMemoryReportingRepository");
		}

		public IList<TNonEntityOuputObject> GetNonEntityWithRawSql<TEntity, TNonEntityOuputObject>(string query,
			Expression<Func<TEntity, TNonEntityOuputObject>> transformer = null) where TEntity : class
		{
			throw new NotImplementedException("GetNonEntityWithRawSql is not implemented in InMemoryReportingRepository");
		}

		public Task<List<TNonEntityOuputObject>> GetNonEntityWithRawSqlAsync<TEntity, TNonEntityOuputObject>(string query, Expression<Func<TEntity, TNonEntityOuputObject>> transformer = null) where TEntity : class
		{
			throw new NotImplementedException("GetNonEntityWithRawSqlAsync is not implemented in InMemoryReportingRepository");
		}

		public IList<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
			where TEntity : class
		{
			return GetOperation(null, filter).Skip(offset).Take(fetchSize).ToList();
		}

		public Task<List<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
		{
			var result = Get(filter, orderBy, fetchSize, offset).ToList();
			return Task.FromResult(result);
		}

		public IList<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
		{
			return GetOperation(null, filter);
		}

		public Task<List<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
		{
			var result = FindAllOperation(null, filter, orderBy);
			return Task.FromResult(result);
		}

		public TEntity FindFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			return GetOperation(null, filter).FirstOrDefault();
		}

		public Task<TEntity> FindFirstAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			var result = FindFirstOperation(null, filter);
			return Task.FromResult(result);
		}

		public int Count<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			return FindAll(filter).Count();
		}

		public Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			var result = Count(filter);
			return Task.FromResult(result);
		}

		public Task<int> CountDistinctAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			var result = FindAll(filter).Distinct().Count();
			return Task.FromResult(result);
		}

		public Dictionary<TKey, int> Count<TEntity, TKey>(Expression<Func<TEntity, bool>> filter,
			Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
		{
            var query = FindAll<TEntity>(filter).AsQueryable();
            query.GroupBy(groupBy);

			var grouped = query.GroupBy(groupBy).ToDictionary(x => x.Key, x => x.Count());
            return grouped;
        }

		public Task<Dictionary<TKey, int>> CountAsync<TEntity, TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class
        {
            var result = Count(filter, groupBy);
            return Task.FromResult(result);
		}

		public void Insert<TEntity>(TEntity entity) where TEntity : class
		{
			InsertOperation(null, entity);
		}

		public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
		{
            InsertOperation(null, entity);
			return Task.CompletedTask;
		}

		public void Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
		{
			DeleteOperation(null, filter);
        }

		public Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
		{
			DeleteOperation(null, filter);
            return Task.CompletedTask;
		}

		public void Delete<TEntity>(TEntity entityToDelete) where TEntity : class
		{
			DeleteOperation(null, entityToDelete);
		}

		public Task DeleteAsync<TEntity>(TEntity entityToDelete) where TEntity : class
		{
            DeleteOperation(null, entityToDelete);
            return Task.CompletedTask;
		}

		public void Update<TEntity>(TEntity entityToUpdate) where TEntity : class
		{
        }

		public Task UpdateAsync<TEntity>(TEntity entityToUpdate) where TEntity : class
		{
            return Task.CompletedTask;
		}


		public void ExecuteInTransaction(Action<IUnitOfWork> action)
		{
			action.Invoke(null);
		}

		public Task ExecuteInTransactionAsync(Action<IUnitOfWork> action)
		{
            action.Invoke(null);
            return Task.CompletedTask;
		}

		public TEntity FindFirstOperation<TEntity>(IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			return GetOperation(null, filter).FirstOrDefault();
		}

		public Task<TEntity> FindFirstOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
			var result = GetOperation(null, filter).FirstOrDefault();
			return Task.FromResult(result);
		}


		public IList<TEntity> GetOperation<TEntity>(IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200,
			int offset = 0) where TEntity : class
		{
			var key = typeof(TEntity);
			if (!inMemoryMap.ContainsKey(key))
			{
				return new List<TEntity>();
			}

			if (filter == null)
			{
				return inMemoryMap[key].Cast<TEntity>().ToList();
			}

			var query = filter.Compile();
			return inMemoryMap[key].Cast<TEntity>().Where(query).ToList();
		}

		public Task<List<TEntity>> GetOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			int fetchSize = 200, int offset = 0) where TEntity : class
        {
            var result = GetOperation(unitOfWork, filter, orderBy, fetchSize, offset).ToList();
            return Task.FromResult(result);
		}

		public List<TEntity> FindAllOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
		{
			return GetOperation(unitOfWork, filter, orderBy).ToList();
		}

		public Task<List<TEntity>> FindAllOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
		{
			var result = GetOperation(unitOfWork, filter, orderBy).ToList();
			return Task.FromResult(result);
		}

		public Task<List<TEntity>> GetDistinctOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			int fetchSize = 200, int offset = 0) where TEntity : class
		{
			var result = GetOperation(unitOfWork, filter, orderBy).Distinct().ToList();
			return Task.FromResult(result);
		}

		public IList<TEntity> GetJoinOperation<TEntity, TInner, TKey>(IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> filter = null,
			Expression<Func<TInner, bool>> innerFilter = null, Expression<Func<TEntity, TKey>> outerKeySelector = null,
			Expression<Func<TInner, TKey>> innerKeySelector = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
			where TEntity : class where TInner : class
        {
            var query = GetQuery(unitOfWork, filter);
            var innerQuery = GetQuery(unitOfWork, innerFilter);

			var resultQuery = JoinQueries(query, innerQuery,outerKeySelector, innerKeySelector);
            return FetchQuery(resultQuery, orderBy, fetchSize, offset);
        }

		public void InsertOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class
		{
			var key = typeof(TEntity);
			if (!inMemoryMap.ContainsKey(key))
			{
				inMemoryMap.Add(key, new List<object>());
			}

			inMemoryMap[key].Add(entity);
		}

		public Task InsertOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class
		{
			InsertOperation(unitOfWork, entity);
			return Task.CompletedTask;
		}

        public Task BulkInsertOperationAsync<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entities) where TEntity : class
        {
            var key = typeof(TEntity);
            if (!inMemoryMap.ContainsKey(key))
            {
                inMemoryMap.Add(key, new List<object>());
            }

            inMemoryMap[key].AddRange(entities);
            return Task.CompletedTask;
        }

        public void DeleteOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class
		{
			var key = typeof(TEntity);
			inMemoryMap[key].Remove(entityToDelete);
		}

		public Task DeleteOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class
		{
			DeleteOperation(unitOfWork, entityToDelete);
			return Task.CompletedTask;
		}

		public void DeleteManyOperation<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class
		{
			foreach (var entity in entitiesToDelete)
			{
				DeleteOperation(unitOfWork, entity);
			}
		}

		public Task DeleteManyOperationAsync<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class
		{
			DeleteManyOperation(unitOfWork, entitiesToDelete);
			return Task.CompletedTask;
		}

		public void UpdateOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class
		{
		}

		public Task UpdateOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class
		{
            return Task.CompletedTask;
		}


		public int CountOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null)
			where TEntity : class
		{
            return FindAllOperation(unitOfWork, filter).Count();
		}

		public Task<int> CountOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            var result = CountOperation(unitOfWork, filter);
            return Task.FromResult(result);
		}

		public string GetConnectionString()
		{
			return "InMemory";
		}

		public List<TEntity> FindTop<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
		{
            return GetOperation(GetUnitOfWork(), filter, orderBy, fetchSize, offset).ToList();
		}

		public Task<List<TEntity>> FindTopAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
		{
            return Task.FromResult(FindTop(filter, orderBy, fetchSize, offset));
		}

		public Task<List<TEntity>> FindTopDistinctAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0) where TEntity : class
        {
            var result = FindAll(filter, orderBy).Distinct().Skip(offset).Take(fetchSize).ToList();
            return Task.FromResult(result);
		}

		public void ExecuteSqlCommand(IUnitOfWork unitOfWork, string query)
		{
			throw new NotImplementedException("ExecuteSqlCommand is not implemented in InMemoryReportingRepository");
		}

		public Task ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query)
		{
			throw new NotImplementedException("ExecuteSqlCommandAsync is not implemented in InMemoryReportingRepository");
		}

		public int ExecuteSqlCommand(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters)
		{
            throw new NotImplementedException("ExecuteSqlCommand is not implemented in InMemoryReportingRepository");
		}

		public Task<int> ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters)
		{
            throw new NotImplementedException("ExecuteSqlCommandAsync is not implemented in InMemoryReportingRepository");
		}

		public List<T> RawSqlQuery<T>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, T> map, int timeoutInSec = 60)
		{
            throw new NotImplementedException("RawSqlQuery is not implemented in InMemoryReportingRepository");
		}

		public Task<List<T>> RawSqlQueryAsync<T>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, T> map, int timeoutInSec = 60)
		{
            throw new NotImplementedException("RawSqlQueryAsync is not implemented in InMemoryReportingRepository");
		}

		public IQueryable<TEntity> GetQuery<TEntity>(IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> filter = null) where TEntity : class
		{
            return FindAll<TEntity>(filter).AsQueryable();
		}

		public IQueryable<TResult<TEntity, TInner>> GetJoinQuery<TEntity, TInner, TKey>(IUnitOfWork unitOfWork,
			IQueryable<TEntity> query, Expression<Func<TInner, bool>> innerFilter = null,
			Expression<Func<TEntity, TKey>> outerKeySelector = null,
			Expression<Func<TInner, TKey>> innerKeySelector = null) where TEntity : class where TInner : class
        {
            var innerQuery = GetQuery(unitOfWork, innerFilter);
           
			var queryWithJoin = query.Join(innerQuery, outerKeySelector, innerKeySelector, (e, i) => new TResult<TEntity, TInner>(e, i));
            return queryWithJoin;
        }

		public IQueryable<TEntity> JoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query,
			IQueryable<TInner> innerQuery,
			Expression<Func<TEntity, TKey>> outerKeySelector = null,
			Expression<Func<TInner, TKey>> innerKeySelector = null) where TEntity : class where TInner : class
		{
            var queryWithJoin = query.Join(innerQuery, outerKeySelector, innerKeySelector, (e, i) => e);

            return queryWithJoin;
		}

		public IQueryable<TEntity> LeftJoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query,
			IQueryable<TInner> innerQuery,
			Expression<Func<TEntity, TKey>> outerKeySelector = null,
			Expression<Func<TInner, TKey>> innerKeySelector = null) where TEntity : class where TInner : class
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

		public List<TEntity> FetchQuery<TEntity>(IQueryable<TEntity> query,
			Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
		{
            if (orderBy != null)
            {
                return orderBy(query).Skip(offset).Take(fetchSize).ToList();
            }

            return query.Skip(offset).Take(fetchSize).ToList();
		}

		public Task<List<TEntity>> FetchQueryAsync<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
		{
            return Task.FromResult(FetchQuery(query, orderBy, fetchSize, offset));
		}

		public List<TEntity> FetchAll<TEntity>(IQueryable<TEntity> query,
			Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
		{
            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            return query.ToList();
		}

		public Task<List<TEntity>> FetchAllAsync<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
		{
            return Task.FromResult(FetchAll(query, orderBy));
		}

		public IUnitOfWork GetUnitOfWork(bool autoFlush = true)
		{
            return new FakeUnitOfWork();
		}
	}
}