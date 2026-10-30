using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ExceptionUtilities
{
	public static Exception Unreachable => new InvalidOperationException("This program location is thought to be unreachable.");

	[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
	public static Exception SkippedForNow => new NotImplementedException("This is currently not implemented in the AL version.");

	public static Exception UnexpectedValue(object value)
	{
		string message = string.Format(CultureInfo.InvariantCulture, "Unexpected value '{0}' of type '{1}'", value, (value != null) ? value.GetType().FullName : "<unknown>");
		DebugAssertHelper.Assert(condition: false, message);
		return new InvalidOperationException(message);
	}

	public static Exception UnreachableWithMessage(string message = "")
	{
		return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "This program location is thought to be unreachable. {0}", message));
	}
}
