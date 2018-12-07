using System;
using AspectCore.DynamicProxy;

namespace AopCaching.Core
{
	[NonAspect]
	[AttributeUsage(AttributeTargets.Method)]
	public class AopCachingAttribute : Attribute
	{
		/// <summary>
		/// Cached key
		/// Use the parameter value of the {index} alternative.
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Generate a 32-character MD5 string key that takes up little space but is poorly readable.
		/// </summary>
		public AopCacheFunctionSwitch ShortKey { get; set; } = AopCacheFunctionSwitch.Ignore;

		/// <summary>
		/// Expiration time(second)
		/// </summary>
		public int Expiration { get; set; } = -1;

		/// <summary>
		/// Create a key when the method return value has no result set, preventing cache penetration.
		/// </summary>
		public AopCacheFunctionSwitch NoneResultForceSetKey { get; set; } = AopCacheFunctionSwitch.Ignore;

		/// <summary>
		/// The expiration time of the key when the method returns no value set.
		/// </summary>
		public int NoneResultKeyExpiration { get; set; } = -1;

		/// <summary>
		/// Use bloom filter.
		/// </summary>
		public AopCacheFunctionSwitch BloomFilter { get; set; } = AopCacheFunctionSwitch.Ignore;
	}


	public enum AopCacheFunctionSwitch
	{
		Ignore = -1,
		Disable = 0,
		Enable = 1
	}
}
