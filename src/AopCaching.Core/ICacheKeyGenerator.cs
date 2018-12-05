using System.Reflection;
using AspectCore.DynamicProxy;

namespace AopCaching.Core
{
	[NonAspect]
	public interface ICacheKeyGenerator
	{
		string GeneratorKey(MethodInfo methodInfo, object[] args, string customKey = "", string prefix = "", bool shortKey = false);
	}
}