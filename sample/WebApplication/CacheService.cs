using System;
using System.Threading.Tasks;
using AopCaching.Core;
using Newtonsoft.Json;

namespace WebApplication
{
	public class CacheService
	{
		//[AopCaching(Key = "GetTime", BloomFilter = AopCacheFunctionSwitch.Enable, Expiration = 30, ShortKey = AopCacheFunctionSwitch.Disable)]
		public virtual DateTime GetTime()
		{
			Console.WriteLine($"--------------exec GetTime---------------");
			return DateTime.Now;
		}

		//[AspectCaching(NoneResultKeyExpiration = 20)]
		public virtual async Task<DateTime> GetTimeAsync()
		{
			Console.WriteLine($"--------------exec GetTimeAsync---------------");
			return await Task.FromResult(default(DateTime));
		}

		public virtual DateTime? GetNullableTime()
		{
			Console.WriteLine($"--------------exec GetNullableTime---------------");
			return DateTime.Now;
		}


		public virtual async Task<DateTime?> GetNullableTimeAsync()
		{
			Console.WriteLine($"--------------exec GetNullableTimeAsync---------------");
			return await Task.FromResult<DateTime?>(null);
		}

		public virtual string GetString(string str)
		{
			Console.WriteLine($"--------------exec GetString---------------");
			return str;
		}

		public virtual async Task<string> GetStringAsync(string str)
		{
			Console.WriteLine($"--------------exec GetStringAsync---------------");
			return await Task.FromResult(str);
		}

		public virtual int GetInt(int id)
		{
			Console.WriteLine($"--------------exec GetInt---------------");
			return id;
		}

		public virtual async Task<int> GetIntAsync(int id)
		{
			Console.WriteLine($"--------------exec GetIntAsync---------------");
			return await Task.FromResult(id);
		}

		public virtual byte[] GetByte()
		{
			Console.WriteLine($"--------------exec GetByte---------------");
			return System.Text.Encoding.UTF8.GetBytes("Thie is test string");
		}

		public virtual async Task<byte[]> GetByteAsync()
		{
			Console.WriteLine($"--------------exec GetByteAsync---------------");
			return await Task.FromResult(System.Text.Encoding.UTF8.GetBytes("Thie is test string"));
		}

		public virtual (bool yes, DateTime? Time) GetTuple()
		{
			Console.WriteLine($"--------------exec GetTuple---------------");
			return (true, default(DateTime?));
		}

		public virtual async Task<(bool yes, DateTime? Time)> GetTupleAsync()
		{
			Console.WriteLine($"--------------exec GetTupleAsync---------------");
			return await Task.FromResult((true, default(DateTime?)));
		}

		public virtual Person GetEntity(Person person)
		{
			Console.WriteLine($"--------------exec GetEntity---------------");
			return person;
		}


		public virtual async Task<Person> GetEntityAsync(Person person)
		{
			Console.WriteLine($"--------------exec GetEntityAsync---------------");
			return await Task.FromResult(person);
		}

		public virtual void GetStringVoid()
		{
			Console.WriteLine($"--------------exec GetStringVoid---------------");
		}

		public virtual async Task GetStringTask()
		{
			Console.WriteLine($"--------------exec GetStringTask---------------");
		}

		public virtual T GetGenericType<T>()
		{
			object value;
			if (typeof(T) == typeof(int))
			{
				value = 99;
				return (T)value;
			}

			if (typeof(T) == typeof(bool))
			{
				value = true;
				return (T)value;
			}

			return default(T);
		}
	}

	public class Person
	{
		public string Name { get; set; }

		public int Age { get; set; }
	}
}
