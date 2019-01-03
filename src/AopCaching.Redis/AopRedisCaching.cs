using System;
using AopCaching.Core;
using AspectCore.DynamicProxy;

namespace AopCaching.Redis
{
	[NonAspect]
	public class AopRedisCaching : IAopCaching
	{
		//private static MethodInfo Method { get; }

		//protected internal static readonly ConcurrentDictionary<Type, MethodInfo>
		//	TypeofGenericMethods = new ConcurrentDictionary<Type, MethodInfo>();

		//protected internal static readonly ConcurrentDictionary<Type, Type>
		//	ValueGenericType = new ConcurrentDictionary<Type, Type>();

		//static AopRedisCaching()
		//{
		//Method = typeof(RedisHelper).GetMethods().First(p => p.Name == "Get" && p.ContainsGenericParameters);
		//}


		public void Set(string key, object value, Type type, TimeSpan expire)
		{
			RedisHelper.Set(key, DataSerializer.Serialize(value), (int)expire.TotalSeconds);
		}

		public (dynamic Value, bool HasKey) Get(string key, Type type)
		{
			var bytes = RedisHelper.Get<byte[]>(key);
			if ((bytes?.LongLength ?? 0) == 0)
				return (null, false);
			return (DataSerializer.Deserialize(bytes, type), true);
		}

		public void Remove(params string[] keys)
		{
			RedisHelper.Del(keys);
		}
	}
}