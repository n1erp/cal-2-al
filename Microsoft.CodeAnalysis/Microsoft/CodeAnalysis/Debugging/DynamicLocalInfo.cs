using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Debugging;

internal struct DynamicLocalInfo(ImmutableArray<bool> flags, int slotId, string localName)
{
	public readonly ImmutableArray<bool> Flags = flags;

	public readonly int SlotId = slotId;

	public readonly string LocalName = localName;
}
