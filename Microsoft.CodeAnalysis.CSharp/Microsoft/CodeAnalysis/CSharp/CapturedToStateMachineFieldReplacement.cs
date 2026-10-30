using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CapturedToStateMachineFieldReplacement : CapturedSymbolReplacement
{
	public readonly StateMachineFieldSymbol HoistedField;

	public CapturedToStateMachineFieldReplacement(StateMachineFieldSymbol hoistedField, bool isReusable)
		: base(isReusable)
	{
		HoistedField = hoistedField;
	}

	public override BoundExpression Replacement(SyntaxNode node, Func<NamedTypeSymbol, BoundExpression> makeFrame)
	{
		BoundExpression boundExpression = makeFrame(HoistedField.ContainingType);
		FieldSymbol fieldSymbol = HoistedField.AsMember((NamedTypeSymbol)boundExpression.Type);
		return new BoundFieldAccess(node, boundExpression, fieldSymbol, null);
	}
}
