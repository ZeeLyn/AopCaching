using System;
using System.Collections.Concurrent;

namespace AopCaching.Core
{
	public static class ExtensionMethod
	{
		/// <summary>
		/// Set key generator
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="options"></param>
		public static void AddCacheKeyGenerator<T>(this BaseCacheOptions options) where T : ICacheKeyGenerator
		{
			options.CacheKeyGenerator = typeof(T);
		}
		/// <summary>
		/// Set key generator
		/// </summary>
		/// <param name="options"></param>
		/// <param name="type"></param>
		public static void AddCacheKeyGenerator(this BaseCacheOptions options, Type type)
		{
			options.CacheKeyGenerator = type;
		}

		private static readonly ConcurrentDictionary<Type, object>
			TypeofDefaultValue = new ConcurrentDictionary<Type, object>();

		public static dynamic GetDefaultValue(this Type type)
		{
			if (type.IsValueType)
				return TypeofDefaultValue.GetOrAdd(type, Activator.CreateInstance);
			return null;
		}

		internal static byte[] AsBytes(this string source)
		{
			return System.Text.Encoding.UTF8.GetBytes(source);
		}
	}
}
