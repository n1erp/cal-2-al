using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class GreenStats
{
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "node")]
	internal static void NoteGreen(InternalSyntaxNode node)
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
