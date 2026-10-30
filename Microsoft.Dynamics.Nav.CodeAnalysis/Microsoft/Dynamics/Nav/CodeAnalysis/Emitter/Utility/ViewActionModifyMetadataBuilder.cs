using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ViewActionModifyMetadataBuilder : ContentModifyMetadataBuilder<ViewActionModifyMetadataBuilder, ViewSymbol>
{
	protected override string ContentElementName { get; } = "ActionChange";

	protected override string GetSemanticKind(ViewSymbol symbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(symbol);
	}

	protected override string GetTargetType(ViewSymbol symbol)
	{
		return "ActionDefinition";
	}

	protected override string GetTargetName(ViewSymbol symbol)
	{
		return CompilationUtilities.CreateEscapedViewName(symbol.ContainingModule.AppId, symbol.Name);
	}
}
