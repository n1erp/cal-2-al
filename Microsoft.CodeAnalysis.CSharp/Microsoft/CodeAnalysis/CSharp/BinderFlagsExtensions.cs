namespace Microsoft.CodeAnalysis.CSharp;

internal static class BinderFlagsExtensions
{
	public static bool Includes(this BinderFlags self, BinderFlags other)
	{
		return (self & other) == other;
	}

	public static bool IncludesAny(this BinderFlags self, BinderFlags other)
	{
		return (self & other) != 0;
	}
}
