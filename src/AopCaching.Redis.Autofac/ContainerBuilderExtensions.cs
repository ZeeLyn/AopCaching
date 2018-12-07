using System;
using System.Linq;
using AopCaching.Core;
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
				RegisterDynamicProxy.Register(configurator, options.CacheMethodFilter);
			});
			return containerBuilder;
		}
	}
}
