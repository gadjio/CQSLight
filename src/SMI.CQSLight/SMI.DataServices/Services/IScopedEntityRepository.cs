using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SMI.Data.Services
{
	public interface IScopedEntityRepository
	{

		TEntity FindFirstOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null)
			where TEntity : class;

		IList<TEntity> GetOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int fetchSize = 200, int offset = 0)
			where TEntity : class;

		void InsertOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entity) where TEntity : class;
		void DeleteOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToDelete) where TEntity : class;
		void UpdateOperation<TEntity>(IUnitOfWork unitOfWork, TEntity entityToUpdate) where TEntity : class;

		int CountOperation<TEntity>(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
	}
}