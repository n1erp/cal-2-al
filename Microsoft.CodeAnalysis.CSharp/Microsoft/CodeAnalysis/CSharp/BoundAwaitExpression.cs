using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAwaitExpression : BoundExpression
{
	internal bool IsDynamic => (object)GetResult == null;

	public BoundExpression Expression { get; }

	public MethodSymbol GetAwaiter { get; }

	public PropertySymbol IsCompleted { get; }

	public MethodSymbol GetResult { get; }

	public BoundAwaitExpression(SyntaxNode syntax, BoundExpression expression, MethodSymbol getAwaiter, PropertySymbol isCompleted, MethodSymbol getResult, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AwaitExpression, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
		GetAwaiter = getAwaiter;
		IsCompleted = isCompleted;
		GetResult = getResult;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAwaitExpression(this);
	}

	public BoundAwaitExpression Update(BoundExpression expression, MethodSymbol getAwaiter, PropertySymbol isCompleted, MethodSymbol getResult, TypeSymbol type)
	{
		if (expression != Expression || getAwaiter != GetAwaiter || isCompleted != IsCompleted || getResult != GetResult || type != Type)
		{
			return new BoundAwaitExpression(Syntax, expression, getAwaiter, isCompleted, getResult, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
