using System;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using IAopCaching = AopCaching.Core.IAopCaching;


namespace AopCaching.InMemory
{
	[NonAspect]
	public class MemoryCaching : IAopCaching
	{
		private IMemoryCache Cache { get; }

		public MemoryCaching(IMemoryCache cache)
		{
			Cache = cache;
		}
		public void Set(string key, object value, Type type, TimeSpan expire)
		{
			Cache.Set(key, new { Value = value }, expire);
		}

		public (dynamic Value, bool HasKey) Get(string key, Type type)
		{
			dynamic value = Cache.Get(key);
			if (value == null)
				return (null, false);
			return (value.Value, true);
		}

		public void Remove(params string[] keys)
		{
			foreach (var key in keys)
			{
				Cache.Remove(key);
			}
		}
	}
}
