using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ChangeKindMapping
{
	public IReadOnlyDictionary<ChangeKind, ContentMetadataChangeKind> Mapping { get; private set; }

	public ChangeKindMapping(ChangeKind contentFirst, ChangeKind contentLast, ChangeKind contentBefore, ChangeKind contentAfter)
	{
		Mapping = new Dictionary<ChangeKind, ContentMetadataChangeKind>
		{
			{
				contentFirst,
				ContentMetadataChangeKind.ContentFirst
			},
			{
				contentLast,
				ContentMetadataChangeKind.ContentLast
			},
			{
				contentBefore,
				ContentMetadataChangeKind.ContentBefore
			},
			{
				contentAfter,
				ContentMetadataChangeKind.ContentAfter
			}
		};
	}
}
