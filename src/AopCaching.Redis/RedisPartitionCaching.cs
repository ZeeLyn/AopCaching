using System;
using AopCaching.Core;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Caching.Distributed;
using IAopCaching = AopCaching.Core.IAopCaching;

namespace AopCaching.Redis
{
	[NonAspect]
	public class RedisPartitionCaching : IAopCaching
	{
		[FromContainer]
		public IDistributedCache Cache { get; set; }

		public void Set(string key, object value, Type type, TimeSpan expire)
		{
			Cache.Set(key, DataSerializer.Serialize(value), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expire
			});
		}

		public (dynamic Value, bool HasKey) Get(string key, Type type)
		{
			var bytes = Cache.Get(key);
			if ((bytes?.LongLength ?? 0) == 0)
				return (null, false);
			dynamic value = DataSerializer.Deserialize(bytes, type);
			if (value is null)
				return (null, false);
			return (value, true);
		}

		public void Remove(params string[] keys)
		{
			Cache.Remove(string.Join("|", keys));
		}
	}
}
