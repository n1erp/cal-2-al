namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class MessageIDExtensions
{
	public static LocalizableErrorArgument Localize(this MessageID id)
	{
		return new LocalizableErrorArgument(id);
	}
}
