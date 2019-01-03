using System;
using MessagePack;
using MessagePack.Resolvers;

namespace AopCaching.Core
{
	public class DataSerializer
	{
		static DataSerializer()
		{
			CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
			MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolverAllowPrivate.Instance);
		}

		public static byte[] Serialize(object data)
		{
			return MessagePackSerializer.Serialize(data);
		}

		public static object Deserialize(byte[] data, Type type)
		{
			return data == null ? null : MessagePackSerializer.NonGeneric.Deserialize(type, data);
		}

		public static string ToJson(object data)
		{
			return MessagePackSerializer.ToJson(data);
		}
	}
}
