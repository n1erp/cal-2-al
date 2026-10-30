using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal abstract class ContentMoveMetadataBuilder<T, TU> : ContentDestinationMetadataBuilder<ContentMoveMetadataBuilder<T, TU>, TU> where T : ContentMoveMetadataBuilder<T, TU> where TU : Symbol, ISymbolWithId
{
	protected abstract string TargetType { get; }

	private TU Target { get; set; }

	private TU Anchor { get; set; }

	private ChangeKind MoveChangeKind { get; set; }

	protected ContentMoveMetadataBuilder(TU anchorSymbol, TU target)
		: base(new ChangeKindMapping(ChangeKind.MoveFirst, ChangeKind.MoveLast, ChangeKind.MoveBefore, ChangeKind.MoveAfter))
	{
		DebugAssertHelper.Assert(target != null);
		DebugAssertHelper.Assert(target.Kind == SymbolKind.View || anchorSymbol != null);
		Anchor = anchorSymbol;
		Target = target;
	}

	public T ForMoveType(ChangeKind moveChangeKind)
	{
		if ((uint)(moveChangeKind - 5) <= 3u)
		{
			MoveChangeKind = moveChangeKind;
			return this as T;
		}
		throw ExceptionUtilities.UnexpectedValue(moveChangeKind);
	}

	protected virtual string GetTargetName(TU symbol)
	{
		return symbol.Name;
	}

	protected void SetContentMetadata(ref ContentMoveMetadata metadata)
	{
		ContentDestinationMetadata metadata2 = metadata;
		base.SetContentMetadata(ref metadata2, MoveChangeKind, Anchor);
		metadata.TargetId = Target.Id;
		metadata.TargetName = GetTargetName(Target);
		metadata.TargetType = TargetType;
		metadata.SemanticKind = GetSemanticKind(Anchor);
	}

	public ContentMoveMetadata Build()
	{
		ContentMoveMetadata metadata = new ContentMoveMetadata();
		SetContentMetadata(ref metadata);
		return metadata;
	}
}
