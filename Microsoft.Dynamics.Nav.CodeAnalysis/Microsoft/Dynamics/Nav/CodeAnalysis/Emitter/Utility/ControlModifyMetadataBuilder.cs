using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ControlModifyMetadataBuilder : ContentModifyMetadataBuilder<ControlModifyMetadataBuilder, ControlSymbol>
{
	protected override string ContentElementName { get; } = "ControlChange";

	protected override string GetTargetType(ControlSymbol symbol)
	{
		if (symbol.ControlKind.IsGroup())
		{
			if (symbol.ControlKind == ControlKind.Repeater)
			{
				return "RepeaterDefinition";
			}
			if (symbol.ControlKind == ControlKind.CueGroup)
			{
				return "StackGroupDefinition";
			}
			return "ControlGroupDefinition";
		}
		ControlKind controlKind = symbol.ControlKind;
		if ((uint)(controlKind - 6) <= 1u || controlKind == ControlKind.ChartPart)
		{
			return "ControlBaseDefinition";
		}
		return "ControlDefinition";
	}

	protected override string GetSemanticKind(ControlSymbol anchorSymbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(anchorSymbol);
	}
}
