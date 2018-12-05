using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AopCaching.Core
{
	[NonAspect]
	public class CacheMethodFilter
	{
		public IEnumerable<string> IncludeService { get; set; }

		public IEnumerable<string> ExcludeService { get; set; }

		public IEnumerable<string> IncludeNameSpace { get; set; }

		public IEnumerable<string> ExcludeNameSpace { get; set; }

		public IEnumerable<string> IncludeMethod { get; set; }

		public IEnumerable<ExcludeMethodInfo> ExcludeMethod { get; set; }
	}

	[NonAspect]
	public class ExcludeMethodInfo
	{
		public string ServiceName { get; set; }

		public string MethodName { get; set; }
	}
}
