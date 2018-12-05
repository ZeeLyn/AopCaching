using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AspectCore.DynamicProxy;
using Newtonsoft.Json;


namespace AopCaching.Core
{
	[NonAspect]
	public class DefaultCacheKeyGenerator : ICacheKeyGenerator
	{
		private const string LinkString = ":";

		public string GeneratorKey(MethodInfo methodInfo, object[] args, string customKey = "", string prefix = "", bool shortKey = false)
		{
			var attribute =
				methodInfo.GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(AopCachingAttribute))
					as AopCachingAttribute;
			if (attribute == null || string.IsNullOrWhiteSpace(attribute.Key))
			{
				var typeName = methodInfo.DeclaringType?.FullName;
				var methodName = methodInfo.Name;
				if (shortKey)
					return
						MD5($"{typeName}{LinkString}{methodName}{(args.Any() ? LinkString : "")}{(args.Any() ? JsonConvert.SerializeObject(args) : "")}");
				return
					$"{(string.IsNullOrWhiteSpace(prefix) ? "" : $"{prefix}{LinkString}")}{typeName}{LinkString}{methodName}{(args.Any() ? LinkString : "")}{(args.Any() ? MD5(JsonConvert.SerializeObject(args)) : "")}";
			}
			return string.IsNullOrWhiteSpace(prefix) ? "" : $"{prefix}{LinkString}" + string.Format(attribute.Key, args);
		}

		private string MD5(string source)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(source);
			using (MD5 md5 = new MD5CryptoServiceProvider())
			{
				var hash = md5.ComputeHash(bytes);
				md5.Clear();
				return BitConverter.ToString(hash).Replace("-", "").ToLower();
			}
		}
	}
}
