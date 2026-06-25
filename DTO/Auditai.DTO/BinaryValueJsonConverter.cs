using System;
using Newtonsoft.Json;

namespace Auditai.DTO;

internal class BinaryValueJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, ((BinaryValue)value).GetBytes());
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return BinaryValue.FromObject(serializer.Deserialize<byte[]>(reader));
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(BinaryValue);
	}
}
