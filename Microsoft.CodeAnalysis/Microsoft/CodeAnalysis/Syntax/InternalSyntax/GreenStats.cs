using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal class GreenStats
{
	internal static void NoteGreen(GreenNode node)
	{
	}

	[Conditional("DEBUG")]
	internal static void ItemAdded()
	{
	}

	[Conditional("DEBUG")]
	internal static void ItemCacheable()
	{
	}

	[Conditional("DEBUG")]
	internal static void CacheHit()
	{
	}
}
