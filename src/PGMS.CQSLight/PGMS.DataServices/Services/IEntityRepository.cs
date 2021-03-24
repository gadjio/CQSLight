using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PGMS.Data.Services
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork GetUnitOfWork(bool autoFlush = true);
	}

	public interface IEntityRepository :  IScopedEntityRepository, IUnitOfWorkProvider
    {
        IList<TEntity> GetListWithRawSql<TEntity>(string query, params object[] parameters) where TEntity : class;
        IList<TNonEntityOuputObject> GetNonEntityWithRawSql<TEntity, TNonEntityOuputObject>(string query, Expression<Func<TEntity, TNonEntityOuputObject>> transformer = null) where TEntity : class;

        IList<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null,
	        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
	        where TEntity : class;

        IList<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
            where TEntity : class;


        TEntity FindFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

        int Count<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;

        Dictionary<TKey, int> Count<TEntity, TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> groupBy) where TEntity : class;

        void Insert<TEntity>(TEntity entity) where TEntity : class;
        //void Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
        //void Delete<TEntity>(TEntity entityToDelete) where TEntity : class;
        //void Update<TEntity>(TEntity entityToUpdate) where TEntity : class;

        void ExecuteInTransaction(Action<IUnitOfWork> action);

        string GetConnectionString();
    }

}