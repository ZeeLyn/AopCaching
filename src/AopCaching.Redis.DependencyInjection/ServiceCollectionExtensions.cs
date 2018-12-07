using System;
using System.Linq;
using AopCaching.Core;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using BloomFilter;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace AopCaching.Redis.DependencyInjection
{
	[NonAspect]
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAopCacheInRedis(this IServiceCollection serviceCollection, Action<RedisCacheOptions> optionBuilder)
		{
			if (optionBuilder == null)
				throw new ArgumentNullException(nameof(optionBuilder));
			var configure = new RedisCacheOptions();
			optionBuilder.Invoke(configure);
			return serviceCollection.AddAopCacheInRedis(configure);
		}

		public static IServiceCollection AddAopCacheInRedis(this IServiceCollection serviceCollection, RedisCacheOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			serviceCollection.AddSingleton(typeof(BaseCacheOptions), options);

			if (options.UsePartition)
			{
				RedisHelper.Initialization(new CSRedis.CSRedisClient(options.NodeRule, options.Endpoints.ToArray()));
				serviceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
				serviceCollection.AddSingleton<IAopCaching, RedisPartitionCaching>();
			}
			else
			{
				RedisHelper.Initialization(new CSRedis.CSRedisClient(options.Endpoints.First()));
				serviceCollection.AddSingleton<IAopCaching, AopRedisCaching>();
			}
			if (options.PreventPenetrationPolicy?.BloomFilterPolicy != null)
			{
				if (options.PreventPenetrationPolicy.BloomFilterPolicy.Enable)
				{
					serviceCollection.AddSingleton<IBloomFilter>(FilterBuilder.Build<string>(options.PreventPenetrationPolicy.BloomFilterPolicy.ExpectedElements, options.PreventPenetrationPolicy.BloomFilterPolicy.ErrorRate));
				}
			}
			serviceCollection.AddSingleton(typeof(ICacheKeyGenerator), options.CacheKeyGenerator);
			serviceCollection.ConfigureDynamicProxy(configurator =>
			{
				RegisterDynamicProxy.Register(configurator, options.CacheMethodFilter);
			});
			return serviceCollection;
		}
	}
}
