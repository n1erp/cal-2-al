using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundParenthesizedFilterExpression : BoundPropertyValueExpression
{
	public BoundPropertyValueExpression Expression { get; private set; }

	public BoundParenthesizedFilterExpression(SyntaxNode syntax, BoundPropertyValueExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ParenthesizedFilterExpression, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParenthesizedFilterExpression(this);
	}

	public BoundParenthesizedFilterExpression Update(BoundPropertyValueExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundParenthesizedFilterExpression(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
