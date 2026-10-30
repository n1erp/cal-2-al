using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLNotSupportedMethodOnPageException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotSupportedMethodOnPageException()
	{
	}

	public NavNCLNotSupportedMethodOnPageException(string methodName)
		: base(CreateMessage(methodName))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotSupportedMethodOnPageException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLNotSupportedMethodOnPageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string methodName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50116, methodName);
	}
}
