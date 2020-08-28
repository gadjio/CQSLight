using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace SMI.Data.Services
{	
    public class LocalScopeCacheRepository<TValue> 
	{
		private ConcurrentDictionary<string, TValue> inMemoryMap = new ConcurrentDictionary<string, TValue>();
		private ConcurrentDictionary<string, DateTime> inMemoryValidity = new ConcurrentDictionary<string, DateTime>();

		public TValue Get(string key)
		{
			if (inMemoryMap.TryGetValue(key, out var value))
			{
				if (inMemoryValidity.TryGetValue(key, out var validUntil))
				{
					if (DateTime.Now <= validUntil)
						return value;
				}
			}

			return default(TValue);
		}

		public void Set(string key, TValue value, DateTime validUntil)
		{
			inMemoryMap.AddOrUpdate(key, value, (mapKey, oldValue) => value);
			inMemoryValidity.AddOrUpdate(key, validUntil, (mapKey, oldValue) => validUntil);
		}

		public void Clear()
		{
			inMemoryMap.Clear();
			inMemoryValidity.Clear();
		}
	}

	public class LocalScopeCacheRepository<TKey, TValue> : LocalScopeCacheRepository<TValue>
	{		
		public TValue Get(TKey key)
		{
			var strKey = JsonConvert.SerializeObject(key);
			return base.Get(strKey);			
		}

		public void Set(TKey key, TValue value, DateTime validUntil)
		{
			var strKey = JsonConvert.SerializeObject(key);
			base.Set(strKey, value, validUntil);			
		}		
	}
}