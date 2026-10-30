using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayAccess : BoundExpression
{
	public BoundExpression Expression { get; }

	public ImmutableArray<BoundExpression> Indices { get; }

	public BoundArrayAccess(SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundExpression> indices, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArrayAccess, syntax, type, hasErrors || expression.HasErrors() || indices.HasErrors())
	{
		Expression = expression;
		Indices = indices;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayAccess(this);
	}

	public BoundArrayAccess Update(BoundExpression expression, ImmutableArray<BoundExpression> indices, TypeSymbol type)
	{
		if (expression != Expression || indices != Indices || type != Type)
		{
			return new BoundArrayAccess(Syntax, expression, indices, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
