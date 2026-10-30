using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidComparisonException : NavNCLException
{
	public NavNCLInvalidComparisonException()
	{
	}

	public NavNCLInvalidComparisonException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidComparisonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidComparisonException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidComparisonException Create(CultureInfo culture, string navTypeName, string unsupportedTypeName)
	{
		return new NavNCLInvalidComparisonException(string.Format(culture, Lang.InvalidComparison, navTypeName, unsupportedTypeName));
	}
}
