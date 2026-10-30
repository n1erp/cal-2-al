namespace System.Reflection.Internal;

internal static class ExceptionUtilities
{
	internal static Exception Unreachable => new InvalidOperationException("This program location is thought to be unreachable.");

	internal static Exception UnexpectedValue(object value)
	{
		return new InvalidOperationException(string.Format("Unexpected value '{0}' of type '{1}'", new object[2]
		{
			value,
			value?.GetType().FullName ?? "<unknown>"
		}));
	}
}
