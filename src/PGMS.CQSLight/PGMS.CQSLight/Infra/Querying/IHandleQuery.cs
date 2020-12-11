using System.Threading.Tasks;

namespace PGMS.CQSLight.Infra.Querying
{
    public interface IHandleQuery<in TQuery, out TResult>
    {
        TResult Handle(TQuery query);
    }

    public interface IHandleQueryAsync<in TQuery, TResult>
    {
	    Task<TResult> Handle(TQuery query);
    }

    public interface IHandleQueryAsyncEnumerable<in TQuery, TResult>
    {
	    TResult Handle(TQuery query);
    }
}