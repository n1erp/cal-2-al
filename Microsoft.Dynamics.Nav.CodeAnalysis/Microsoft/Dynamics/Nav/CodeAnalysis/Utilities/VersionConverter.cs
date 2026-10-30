using System;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public class VersionConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, value.ToString());
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return Version.Parse(serializer.Deserialize<string>(reader));
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Version);
	}
}
