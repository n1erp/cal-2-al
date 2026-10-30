using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CapturedToFrameSymbolReplacement : CapturedSymbolReplacement
{
	public readonly LambdaCapturedVariable HoistedField;

	public CapturedToFrameSymbolReplacement(LambdaCapturedVariable hoistedField, bool isReusable)
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
