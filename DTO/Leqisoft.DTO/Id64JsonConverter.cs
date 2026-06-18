using System;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

internal class Id64JsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, ((Id64)value).Value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}
		return new Id64(serializer.Deserialize<long>(reader));
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Id64);
	}
}
