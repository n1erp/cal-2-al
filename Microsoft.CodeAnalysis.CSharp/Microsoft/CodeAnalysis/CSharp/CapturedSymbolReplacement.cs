using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CapturedSymbolReplacement
{
	public readonly bool IsReusable;

	public CapturedSymbolReplacement(bool isReusable)
	{
		IsReusable = isReusable;
	}

	public abstract BoundExpression Replacement(SyntaxNode node, Func<NamedTypeSymbol, BoundExpression> makeFrame);
}
