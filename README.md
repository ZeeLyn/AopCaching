# AspectCaching
A aop cache library, No code intrusion on the cached method. Support memory and redis,redis support partition. And support bloom filter.

# Packages & Status
Packages | NuGet
---------|------
AspectCaching.Core|[![NuGet package](https://buildstats.info/nuget/Extensions.Configuration.Consul)](https://www.nuget.org/packages/Extensions.Configuration.Consul)

# Usage
### Use Autofac
```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			var builder = new ContainerBuilder();
			builder.Populate(services);

			builder.AddAspectCacheInRedis(options =>
			{
				options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=1", "localhost:6380,password=123456,defaultDatabase=1" };
				options.Expiration = TimeSpan.FromMinutes(10);
                options.UsePartition = true;
				options.CacheMethodFilter = new CacheMethodFilter
				{
					IncludeService = new[] { "AspectCaching.WebApi.CacheService" }
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
			services.AddAspectCacheInRedis(options =>
			{
				options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=1", "localhost:6380,password=123456,defaultDatabase=1" };
				options.Expiration = TimeSpan.FromMinutes(10);
                options.UsePartition = true;
				options.CacheMethodFilter = new CacheMethodFilter
				{
					IncludeService = new[] { "AspectCaching.WebApi.CacheService" }
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
		[AspectCaching(Key = "GetTime:{0}", BloomFilter = OptionBoolean.Enable, Expiration = 30)]
		public virtual DateTime GetTime(int id)
		{
			return DateTime.Now;
		}
    }
```