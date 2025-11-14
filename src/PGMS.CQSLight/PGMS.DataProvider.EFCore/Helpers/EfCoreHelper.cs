using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PGMS.DataProvider.EFCore.Helpers
{
	public static class EfCoreHelper
	{
		public static string ToSql<TEntity>(IQueryable<TEntity> query) where TEntity : class
		{
			return query.ToQueryString();
		}
	}
}