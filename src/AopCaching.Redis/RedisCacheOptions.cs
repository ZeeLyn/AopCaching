using System;
using System.Collections.Generic;
using AopCaching.Core;
using AspectCore.DynamicProxy;

namespace AopCaching.Redis
{
	[NonAspect]
	public class RedisCacheOptions : BaseCacheOptions
	{
		public IEnumerable<string> Endpoints { get; set; }

		public bool UsePartition { get; set; }

		/// <summary>
		/// Return value format-> ip:port/database
		/// sample-> 127.0.0.1:6379/10
		/// </summary>
		public Func<string, string> NodeRule { get; set; }
	}
}
