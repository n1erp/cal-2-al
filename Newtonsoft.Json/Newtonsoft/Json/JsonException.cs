using System;

namespace Newtonsoft.Json;

public class JsonException : Exception
{
	public JsonException()
	{
	}

	public JsonException(string message)
		: base(message)
	{
	}

	public JsonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	internal static JsonException Create(IJsonLineInfo lineInfo, string path, string message)
	{
		message = JsonPosition.FormatMessage(lineInfo, path, message);
		return new JsonException(message);
	}
}
