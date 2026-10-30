using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

public static class SymbolDisplay
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
		format = format ?? SymbolDisplayFormat.CSharpErrorMessageFormat;
		return ToDisplayParts(symbol, null, -1, format, minimal: false);
	}

	public static ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(ISymbol symbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		format = format ?? SymbolDisplayFormat.MinimallyQualifiedFormat;
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
				throw new ArgumentException(CSharpResources.SyntaxTreeSemanticModelMust);
			}
			if (positionOpt < 0 || positionOpt > semanticModelOpt.SyntaxTree.Length)
			{
				throw new ArgumentOutOfRangeException(CSharpResources.PositionNotWithinTree);
			}
		}
		ArrayBuilder<SymbolDisplayPart> instance = ArrayBuilder<SymbolDisplayPart>.GetInstance();
		SymbolDisplayVisitor visitor = new SymbolDisplayVisitor(instance, format, semanticModelOpt, positionOpt);
		symbol.Accept(visitor);
		return instance.ToImmutableAndFree();
	}

	public static string FormatPrimitive(object obj, bool quoteStrings, bool useHexadecimalNumbers)
	{
		ObjectDisplayOptions objectDisplayOptions = ObjectDisplayOptions.EscapeNonPrintableCharacters;
		if (quoteStrings)
		{
			objectDisplayOptions |= ObjectDisplayOptions.UseQuotes;
		}
		if (useHexadecimalNumbers)
		{
			objectDisplayOptions |= ObjectDisplayOptions.UseHexadecimalNumbers;
		}
		return ObjectDisplay.FormatPrimitive(obj, objectDisplayOptions);
	}

	public static string FormatLiteral(string value, bool quote)
	{
		ObjectDisplayOptions options = (ObjectDisplayOptions)(0x10 | (quote ? 8 : 0));
		return ObjectDisplay.FormatLiteral(value, options);
	}

	public static string FormatLiteral(char c, bool quote)
	{
		ObjectDisplayOptions options = (ObjectDisplayOptions)(0x10 | (quote ? 8 : 0));
		return ObjectDisplay.FormatLiteral(c, options);
	}
}
