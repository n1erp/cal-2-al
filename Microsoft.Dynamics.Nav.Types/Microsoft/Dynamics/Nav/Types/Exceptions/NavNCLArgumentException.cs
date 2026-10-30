using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArgumentException : NavNCLException
{
	public NavNCLArgumentException()
	{
	}

	public NavNCLArgumentException(string message)
		: base(message)
	{
	}

	public NavNCLArgumentException(object argument, string methodName, int argumentNumber)
		: base(CreateMessage(argument, methodName, argumentNumber))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLArgumentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArgumentException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(object argument, string methodName, int argumentNumber)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50165, argument, methodName, argumentNumber);
	}
}
