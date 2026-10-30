using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLTypeException : NavNCLException
{
	public NavNCLTypeException()
	{
	}

	public NavNCLTypeException(string message)
		: base(message)
	{
	}

	public NavNCLTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLNotSupportedTypeException CreateWrongLengthException(NavType type)
	{
		return new NavNCLNotSupportedTypeException(string.Format(CultureInfo.InvariantCulture, "The supplied {0} does not contain the correct length.", type.ToString()));
	}

	public static NavNCLNotSupportedTypeException CreateSourceHasWrongLengthException(NavType type)
	{
		return new NavNCLNotSupportedTypeException(string.Format(CultureInfo.InvariantCulture, "The supplied data source with type {0} does not contain the correct length.", type.ToString()));
	}
}
