using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.Model;

public class FilterJsonConverter : JsonConverter<FilterBase>
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, FilterBase value, JsonSerializer serializer)
	{
	}

	public override FilterBase ReadJson(JsonReader reader, Type objectType, FilterBase existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		FilterBase filterBase = FilterBase.CreateFromKind((FilterKind)jObject.Value<int>("Kind"));
		serializer.Populate(jObject.CreateReader(), filterBase);
		return filterBase;
	}
}
