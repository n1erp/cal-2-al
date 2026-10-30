using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ActionMoveMetadataBuilder(ActionSymbol anchorSymbol, ActionSymbol control) : ContentMoveMetadataBuilder<ActionMoveMetadataBuilder, ActionSymbol>(anchorSymbol, control)
{
	protected override string ContentElementName { get; } = "ActionMove";

	protected override string TargetType { get; } = "ActionBaseDefinition";

	protected override string GetSemanticKind(ActionSymbol anchor)
	{
		return MetadataEmitterHelper.GetSemanticKind(anchor);
	}

	protected override string ContainerGetter(ActionSymbol anchor)
	{
		return MetadataEmitterHelper.GetContainerType(SyntaxFacts.GetActionAreaKind(anchor.GetArea().Name));
	}

	protected override bool IsArea(ActionSymbol group)
	{
		return group.ActionKind == ActionKind.Area;
	}
}
