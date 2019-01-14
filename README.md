![AopCaching](banner.png?raw=true)


# AopCaching
A aop cache library, No code intrusion on the cached method. Support memory and redis,redis support partition. And support bloom filter.

# Note
The proxy method must be a virtual method or an interface.

# Packages & Status

### Core
Packages | NuGet
---------|------
AopCaching.Core|[![NuGet package](https://buildstats.info/nuget/AopCaching.Core)](https://www.nuget.org/packages/AopCaching.Core)

### Provider
Packages | NuGet
---------|------
AopCaching.InMemory|[![NuGet package](https://buildstats.info/nuget/AopCaching.InMemory)](https://www.nuget.org/packages/AopCaching.InMemory)
AopCaching.Redis|[![NuGet package](https://buildstats.info/nuget/AopCaching.Redis)](https://www.nuget.org/packages/AopCaching.Redis)

### Injection
Packages | NuGet
---------|------
AopCaching.InMemory.Autofac|[![NuGet package](https://buildstats.info/nuget/AopCaching.InMemory.Autofac)](https://www.nuget.org/packages/AopCaching.InMemory.Autofac)
AopCaching.InMemory.DependencyInjection|[![NuGet package](https://buildstats.info/nuget/AopCaching.InMemory.DependencyInjection)](https://www.nuget.org/packages/AopCaching.InMemory.DependencyInjection)
AopCaching.Redis.Autofac|[![NuGet package](https://buildstats.info/nuget/AopCaching.Redis.Autofac)](https://www.nuget.org/packages/AopCaching.Redis.Autofac)
AopCaching.Redis.DependencyInjection|[![NuGet package](https://buildstats.info/nuget/AopCaching.Redis.DependencyInjection)](https://www.nuget.org/packages/AopCaching.Redis.DependencyInjection)

# Dependencies
Packages | Description
---------|------
[AspectCore](https://github.com/dotnetcore/AspectCore-Framework) | An Aspect-Oriented Programming based cross platform framework for .NET Core and .NET Framework.Core support for aspect-interceptor,dependency injection integration , web applications , data validation , and more.
[CSRedisCore](https://github.com/2881099/csredis) | A High-performance redis client.
[BloomFilter.NetCore](https://github.com/vla/BloomFilter.NetCore) | Library Bloom filters in C# with optional Redis-backing.


# Usage
### Use Autofac
```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			var builder = new ContainerBuilder();
			builder.Populate(services);

			builder.AddAopCacheInRedis(options =>
			{
				options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=1", "localhost:6380,password=123456,defaultDatabase=1" };
				options.Expiration = TimeSpan.FromMinutes(10);
                options.UsePartition = true;
				options.CacheMethodFilter = new CacheMethodFilter
				{
					IncludeService = new[] { "WebApplication.*Service" }
				};
				options.PreventPenetrationPolicy = new PreventPenetrationPolicy
				{
					BasicPolicy = new BasicPolicy
					{
						NoneResultKeyExpiration = TimeSpan.FromMinutes(10)
					},
					BloomFilterPolicy = new BloomFilterPolicy
					{
						Enable = true,
					}
				};
			});

			builder.RegisterType<CacheService>().PropertiesAutowired().SingleInstance();
			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.PropertiesAutowired().InstancePerLifetimeScope();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```

### Use DependencyInjection

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			services.AddAopCacheInRedis(options =>
			{
				options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=1", "localhost:6380,password=123456,defaultDatabase=1" };
				options.Expiration = TimeSpan.FromMinutes(10);
                options.UsePartition = true;
				options.CacheMethodFilter = new CacheMethodFilter
				{
					IncludeService = new[] { "WebApplication.*Service" }
				};
				options.PreventPenetrationPolicy = new PreventPenetrationPolicy
				{
					BasicPolicy = new BasicPolicy
					{
						NoneResultKeyExpiration = TimeSpan.FromMinutes(10)
					},
					BloomFilterPolicy = new BloomFilterPolicy
					{
						Enable = true,
					}
				};
			});

			services.AddSingleton<CacheService>();
			return services.BuildAspectInjectorProvider();
		}
```

### If you want to use a different configuration on some methods, you can use custom attribute.

```csharp
	public class CacheService
	{
		[AopCaching(Key = "GetTime:{0}", BloomFilter = AopCacheFunctionSwitch.Enable, Expiration = 30)]
		public virtual DateTime GetTime(int id)
		{
			return DateTime.Now;
		}
    }
```

### Exclude methods

```csharp
	public class CacheService
	{
		[NonAopCaching]
		public virtual DateTime GetTime(int id)
		{
			return DateTime.Now;
		}
    }
```