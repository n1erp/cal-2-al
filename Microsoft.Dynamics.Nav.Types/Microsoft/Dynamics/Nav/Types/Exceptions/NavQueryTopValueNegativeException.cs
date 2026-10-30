using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavQueryTopValueNegativeException : NavException
{
	public NavQueryTopValueNegativeException()
	{
	}

	public NavQueryTopValueNegativeException(string message)
		: base(message)
	{
	}

	public NavQueryTopValueNegativeException(string message, string name)
		: base(CreateMessage(message, name))
	{
	}

	public NavQueryTopValueNegativeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavQueryTopValueNegativeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string message, string name)
	{
		return string.Format(CultureInfo.CurrentCulture, message, name);
	}
}
