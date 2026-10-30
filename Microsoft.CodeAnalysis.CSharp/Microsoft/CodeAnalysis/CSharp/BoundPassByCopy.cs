using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPassByCopy : BoundExpression
{
	public override ConstantValue ConstantValue => null;

	public override Symbol ExpressionSymbol => Expression.ExpressionSymbol;

	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Expression);

	public override object Display => Expression.Display;

	public BoundExpression Expression { get; }

	public BoundPassByCopy(SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PassByCopy, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPassByCopy(this);
	}

	public BoundPassByCopy Update(BoundExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundPassByCopy(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
