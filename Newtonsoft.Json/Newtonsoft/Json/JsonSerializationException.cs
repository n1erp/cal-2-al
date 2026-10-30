using System;

namespace Newtonsoft.Json;

public class JsonSerializationException : JsonException
{
	public JsonSerializationException()
	{
	}

	public JsonSerializationException(string message)
		: base(message)
	{
	}

	public JsonSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	internal static JsonSerializationException Create(JsonReader reader, string message)
	{
		return Create(reader, message, null);
	}

	internal static JsonSerializationException Create(JsonReader reader, string message, Exception ex)
	{
		return Create(reader as IJsonLineInfo, reader.Path, message, ex);
	}

	internal static JsonSerializationException Create(IJsonLineInfo lineInfo, string path, string message, Exception ex)
	{
		message = JsonPosition.FormatMessage(lineInfo, path, message);
		return new JsonSerializationException(message, ex);
	}
}
