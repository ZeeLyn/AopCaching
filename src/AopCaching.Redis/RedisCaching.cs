using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using IAopCaching = AopCaching.Core.IAopCaching;

namespace AopCaching.Redis
{
	[NonAspect]
	public class RedisCaching : IAopCaching
	{
		private static MethodInfo Method { get; }

		protected internal static readonly ConcurrentDictionary<Type, MethodInfo>
			TypeofGenericMethods = new ConcurrentDictionary<Type, MethodInfo>();

		protected internal static readonly ConcurrentDictionary<Type, Type>
			ValueGenericType = new ConcurrentDictionary<Type, Type>();

		static RedisCaching()
		{
			Method = typeof(RedisHelper).GetMethods().First(p => p.Name == "Get" && p.ContainsGenericParameters);
		}


		public void Set(string key, object value, Type type, TimeSpan expire)
		{
			//var valueType = ValueGenericType.GetOrAdd(type, t => typeof(CacheValue<>).MakeGenericType(t));
			//Activator.CreateInstance(valueType, value)
			RedisHelper.Set(key, new CacheValue<object>(value), (int)expire.TotalSeconds);
		}

		public (dynamic Value, bool HasKey) Get(string key, Type type)
		{
			var helper = TypeofGenericMethods.GetOrAdd(type,
				t => Method.MakeGenericMethod(ValueGenericType.GetOrAdd(t, tp => typeof(CacheValue<>).MakeGenericType(tp))));
			dynamic value = helper.Invoke(null, new object[] { key });
			if (value is null)
				return (null, false);
			return (value.Value, true);
		}

		public void Remove(params string[] keys)
		{
			RedisHelper.Del(keys);
		}
	}
}