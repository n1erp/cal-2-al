using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ControlAddMetadataBuilder(ControlSymbol anchorSymbol, SourceControlSymbol control) : ContentAddMetadataBuilder<ControlAddMetadataBuilder, ControlSymbol, SourceControlSymbol>(anchorSymbol, control)
{
	protected override string ContentElementName { get; } = "ControlAdd";

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
