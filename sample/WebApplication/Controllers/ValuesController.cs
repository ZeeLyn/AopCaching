using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private CacheService CacheHelper { get; }

		public ValuesController(CacheService cacheService)
		{
			CacheHelper = cacheService;
		}

		// GET api/values
		[HttpGet("{str?}")]
		public async Task<IActionResult> Get()
		{

			CacheHelper.GetStringVoid();
			await CacheHelper.GetStringTask();
			return Ok(new
			{
				String = CacheHelper.GetString("This is test string"),
				StringAsync = await CacheHelper.GetStringAsync("This is test string"),
				Int = CacheHelper.GetInt(22),
				IntAsync = await CacheHelper.GetIntAsync(33),
				Time = CacheHelper.GetTime(),
				NullableTime = CacheHelper.GetNullableTime(),
				TimeAsync = await CacheHelper.GetTimeAsync(),
				NullableTimeAsync = await CacheHelper.GetNullableTimeAsync(),
				Bytes = System.Text.Encoding.UTF8.GetString(CacheHelper.GetByte()),
				BytesAsync = System.Text.Encoding.UTF8.GetString(await CacheHelper.GetByteAsync()),
				Tuple = CacheHelper.GetTuple(),
				TupleAsync = await CacheHelper.GetTupleAsync(),
				Entity = CacheHelper.GetEntity(new Person
				{
					Name = "Jack",
					Age = 18
				}),
				EntityAsync = await CacheHelper.GetEntityAsync(new Person
				{
					Name = "Jack",
					Age = 18
				}),
				GetGenericTypeBool = CacheHelper.GetGenericType<bool>(),
				GetGenericTypeInt = CacheHelper.GetGenericType<int>()
			});

		}
	}
}
