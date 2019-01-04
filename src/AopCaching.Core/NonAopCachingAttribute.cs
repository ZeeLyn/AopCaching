using System;

namespace AopCaching.Core
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class NonAopCachingAttribute : Attribute
	{
	}
}
