using System;
using System.Linq;
using System.Threading.Tasks;
using AopCaching.Core;
using AspectCore.Configuration;
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
			serviceCollection.AddSingleton<IAopCaching, MemoryCaching>();
			if (options.PreventPenetrationPolicy?.BloomFilterPolicy != null)
			{
				if (options.PreventPenetrationPolicy.BloomFilterPolicy.Enable)
				{
					serviceCollection.AddSingleton<IBloomFilter>(FilterBuilder.Build<string>(options.PreventPenetrationPolicy.BloomFilterPolicy.ExpectedElements, options.PreventPenetrationPolicy.BloomFilterPolicy.ErrorRate));
				}
			}
			serviceCollection.ConfigureDynamicProxy(configurator =>
			{
				configurator.ThrowAspectException = false;
				//Exclude methods that do not return a value
				configurator.NonAspectPredicates.Add(method => method.ReturnType == typeof(void) || method.ReturnType == typeof(Task));

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
