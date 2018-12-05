using System;
using System.Linq;
using System.Threading.Tasks;
using AopCaching.Core;
using AspectCore.Configuration;
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
				configurator.ThrowAspectException = false;
				//Exclude methods that do not return a value
				configurator.NonAspectPredicates.Add(method => method.ReturnType == typeof(void) || method.ReturnType == typeof(Task));
				configurator.Interceptors.AddTyped<AopCachingInterceptor>(method =>
					method.GetCustomAttributes(true).Any(p => p.GetType() == typeof(AopCachingAttribute)));

				if (options.CacheMethodFilter?.IncludeService?.Any() ?? false)
					configurator.Interceptors.AddTyped<AopCachingInterceptor>(options.CacheMethodFilter
						.IncludeService.Select(Predicates.ForService).ToArray());
				if (options.CacheMethodFilter?.ExcludeService?.Any() ?? false)
					foreach (var item in options.CacheMethodFilter?.ExcludeService)
					{
						configurator.NonAspectPredicates.AddService(item);
					}

				if (options.CacheMethodFilter?.IncludeMethod?.Any() ?? false)
					configurator.Interceptors.AddTyped<AopCachingInterceptor>(options.CacheMethodFilter
						.IncludeMethod.Select(Predicates.ForMethod).ToArray());
				if (options.CacheMethodFilter?.ExcludeMethod?.Any() ?? false)
					foreach (var item in options.CacheMethodFilter?.ExcludeMethod)
					{
						configurator.NonAspectPredicates.AddMethod(item.ServiceName, item.MethodName);
					}

				if (options.CacheMethodFilter?.IncludeNameSpace?.Any() ?? false)
					configurator.Interceptors.AddTyped<AopCachingInterceptor>(options.CacheMethodFilter
						.IncludeNameSpace.Select(Predicates.ForNameSpace).ToArray());
				if (options.CacheMethodFilter?.ExcludeNameSpace?.Any() ?? false)
					foreach (var item in options.CacheMethodFilter?.ExcludeNameSpace)
					{
						configurator.NonAspectPredicates.AddNamespace(item);
					}

			});
			return serviceCollection;
		}
	}
}
