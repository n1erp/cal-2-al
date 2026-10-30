using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal abstract class ContentDestinationMetadataBuilder<T, TV> : ContentMetadataBuilder where T : ContentDestinationMetadataBuilder<T, TV> where TV : Symbol, ISymbolWithId
{
	protected ChangeKindMapping ChangeMapping { get; private set; }

	private TV PreviousSymbol { get; set; }

	protected ContentDestinationMetadataBuilder(ChangeKindMapping mapping)
	{
		DebugAssertHelper.Assert(mapping != null);
		ChangeMapping = mapping;
	}

	protected abstract string ContainerGetter(TV anchor);

	protected abstract bool IsArea(TV group);

	protected abstract string GetSemanticKind(TV symbol);

	public T WithPreviousSymbol(TV previousSymbol)
	{
		PreviousSymbol = previousSymbol;
		return this as T;
	}

	protected virtual void SetContentMetadata(ref ContentDestinationMetadata metadata, ChangeKind changeKind, TV anchor)
	{
		ContentMetadata metadata2 = metadata;
		SetContentMetadata(ref metadata2);
		metadata.ParentContainer = ContainerGetter(anchor);
		metadata.Operation = ChangeMapping.Mapping[changeKind];
		if (PreviousSymbol != null)
		{
			switch (metadata.Operation)
			{
			case ContentMetadataChangeKind.ContentAfter:
				anchor = PreviousSymbol;
				break;
			case ContentMetadataChangeKind.ContentFirst:
				metadata.Operation = ContentMetadataChangeKind.ContentAfter;
				anchor = PreviousSymbol;
				break;
			}
		}
		bool flag = IsArea(anchor);
		metadata.AnchorId = ((flag || ((object)anchor != null && anchor.Id == 0)) ? ((int?)null) : anchor?.Id);
		metadata.AnchorName = (flag ? null : GetAnchorName(anchor));
	}

	protected virtual string GetAnchorName(TV symbol)
	{
		return symbol.Name;
	}
}
