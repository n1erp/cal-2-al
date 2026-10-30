using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal abstract class ContentModifyMetadataBuilder<TU, TV> : ContentMetadataBuilder where TU : ContentModifyMetadataBuilder<TU, TV> where TV : Symbol, ISymbolWithId
{
	private int TargetId { get; set; }

	private string TargetName { get; set; }

	private string TargetType { get; set; }

	private string SemanticKind { get; set; }

	protected virtual string GetTargetName(TV symbol)
	{
		return symbol.Name;
	}

	protected abstract string GetTargetType(TV symbol);

	protected abstract string GetSemanticKind(TV symbol);

	public TU ForTarget(TV target)
	{
		DebugAssertHelper.Assert(target != null);
		TargetType = GetTargetType(target);
		TargetId = target.Id;
		TargetName = GetTargetName(target);
		SemanticKind = GetSemanticKind(target);
		return this as TU;
	}

	protected void SetContentMetadata(ref ContentModifyMetadata metadata)
	{
		ContentMetadata metadata2 = metadata;
		SetContentMetadata(ref metadata2);
		metadata.TargetId = TargetId;
		metadata.TargetName = TargetName;
		metadata.TargetType = TargetType;
		metadata.SemanticKind = SemanticKind;
	}

	public ContentModifyMetadata Build()
	{
		ContentModifyMetadata metadata = new ContentModifyMetadata();
		SetContentMetadata(ref metadata);
		return metadata;
	}
}
