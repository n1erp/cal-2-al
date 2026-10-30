using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavMethodNotSupportedException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavMethodNotSupportedException()
	{
	}

	public NavMethodNotSupportedException(string methodName)
		: base(CreateMessage(methodName))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavMethodNotSupportedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavMethodNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string methodName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50174, methodName);
	}
}
