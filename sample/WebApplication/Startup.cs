using System;
using System.Reflection;
using AopCaching.Core;
using AopCaching.InMemory.Autofac;
using AopCaching.InMemory.DependencyInjection;
using AopCaching.Redis.Autofac;
using AopCaching.Redis.DependencyInjection;
using AspectCore.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public IContainer ApplicationContainer { get; private set; }
		// This method gets called by the runtime. Use this method to add services to the container.
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			services.AddSingleton<CacheService>();


			#region DependencyInjection

			#region Use redis

			//services.AddAopCacheInRedis(options =>
			//{
			//	options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=15", "localhost:6380,password=123456,defaultDatabase=15" };
			//	options.UsePartition = true;
			//	options.Expiration = TimeSpan.FromMinutes(10);
			//	options.CacheMethodFilter = new CacheMethodFilter
			//	{
			//		IncludeService = new[] { "WebApplication.*Service" }
			//	};
			//	options.PreventPenetrationPolicy = new PreventPenetrationPolicy
			//	{
			//		BasicPolicy = new BasicPolicy
			//		{
			//			NoneResultKeyExpiration = TimeSpan.FromMinutes(10)
			//		},
			//		BloomFilterPolicy = new BloomFilterPolicy
			//		{
			//			Enable = true,
			//		}
			//	};
			//});
			#endregion

			#region Use memory
			//services.AddAopCacheInMemory(options =>
			//{
			//	options.CacheMethodFilter = new CacheMethodFilter
			//	{
			//		IncludeService = new[] { "WebApplication.*Service" }
			//	};
			//	options.PreventPenetrationPolicy = new PreventPenetrationPolicy
			//	{
			//		BloomFilterPolicy = new BloomFilterPolicy
			//		{
			//			Enable = true
			//		}
			//	};
			//});
			#endregion

			//return services.BuildAspectInjectorProvider();
			#endregion


			#region Autofac
			var builder = new ContainerBuilder();
			builder.Populate(services);


			#region Use memory
			//builder.AddAopCacheInMemory(options =>
			//{
			//	options.CacheMethodFilter = new CacheMethodFilter
			//	{
			//		IncludeService = new[] { "WebApplication.*CacheService" }
			//	};
			//	options.PreventPenetrationPolicy = new PreventPenetrationPolicy
			//	{
			//		BloomFilterPolicy = new BloomFilterPolicy
			//		{
			//			Enable = true
			//		}
			//	};
			//});
			#endregion

			#region Use redis
			builder.AddAopCacheInRedis(options =>
			{
				options.Endpoints = new[] { "localhost:6379,password=123456,defaultDatabase=15", "localhost:6380,password=123456,defaultDatabase=15" };
				//options.ShortKey = true;
				//options.UsePartition = true;
				options.Expiration = TimeSpan.FromMinutes(10);
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
			#endregion

			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
							.Where(t => t.Name.EndsWith("Controller"))
							.PropertiesAutowired().InstancePerLifetimeScope();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);

			#endregion
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
		}
	}
}
