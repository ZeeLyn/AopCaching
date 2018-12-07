using System;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using IAopCaching = AopCaching.Core.IAopCaching;

namespace AopCaching.Redis
{
	[NonAspect]
	public class RedisPartitionCaching : IAopCaching
	{
		[FromContainer]
		public IDistributedCache Cache { get; set; }

		public void Set(string key, object value, Type type, TimeSpan expire)
		{
			Cache.SetObject(key, JsonConvert.SerializeObject(value, _jsonSerializerSettings()), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expire
			});
		}

		public (dynamic Value, bool HasKey) Get(string key, Type type)
		{
			var json = Cache.GetObject<string>(key);
			if (string.IsNullOrWhiteSpace(json))
				return (null, false);
			dynamic value = JsonConvert.DeserializeObject(json, type, _jsonSerializerSettings());
			if (value is null)
				return (null, false);
			return (value, true);
		}

		public void Remove(params string[] keys)
		{
			Cache.Remove(string.Join("|", keys));
		}

		private readonly Func<JsonSerializerSettings> _jsonSerializerSettings = () =>
		{
			var st = new JsonSerializerSettings();
			st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			st.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			st.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
			return st;
		};
	}
}
