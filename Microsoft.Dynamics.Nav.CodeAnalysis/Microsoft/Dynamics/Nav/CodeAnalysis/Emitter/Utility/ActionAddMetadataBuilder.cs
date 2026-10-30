using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ActionAddMetadataBuilder(ActionSymbol anchorSymbol, SourceActionSymbol action) : ContentAddMetadataBuilder<ActionAddMetadataBuilder, ActionSymbol, SourceActionSymbol>(anchorSymbol, action)
{
	protected override string ContentElementName { get; } = "ActionAdd";

	protected override string GetSemanticKind(ActionSymbol action)
	{
		return MetadataEmitterHelper.GetSemanticKind(action);
	}

	protected override bool IsArea(ActionSymbol group)
	{
		return group.ActionKind == ActionKind.Area;
	}

	protected override string ContainerGetter(ActionSymbol anchor)
	{
		ActionSymbol area = anchor.GetArea();
		string value = area?.Name.ToUpperInvariant();
		DebugAssertHelper.Assert(area != null, anchor);
		return MetadataEmitterHelper.GetContainerType(SyntaxFacts.GetActionAreaKind(value));
	}
}
