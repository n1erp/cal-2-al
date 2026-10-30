using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundThisReference : BoundExpression
{
	public BoundThisReference(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ThisReference, syntax, type, hasErrors)
	{
	}

	public BoundThisReference(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ThisReference, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitThisReference(this);
	}

	public BoundThisReference Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundThisReference(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
