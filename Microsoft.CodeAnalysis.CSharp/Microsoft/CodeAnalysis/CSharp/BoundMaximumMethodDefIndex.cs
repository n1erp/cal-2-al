using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundMaximumMethodDefIndex : BoundExpression
{
	public BoundMaximumMethodDefIndex(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.MaximumMethodDefIndex, syntax, type, hasErrors)
	{
	}

	public BoundMaximumMethodDefIndex(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.MaximumMethodDefIndex, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMaximumMethodDefIndex(this);
	}

	public BoundMaximumMethodDefIndex Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundMaximumMethodDefIndex(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
