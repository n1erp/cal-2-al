using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundThrowExpression : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Expression);

	public override object Display => MessageID.IDS_ThrowExpression.Localize();

	public BoundExpression Expression { get; }

	public BoundThrowExpression(SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ThrowExpression, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitThrowExpression(this);
	}

	public BoundThrowExpression Update(BoundExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundThrowExpression(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
