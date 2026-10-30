using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArgList : BoundExpression
{
	public BoundArgList(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ArgList, syntax, type, hasErrors)
	{
	}

	public BoundArgList(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ArgList, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArgList(this);
	}

	public BoundArgList Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundArgList(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
