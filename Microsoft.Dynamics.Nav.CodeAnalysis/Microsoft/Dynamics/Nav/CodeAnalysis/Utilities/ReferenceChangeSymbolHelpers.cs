using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ReferenceChangeSymbolHelpers
{
	internal static Symbol GetAnchorFromPageExtension(string anchorName, SymbolKind changeTargetKind, IApplicationObjectExtensionTypeSymbol extension)
	{
		foreach (ISymbol member in extension.GetMembers())
		{
			if (member.Kind != SymbolKind.Change)
			{
				continue;
			}
			ReferenceChangeAddControlSymbol referenceChangeAddControlSymbol = member as ReferenceChangeAddControlSymbol;
			if (!(referenceChangeAddControlSymbol != null) || referenceChangeAddControlSymbol.ChangeTargetKind != changeTargetKind)
			{
				continue;
			}
			foreach (Symbol addedSymbol in referenceChangeAddControlSymbol.AddedSymbols)
			{
				if (SemanticFacts.IsSameName(addedSymbol.Name, anchorName))
				{
					return referenceChangeAddControlSymbol.Anchor;
				}
			}
		}
		return null;
	}
}
