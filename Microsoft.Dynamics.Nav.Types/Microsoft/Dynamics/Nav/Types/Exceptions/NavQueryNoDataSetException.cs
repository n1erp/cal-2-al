using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavQueryNoDataSetException : NavException
{
	public NavQueryNoDataSetException()
	{
	}

	public NavQueryNoDataSetException(string message)
		: base(message)
	{
	}

	public NavQueryNoDataSetException(string message, string name)
		: base(CreateMessage(message, name))
	{
	}

	public NavQueryNoDataSetException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavQueryNoDataSetException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string message, string name)
	{
		return string.Format(CultureInfo.CurrentCulture, message, name);
	}
}
