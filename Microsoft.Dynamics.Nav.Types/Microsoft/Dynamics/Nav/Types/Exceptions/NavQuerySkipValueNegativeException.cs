using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavQuerySkipValueNegativeException : NavException
{
	public NavQuerySkipValueNegativeException()
	{
	}

	public NavQuerySkipValueNegativeException(string message)
		: base(message)
	{
	}

	public NavQuerySkipValueNegativeException(string message, string name)
		: base(CreateMessage(message, name))
	{
	}

	public NavQuerySkipValueNegativeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavQuerySkipValueNegativeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string message, string name)
	{
		return string.Format(CultureInfo.CurrentCulture, message, name);
	}
}
