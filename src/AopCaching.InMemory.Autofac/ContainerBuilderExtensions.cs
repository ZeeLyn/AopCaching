using System;
using System.Linq;
using System.Threading.Tasks;
using AopCaching.Core;
using AspectCore.Configuration;
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
			containerBuilder.RegisterType<MemoryCaching>().As<IAopCaching>().PropertiesAutowired().SingleInstance();
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
