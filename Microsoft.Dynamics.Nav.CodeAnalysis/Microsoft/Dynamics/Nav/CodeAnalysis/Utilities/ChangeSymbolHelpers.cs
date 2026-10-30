using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public static class ChangeSymbolHelpers
{
	public static ImmutableArray<IChangeSymbolInformation> GetTargetSymbolAndItsParentAndChangeSymbolsTuple(ImmutableArray<IChangeSymbol> moveSymbols, ImmutableArray<IChangeSymbol> addSymbols)
	{
		ImmutableArray<SourceChangeMoveSymbol> moveSymbols2 = moveSymbols.AsImmutable<IChangeSymbol, SourceChangeMoveSymbol>();
		ImmutableArray<SourceChangeAddSymbol> addSymbols2 = addSymbols.AsImmutable<IChangeSymbol, SourceChangeAddSymbol>();
		return ImmutableArray<IChangeSymbolInformation>.CastUp(GetTargetSymbolAndItsParentAndChangeSymbolsTuple(moveSymbols2, addSymbols2));
	}

	internal static ImmutableArray<ChangeSymbolInformation> GetTargetSymbolAndItsParentAndChangeSymbolsTuple(ImmutableArray<SourceChangeMoveSymbol> moveSymbols, ImmutableArray<SourceChangeAddSymbol> addSymbols)
	{
		ArrayBuilder<ChangeSymbolInformation> instance = ArrayBuilder<ChangeSymbolInformation>.GetInstance();
		try
		{
			foreach (SourceChangeMoveSymbol item in moveSymbols)
			{
				Symbol parentSymbol = GetParentSymbol(item);
				foreach (Symbol movedSymbol in item.MovedSymbols)
				{
					instance.Add(new ChangeSymbolInformation(movedSymbol, parentSymbol, item));
				}
			}
			foreach (SourceChangeAddSymbol item2 in addSymbols)
			{
				Symbol parentSymbol2 = GetParentSymbol(item2);
				foreach (Symbol addedSymbol in item2.AddedSymbols)
				{
					instance.Add(new ChangeSymbolInformation(addedSymbol, parentSymbol2, item2));
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static SyntaxNode GetMovedSymbolSyntax(ChangeMoveSymbol moveSymbol, Symbol movedSymbol)
	{
		return moveSymbol.ChangeTargetKind switch
		{
			SymbolKind.Control => (SyntaxNode)((SourceChangeMoveControlSymbol)moveSymbol).Syntax.Controls.FirstOrDefault((IdentifierNameSyntax s) => SemanticFacts.IsSameName(s.GetIdentifierOrLiteralValue(), movedSymbol.Name)), 
			SymbolKind.Action => ((SourceChangeMoveActionSymbol)moveSymbol).Syntax.Actions.FirstOrDefault((IdentifierNameSyntax s) => SemanticFacts.IsSameName(s.GetIdentifierOrLiteralValue(), movedSymbol.Name)), 
			SymbolKind.View => ((SourceChangeMoveViewSymbol)moveSymbol).Syntax.Views.FirstOrDefault((IdentifierNameSyntax s) => SemanticFacts.IsSameName(s.GetIdentifierOrLiteralValue(), movedSymbol.Name)), 
			_ => throw ExceptionUtilities.UnexpectedValue(moveSymbol.ChangeTargetKind), 
		};
	}

	private static Symbol GetParentSymbol(ChangeMoveSymbol changeSymbol)
	{
		if (changeSymbol.ChangeKind.IsRelativeToAnchor())
		{
			return changeSymbol.Anchor?.ContainingSymbol;
		}
		return changeSymbol.Anchor;
	}

	private static Symbol GetParentSymbol(SourceChangeAddSymbol changeSymbol)
	{
		if (changeSymbol.ChangeKind.IsRelativeToAnchor())
		{
			return changeSymbol.Anchor?.ContainingSymbol;
		}
		return changeSymbol.Anchor;
	}
}
