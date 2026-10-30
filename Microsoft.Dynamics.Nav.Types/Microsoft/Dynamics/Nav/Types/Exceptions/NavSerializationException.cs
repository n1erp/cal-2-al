using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSerializationException : NavException
{
	public NavSerializationException()
	{
	}

	public NavSerializationException(string message)
		: base(message)
	{
	}

	public NavSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public static NavSerializationException Create(string assemblyName, string typeName)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "The assembly or type cannot be deserialized, Assembly: {0}, Type; {1}", assemblyName, typeName);
		return new NavSerializationException(text)
		{
			DiagnosticsMessage = text
		};
	}

	protected NavSerializationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
