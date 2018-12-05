using System;
using AopCaching.Core;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using BloomFilter;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace AopCaching.InMemory.DependencyInjection
{
	[NonAspect]
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAopCacheInMemory(this IServiceCollection serviceCollection, Action<MemoryCacheOptions> optionBuilder)
		{
			if (optionBuilder == null)
				throw new ArgumentNullException(nameof(optionBuilder));
			var configure = new MemoryCacheOptions();
			optionBuilder.Invoke(configure);
			return serviceCollection.AddAopCacheInMemory(configure);
		}

		public static IServiceCollection AddAopCacheInMemory(this IServiceCollection serviceCollection, MemoryCacheOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			serviceCollection.AddSingleton<BaseCacheOptions>(options);
			serviceCollection.AddSingleton(typeof(ICacheKeyGenerator), options.CacheKeyGenerator);
			serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();
			serviceCollection.AddSingleton<IAopCaching, AopMemoryCaching>();
			if (options.PreventPenetrationPolicy?.BloomFilterPolicy != null)
			{
				if (options.PreventPenetrationPolicy.BloomFilterPolicy.Enable)
				{
					serviceCollection.AddSingleton<IBloomFilter>(FilterBuilder.Build<string>(options.PreventPenetrationPolicy.BloomFilterPolicy.ExpectedElements, options.PreventPenetrationPolicy.BloomFilterPolicy.ErrorRate));
				}
			}
			serviceCollection.ConfigureDynamicProxy(configurator =>
			{
				RegisterDynamicProxy.Register(configurator, options.CacheMethodFilter);
			});
			return serviceCollection;
		}
	}
}
