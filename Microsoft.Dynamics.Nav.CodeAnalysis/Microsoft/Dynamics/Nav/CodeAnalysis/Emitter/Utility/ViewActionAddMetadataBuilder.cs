using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ViewActionAddMetadataBuilder(ViewSymbol anchorSymbol, SourceViewSymbol view) : ContentAddMetadataBuilder<ViewActionAddMetadataBuilder, ViewSymbol, SourceViewSymbol>(anchorSymbol, view)
{
	protected override string ContentElementName { get; } = "ActionAdd";

	protected override string GetSemanticKind(ViewSymbol symbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(symbol);
	}

	protected override bool IsArea(ViewSymbol group)
	{
		return false;
	}

	protected override string ContainerGetter(ViewSymbol anchor)
	{
		return "ViewActions";
	}

	protected override string GetAnchorName(ViewSymbol symbol)
	{
		if (symbol == null)
		{
			return null;
		}
		return CompilationUtilities.CreateEscapedViewName(symbol.ContainingModule.AppId, symbol.Name);
	}
}
