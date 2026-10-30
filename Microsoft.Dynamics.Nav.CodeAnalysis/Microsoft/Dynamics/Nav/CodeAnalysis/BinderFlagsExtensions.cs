namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class BinderFlagsExtensions
{
	public static bool Includes(this BinderFlags self, BinderFlags other)
	{
		return (self & other) == other;
	}
}
