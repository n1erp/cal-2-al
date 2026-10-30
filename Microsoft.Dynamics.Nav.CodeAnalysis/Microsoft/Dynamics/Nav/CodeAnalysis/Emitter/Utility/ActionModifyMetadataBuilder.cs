using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ActionModifyMetadataBuilder : ContentModifyMetadataBuilder<ActionModifyMetadataBuilder, ActionSymbol>
{
	protected override string ContentElementName { get; } = "ActionChange";

	protected override string GetSemanticKind(ActionSymbol symbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(symbol);
	}

	protected override string GetTargetType(ActionSymbol symbol)
	{
		if (!symbol.ActionKind.IsGroup())
		{
			return "ActionDefinition";
		}
		return "ActionGroupDefinition";
	}
}
