using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CapturedToExpressionSymbolReplacement : CapturedSymbolReplacement
{
	private readonly BoundExpression _replacement;

	public readonly ImmutableArray<StateMachineFieldSymbol> HoistedFields;

	public CapturedToExpressionSymbolReplacement(BoundExpression replacement, ImmutableArray<StateMachineFieldSymbol> hoistedFields, bool isReusable)
		: base(isReusable)
	{
		_replacement = replacement;
		HoistedFields = hoistedFields;
	}

	public override BoundExpression Replacement(SyntaxNode node, Func<NamedTypeSymbol, BoundExpression> makeFrame)
	{
		return _replacement;
	}
}
