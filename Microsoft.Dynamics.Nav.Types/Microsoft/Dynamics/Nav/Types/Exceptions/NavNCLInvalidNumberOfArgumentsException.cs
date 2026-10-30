using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLInvalidNumberOfArgumentsException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidNumberOfArgumentsException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidNumberOfArgumentsException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidNumberOfArgumentsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLInvalidNumberOfArgumentsException(string methodName, int expectedNumber, int actualNumber)
		: base(CreateMessage(methodName, expectedNumber, actualNumber))
	{
	}

	private NavNCLInvalidNumberOfArgumentsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string methodName, int expectedNumber, int actualNumber)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50163, methodName, expectedNumber, actualNumber);
	}
}
