using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime.Data.Graph;

internal class RecordBufferListJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NavGraphException(Lang.MethodNotImplemented);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		while (true)
		{
			switch (reader.TokenType)
			{
			case JsonToken.StartObject:
				list.Add(serializer.Deserialize<ReadOnlyRecordBuffer>(reader));
				continue;
			case JsonToken.EndArray:
				return list;
			}
			if (!reader.Read())
			{
				if (list.Count > 0)
				{
					return list;
				}
				throw new NavGraphException(Lang.GraphDataStreamError);
			}
		}
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(List<ReadOnlyRecordBuffer>);
	}
}
