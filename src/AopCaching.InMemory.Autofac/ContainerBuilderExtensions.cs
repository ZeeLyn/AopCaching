using System;
using AopCaching.Core;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using Autofac;
using BloomFilter;
using Microsoft.Extensions.Caching.Memory;

namespace AopCaching.InMemory.Autofac
{
	[NonAspect]
	public static class ContainerBuilderExtensions
	{
		public static ContainerBuilder AddAopCacheInMemory(this ContainerBuilder containerBuilder, Action<MemoryCacheOptions> optionBuilder)
		{
			if (optionBuilder == null)
				throw new ArgumentNullException(nameof(optionBuilder));
			var configure = new MemoryCacheOptions();
			optionBuilder.Invoke(configure);
			return containerBuilder.AddAopCacheInMemory(configure);
		}

		public static ContainerBuilder AddAopCacheInMemory(this ContainerBuilder containerBuilder, MemoryCacheOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			containerBuilder.RegisterInstance(options).As<BaseCacheOptions>().SingleInstance();
			containerBuilder.RegisterType(options.CacheKeyGenerator).As<ICacheKeyGenerator>().PropertiesAutowired().SingleInstance();
			containerBuilder.RegisterType<MemoryCache>().As<IMemoryCache>().PropertiesAutowired().SingleInstance();
			containerBuilder.RegisterType<AopMemoryCaching>().As<IAopCaching>().PropertiesAutowired().SingleInstance();
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
