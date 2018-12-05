using System;
using System.Linq;
using System.Threading.Tasks;
using AopCaching.Core;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using Autofac;
using BloomFilter;
using Microsoft.Extensions.Caching.Distributed;

namespace AopCaching.Redis.Autofac
{
	[NonAspect]
	public static class ContainerBuilderExtensions
	{
		public static ContainerBuilder AddAopCacheInRedis(this ContainerBuilder containerBuilder, Action<RedisCacheOptions> optionBuilder)
		{
			if (optionBuilder == null)
				throw new ArgumentNullException(nameof(optionBuilder));
			var configure = new RedisCacheOptions();
			optionBuilder.Invoke(configure);
			return containerBuilder.AddAopCacheInRedis(configure);
		}

		public static ContainerBuilder AddAopCacheInRedis(this ContainerBuilder containerBuilder, RedisCacheOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			containerBuilder.RegisterInstance(options).As<BaseCacheOptions>().SingleInstance();

			if (options.UsePartition)
			{
				RedisHelper.Initialization(new CSRedis.CSRedisClient(options.NodeRule, options.Endpoints.ToArray()));
				containerBuilder.Register<IDistributedCache>(ctx => new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance)).PropertiesAutowired().SingleInstance();
				containerBuilder.RegisterType<RedisPartitionCaching>().As<IAopCaching>().PropertiesAutowired().SingleInstance();
			}
			else
			{
				RedisHelper.Initialization(new CSRedis.CSRedisClient(options.Endpoints.First()));
				containerBuilder.RegisterType<AopRedisCaching>().As<IAopCaching>().PropertiesAutowired().SingleInstance();
			}

			containerBuilder.RegisterType(options.CacheKeyGenerator).As<ICacheKeyGenerator>().PropertiesAutowired().SingleInstance();

			if (options.PreventPenetrationPolicy?.BloomFilterPolicy != null)
			{
				if (options.PreventPenetrationPolicy.BloomFilterPolicy.Enable)
				{
					containerBuilder.RegisterInstance(FilterBuilder.Build<string>(options.PreventPenetrationPolicy.BloomFilterPolicy.ExpectedElements, options.PreventPenetrationPolicy.BloomFilterPolicy.ErrorRate)).As<IBloomFilter>().SingleInstance();
				}
			}

			containerBuilder.RegisterDynamicProxy(configurator =>
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
			return containerBuilder;
		}
	}
}
