using System;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaException : JsonException
{
	public int LineNumber { get; }

	public int LinePosition { get; }

	public string Path { get; }

	public JsonSchemaException()
	{
	}

	public JsonSchemaException(string message)
		: base(message)
	{
	}

	public JsonSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	internal JsonSchemaException(string message, Exception innerException, string path, int lineNumber, int linePosition)
		: base(message, innerException)
	{
		Path = path;
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}
}
