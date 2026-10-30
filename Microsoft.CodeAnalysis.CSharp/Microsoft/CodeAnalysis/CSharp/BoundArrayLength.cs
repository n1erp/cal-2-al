using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayLength : BoundExpression
{
	public BoundExpression Expression { get; }

	public BoundArrayLength(SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArrayLength, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayLength(this);
	}

	public BoundArrayLength Update(BoundExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundArrayLength(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
