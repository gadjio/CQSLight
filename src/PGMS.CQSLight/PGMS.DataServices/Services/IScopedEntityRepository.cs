using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PGMS.Data.Services
{
	public interface IScopedEntityRepository
	{
        TEntity FindFirstOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        Task<TEntity> FindFirstOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

		IList<TEntity> GetOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
			where TEntity : class;
        Task<List<TEntity>> GetOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
            where TEntity : class;

        Task<List<TEntity>> GetDistinctOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
	        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
	        where TEntity : class;

		IList<TEntity> GetJoinOperation<TEntity, TInner, TKey>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
			Expression<Func<TInner, bool>> innerFilter = null, Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
			where TEntity : class
			where TInner : class;
        

		void InsertOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class;
        Task InsertOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class;

		void DeleteOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class;
        Task DeleteOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class;

		void DeleteManyOperation<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class;
        Task DeleteManyOperationAsync<TEntity>(IUnitOfWork unitOfWork, List<TEntity> entitiesToDelete) where TEntity : class;

		void UpdateOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class;
        Task UpdateOperationAsync<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class;

		int CountOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        Task<int> CountOperationAsync<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

		void ExecuteSqlCommand(IUnitOfWork unitOfWork, string query);
		Task ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query);

		int ExecuteSqlCommand(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters);
        Task<int> ExecuteSqlCommandAsync(IUnitOfWork unitOfWork, string query, SqlParameter[] parameters);

		List<T> RawSqlQuery<T>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, T> map);
        Task<List<T>> RawSqlQueryAsync<T>(IUnitOfWork unitOfWork, string query, Func<DbDataReader, T> map);

		//QueryBuilder
		IQueryable<TEntity> GetQuery<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null)
			where TEntity : class;

		IQueryable<TResult<TEntity, TInner>> GetJoinQuery<TEntity, TInner, TKey>(IUnitOfWork unitOfWork, IQueryable<TEntity> query,
			Expression<Func<TInner, bool>> innerFilter = null, Expression<Func<TEntity, TKey>> outerKeySelector = null,
			Expression<Func<TInner, TKey>> innerKeySelector = null)
			where TEntity : class
			where TInner : class;

		IQueryable<TEntity> JoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query,
			IQueryable<TInner> innerQuery, Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null)
			where TEntity : class
			where TInner : class;

		IQueryable<TEntity> LeftJoinQueries<TEntity, TInner, TKey>(IQueryable<TEntity> query, IQueryable<TInner> innerQuery,
			Expression<Func<TEntity, TKey>> outerKeySelector = null, Expression<Func<TInner, TKey>> innerKeySelector = null)
			where TEntity : class
			where TInner : class;

		List<TEntity> FetchQuery<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0);
        Task<List<TEntity>> FetchQueryAsync<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0);

		List<TEntity> FetchAll<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null);
        Task<List<TEntity>> FetchAllAsync<TEntity>(IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null);
	}


	public class TResult<TEntity, TInner>
	{
		public TEntity E { get; }
		public TInner I { get; }

		public TResult(TEntity e, TInner i)
		{
			E = e;
			I = i;
		}
	}
}