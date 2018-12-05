
namespace AopCaching.Redis
{
	public class CacheValue<T>
	{
		public CacheValue(T value)
		{
			Value = value;
		}

		public T Value { get; set; }
	}
}
