using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLClassNotFoundException : NavNCLException
{
	public NavNCLClassNotFoundException()
	{
	}

	public NavNCLClassNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLClassNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLClassNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLClassNotFoundException Create(CultureInfo culture, string className)
	{
		return new NavNCLClassNotFoundException(string.Format(culture, Lang._00095_50034, className));
	}
}
