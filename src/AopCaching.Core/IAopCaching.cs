using System;
using AspectCore.DynamicProxy;

namespace AopCaching.Core
{
	[NonAspect]
	public interface IAopCaching
	{
		void Set(string key, object value, Type type, TimeSpan expire);

		(dynamic Value, bool HasKey) Get(string key, Type type);

		void Remove(params string[] keys);
	}
}
