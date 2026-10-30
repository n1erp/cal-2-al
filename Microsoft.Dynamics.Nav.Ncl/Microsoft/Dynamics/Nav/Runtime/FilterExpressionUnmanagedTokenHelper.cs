namespace Microsoft.Dynamics.Nav.Runtime;

internal static class FilterExpressionUnmanagedTokenHelper
{
	private const byte SpecialFlag = 128;

	public static FilterExpressionUnmanagedToken MakeNormal(this FilterExpressionUnmanagedToken token)
	{
		return (FilterExpressionUnmanagedToken)((uint)token & 0xFFFFFF7Fu);
	}

	public static FilterExpressionUnmanagedToken MakeSpecial(this FilterExpressionUnmanagedToken token)
	{
		return token | (FilterExpressionUnmanagedToken)0x80;
	}

	public static bool IsSpecial(this FilterExpressionUnmanagedToken token)
	{
		return (token & (FilterExpressionUnmanagedToken)0x80) == (FilterExpressionUnmanagedToken)128;
	}
}
