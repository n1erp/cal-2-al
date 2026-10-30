using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ControlMoveMetadataBuilder(ControlSymbol anchorSymbol, ControlSymbol control) : ContentMoveMetadataBuilder<ControlMoveMetadataBuilder, ControlSymbol>(anchorSymbol, control)
{
	protected override string ContentElementName { get; } = "ControlMove";

	protected override string TargetType { get; } = "ControlBaseDefinition";

	protected override string ContainerGetter(ControlSymbol anchor)
	{
		return MetadataEmitterHelper.GetContainerType(anchor.GetArea());
	}

	protected override bool IsArea(ControlSymbol group)
	{
		return group.ControlKind == ControlKind.Area;
	}

	protected override string GetSemanticKind(ControlSymbol anchorSymbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(anchorSymbol);
	}
}
