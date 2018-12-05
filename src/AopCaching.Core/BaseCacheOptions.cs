using System;
using AspectCore.DynamicProxy;

namespace AopCaching.Core
{
	[NonAspect]
	public class BaseCacheOptions
	{
		/// <summary>
		/// Generate a 32-character MD5 string key that takes up little space but is poorly readable.
		/// </summary>
		public bool ShortKey { get; set; }

		/// <summary>
		/// Choose or exclude the cache method
		/// </summary>
		public CacheMethodFilter CacheMethodFilter { get; set; }

		/// <summary>
		/// Key prefix
		/// </summary>
		public string CacheKeyPrefix { get; set; } = "AspectCache";

		/// <summary>
		/// Cache expiration time
		/// </summary>
		public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(2);

		/// <summary>
		/// Key generator
		/// </summary>
		public Type CacheKeyGenerator { get; set; } = typeof(DefaultCacheKeyGenerator);

		/// <summary>
		/// Prevent cache penetration policy
		/// </summary>
		public PreventPenetrationPolicy PreventPenetrationPolicy { get; set; }
	}

	public class PreventPenetrationPolicy
	{
		/// <summary>
		/// Create a key with the specified value when the method has no result set.
		/// </summary>
		public BasicPolicy BasicPolicy { get; set; }

		/// <summary>
		/// Bloom filter
		/// </summary>
		public BloomFilterPolicy BloomFilterPolicy { get; set; }

	}

	public class BasicPolicy
	{
		public bool Enable { get; set; } = true;

		/// <summary>
		/// The expiration time of the key when the method returns no value set.
		/// </summary>
		public TimeSpan NoneResultKeyExpiration { get; set; } = TimeSpan.FromMinutes(2);
	}

	public class BloomFilterPolicy
	{
		public bool Enable { get; set; } = true;

		/// <summary>
		/// Expected number of caches.
		/// </summary>
		public int ExpectedElements { get; set; } = 10000;

		/// <summary>
		/// Error rate
		/// </summary>
		public double ErrorRate { get; set; } = 0.001;
	}
}
