using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLStreamWideTextMaxLengthException : NavNCLException
{
	public NavNCLStreamWideTextMaxLengthException()
	{
	}

	public NavNCLStreamWideTextMaxLengthException(string message)
		: base(message)
	{
	}

	public NavNCLStreamWideTextMaxLengthException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLStreamWideTextMaxLengthException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLStreamWideTextMaxLengthException Create(CultureInfo culture)
	{
		return new NavNCLStreamWideTextMaxLengthException(string.Format(culture, Lang.StreamWideTextMaxLength));
	}
}
