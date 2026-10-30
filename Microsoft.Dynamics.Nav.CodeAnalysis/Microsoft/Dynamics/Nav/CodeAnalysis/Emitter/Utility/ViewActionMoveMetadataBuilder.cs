using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ViewActionMoveMetadataBuilder(ViewSymbol anchorSymbol, ViewSymbol view) : ContentMoveMetadataBuilder<ViewActionMoveMetadataBuilder, ViewSymbol>(anchorSymbol, view)
{
	protected override string ContentElementName { get; } = "ActionMove";

	protected override string TargetType { get; } = "ActionBaseDefinition";

	protected override string ContainerGetter(ViewSymbol anchor)
	{
		return "ViewActions";
	}

	protected override bool IsArea(ViewSymbol group)
	{
		return false;
	}

	protected override string GetTargetName(ViewSymbol symbol)
	{
		return CompilationUtilities.CreateEscapedViewName(symbol.ContainingModule.AppId, symbol.Name);
	}

	protected override string GetAnchorName(ViewSymbol symbol)
	{
		if (symbol == null)
		{
			return null;
		}
		return CompilationUtilities.CreateEscapedViewName(symbol.ContainingModule.AppId, symbol.Name);
	}

	protected override string GetSemanticKind(ViewSymbol symbol)
	{
		return MetadataEmitterHelper.GetSemanticKind(symbol);
	}
}
