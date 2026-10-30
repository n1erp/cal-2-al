using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundHoistedFieldAccess : BoundExpression
{
	public FieldSymbol FieldSymbol { get; }

	public BoundHoistedFieldAccess(SyntaxNode syntax, FieldSymbol fieldSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.HoistedFieldAccess, syntax, type, hasErrors)
	{
		FieldSymbol = fieldSymbol;
	}

	public BoundHoistedFieldAccess(SyntaxNode syntax, FieldSymbol fieldSymbol, TypeSymbol type)
		: base(BoundKind.HoistedFieldAccess, syntax, type)
	{
		FieldSymbol = fieldSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitHoistedFieldAccess(this);
	}

	public BoundHoistedFieldAccess Update(FieldSymbol fieldSymbol, TypeSymbol type)
	{
		if (fieldSymbol != FieldSymbol || type != Type)
		{
			return new BoundHoistedFieldAccess(Syntax, fieldSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
