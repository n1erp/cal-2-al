using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIsPatternExpression : BoundExpression
{
	public BoundExpression Expression { get; }

	public BoundPattern Pattern { get; }

	public BoundIsPatternExpression(SyntaxNode syntax, BoundExpression expression, BoundPattern pattern, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IsPatternExpression, syntax, type, hasErrors || expression.HasErrors() || pattern.HasErrors())
	{
		Expression = expression;
		Pattern = pattern;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIsPatternExpression(this);
	}

	public BoundIsPatternExpression Update(BoundExpression expression, BoundPattern pattern, TypeSymbol type)
	{
		if (expression != Expression || pattern != Pattern || type != Type)
		{
			return new BoundIsPatternExpression(Syntax, expression, pattern, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
