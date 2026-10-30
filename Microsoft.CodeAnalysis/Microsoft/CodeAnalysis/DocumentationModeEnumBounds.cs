namespace Microsoft.CodeAnalysis;

internal static class DocumentationModeEnumBounds
{
	internal static bool IsValid(this DocumentationMode value)
	{
		if ((int)value >= 0)
		{
			return (int)value <= 2;
		}
		return false;
	}
}
