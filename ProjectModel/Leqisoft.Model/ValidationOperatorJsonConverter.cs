using System;
using Newtonsoft.Json;

namespace Leqisoft.Model;

internal class ValidationOperatorJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		ValidationOperator validationOperator = (ValidationOperator)value;
		serializer.Serialize(writer, validationOperator.Code);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return ValidationOperator.Operators[(long)serializer.Deserialize(reader)];
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ValidationOperator);
	}
}
