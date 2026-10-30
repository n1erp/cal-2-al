using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BoundCodeExpressionPropertyValue : BoundPropertyValue
{
	public BoundExpression Expression { get; private set; }

	internal override object ValueObject => Expression;

	internal override string ValueText => Syntax.ToString();

	protected BoundCodeExpressionPropertyValue(BoundKind kind, SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Expression = expression;
	}

	public BoundCodeExpressionPropertyValue(SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CodeExpressionPropertyValue, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCodeExpressionPropertyValue(this);
	}

	public BoundCodeExpressionPropertyValue Update(BoundExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundCodeExpressionPropertyValue(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.Visit(Expression);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.Visit(Expression, argument);
	}
}
