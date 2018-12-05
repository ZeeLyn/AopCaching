using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using BloomFilter;

namespace AopCaching.Core
{
	[NonAspect]
	public class AspectCachingInterceptor : AbstractInterceptor
	{
		[FromContainer] public IAopCaching Cache { get; set; }

		[FromContainer] public ICacheKeyGenerator KeyGenerator { get; set; }

		[FromContainer] public IBloomFilter BloomFilter { get; set; }

		private static readonly ConcurrentDictionary<Type, MethodInfo>
			TypeofTaskResultMethod = new ConcurrentDictionary<Type, MethodInfo>();

		private static readonly MethodInfo TaskResultMethod;

		static AspectCachingInterceptor()
		{
			TaskResultMethod = typeof(Task).GetMethods()
				.First(p => p.Name == "FromResult" && p.ContainsGenericParameters);
		}

		public override async Task Invoke(AspectContext context, AspectDelegate next)
		{
			var options = context.ServiceProvider.GetService(typeof(BaseCacheOptions)) as BaseCacheOptions ??
						  new BaseCacheOptions();
			var attribute =
				context.ServiceMethod.GetCustomAttributes(true)
						.FirstOrDefault(p => p.GetType() == typeof(AspectCachingAttribute))
					as AspectCachingAttribute;

			var returnType = context.IsAsync()
				? context.ServiceMethod.ReturnType.GetGenericArguments().First()
				: context.ServiceMethod.ReturnType;

			var shortKey = options.ShortKey;
			if (attribute != null && attribute.ShortKey != AspectCacheFunctionSwitch.Ignore)
				shortKey = attribute.ShortKey == AspectCacheFunctionSwitch.Enable;

			var key = KeyGenerator.GeneratorKey(context.ServiceMethod, context.Parameters, attribute?.Key,
				options.CacheKeyPrefix, shortKey);

			var enableBloomFilter = options.PreventPenetrationPolicy?.BloomFilterPolicy?.Enable ?? false;
			if (attribute != null && attribute.BloomFilter != AspectCacheFunctionSwitch.Ignore)
			{
				enableBloomFilter = attribute.BloomFilter == AspectCacheFunctionSwitch.Enable;
			}

			if (enableBloomFilter && BloomFilter.Contains(key.AsBytes()))
			{
				Console.WriteLine($"-----------------bloom filter {context.ServiceMethod.Name}---------------");
				context.ReturnValue = context.IsAsync()
					? TypeofTaskResultMethod.GetOrAdd(returnType,
							t => TaskResultMethod.MakeGenericMethod(returnType))
						.Invoke(null, new object[] { returnType.GetDefaultValue() })
					: returnType.GetDefaultValue();
				return;
			}

			var value = Cache.Get(key, returnType);

			if (value.HasKey)
			{
				context.ReturnValue = context.IsAsync()
					? TypeofTaskResultMethod.GetOrAdd(returnType,
						t => TaskResultMethod.MakeGenericMethod(returnType)).Invoke(null, new object[] { value.Value })
					: value.Value;
			}
			else
			{
				await next(context);
				dynamic returnValue = context.IsAsync() ? await context.UnwrapAsyncReturnValue() : context.ReturnValue;
				var noneResultForceSetKey = options.PreventPenetrationPolicy?.BasicPolicy?.Enable ?? true;
				var expire = options.Expiration;

				if (attribute != null)
				{
					if (attribute.NoneResultForceSetKey != AspectCacheFunctionSwitch.Ignore)
						noneResultForceSetKey = attribute.NoneResultForceSetKey == AspectCacheFunctionSwitch.Enable;
					if (attribute.Expiration >= 0)
						expire = TimeSpan.FromSeconds(attribute.Expiration);
				}
				//No result set
				if (returnValue is null || returnValue?.Equals(returnType.GetDefaultValue()))
				{
					if (enableBloomFilter)
						BloomFilter.Add(key.AsBytes());

					if (!noneResultForceSetKey)
						return;

					if (options.PreventPenetrationPolicy?.BasicPolicy != null && options.PreventPenetrationPolicy.BasicPolicy.NoneResultKeyExpiration.Ticks >= 0)
					{
						expire = options.PreventPenetrationPolicy.BasicPolicy.NoneResultKeyExpiration;
					}

					if (attribute?.NoneResultKeyExpiration >= 0)
					{
						expire = TimeSpan.FromSeconds(attribute.NoneResultKeyExpiration);
					}
				}

				if (expire.Ticks <= 0)
					return;

				Cache.Set(key, returnValue, returnType, expire);
			}
		}
	}
}