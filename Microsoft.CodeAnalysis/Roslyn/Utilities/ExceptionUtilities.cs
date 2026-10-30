using System;

namespace Roslyn.Utilities;

internal static class ExceptionUtilities
{
	internal static Exception Unreachable => new InvalidOperationException("This program location is thought to be unreachable.");

	internal static Exception UnexpectedValue(object o)
	{
		return new InvalidOperationException(string.Format("Unexpected value '{0}' of type '{1}'", o, (o != null) ? o.GetType().FullName : "<unknown>"));
	}
}
