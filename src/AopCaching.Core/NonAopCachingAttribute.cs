using System;

namespace AopCaching.Core
{
	[AttributeUsage(AttributeTargets.Method)]
	public class NonAopCachingAttribute : Attribute
	{
	}
}
