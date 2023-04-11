namespace PGMS.BlazorComponents.Extensions;

public static class DictionaryExtensions
{
	public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key, TValue value)
	{
		map[key] = value;
	}
}