using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDup : BoundExpression
{
	public RefKind RefKind { get; }

	public BoundDup(SyntaxNode syntax, RefKind refKind, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Dup, syntax, type, hasErrors)
	{
		RefKind = refKind;
	}

	public BoundDup(SyntaxNode syntax, RefKind refKind, TypeSymbol type)
		: base(BoundKind.Dup, syntax, type)
	{
		RefKind = refKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDup(this);
	}

	public BoundDup Update(RefKind refKind, TypeSymbol type)
	{
		if (refKind != RefKind || type != Type)
		{
			return new BoundDup(Syntax, refKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
