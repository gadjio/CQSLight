namespace PGMS.CQSLight.Infra.Querying
{
    public interface IHandleQuery<in TQuery, out TResult>
    {
        TResult Handle(TQuery query);
    }
}