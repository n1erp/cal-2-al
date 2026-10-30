using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLTestCodeUnitNestedInvocationException : NavNCLException
{
	public NavNCLTestCodeUnitNestedInvocationException()
	{
	}

	public NavNCLTestCodeUnitNestedInvocationException(string message)
		: base(message)
	{
	}

	public NavNCLTestCodeUnitNestedInvocationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTestCodeUnitNestedInvocationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLTestCodeUnitNestedInvocationException Create(CultureInfo culture, int codeUnitId, string codeUnitName)
	{
		return new NavNCLTestCodeUnitNestedInvocationException(string.Format(culture, Lang.TestCodeUnitsDoNotNest, codeUnitId, codeUnitName));
	}
}
