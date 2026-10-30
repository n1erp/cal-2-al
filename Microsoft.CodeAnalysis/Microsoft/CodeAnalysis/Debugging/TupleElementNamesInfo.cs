using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Debugging;

internal struct TupleElementNamesInfo
{
	internal readonly ImmutableArray<string> ElementNames;

	internal readonly int SlotIndex;

	internal readonly string LocalName;

	internal readonly int ScopeStart;

	internal readonly int ScopeEnd;

	internal TupleElementNamesInfo(ImmutableArray<string> elementNames, int slotIndex, string localName, int scopeStart, int scopeEnd)
	{
		ElementNames = elementNames;
		SlotIndex = slotIndex;
		LocalName = localName;
		ScopeStart = scopeStart;
		ScopeEnd = scopeEnd;
	}
}
