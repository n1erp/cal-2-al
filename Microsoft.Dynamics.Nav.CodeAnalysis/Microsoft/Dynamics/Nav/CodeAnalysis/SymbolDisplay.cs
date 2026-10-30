using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class SymbolDisplay
{
	public static string ToDisplayString(ISymbol symbol, SymbolDisplayFormat format = null)
	{
		return ToDisplayParts(symbol, format).ToDisplayString();
	}

	public static string ToMinimalDisplayString(ISymbol symbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return ToMinimalDisplayParts(symbol, semanticModel, position, format).ToDisplayString();
	}

	public static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null)
	{
		format = format ?? SymbolDisplayFormat.ErrorMessageFormat;
		return ToDisplayParts(symbol, null, -1, format, minimal: false);
	}

	public static ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(ISymbol symbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		format = format ?? SymbolDisplayFormat.ShortFormat;
		return ToDisplayParts(symbol, semanticModel, position, format, minimal: true);
	}

	private static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SemanticModel semanticModelOpt, int positionOpt, SymbolDisplayFormat format, bool minimal)
	{
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if (minimal)
		{
			if (semanticModelOpt == null)
			{
				throw new ArgumentNullException("semanticModelOpt");
			}
			if (positionOpt < 0 || positionOpt > semanticModelOpt.SyntaxTree.Length)
			{
				throw new ArgumentOutOfRangeException(CodeAnalysisResources.PositionNotWithinTree);
			}
		}
		else
		{
			DebugAssertHelper.Assert(semanticModelOpt == null);
			DebugAssertHelper.Assert(positionOpt < 0);
		}
		ArrayBuilder<SymbolDisplayPart> instance = ArrayBuilder<SymbolDisplayPart>.GetInstance();
		SymbolDisplayVisitor visitor = new SymbolDisplayVisitor(instance, format, semanticModelOpt, positionOpt);
		((Symbol)symbol).Accept(visitor);
		return instance.ToImmutableAndFree();
	}
}
