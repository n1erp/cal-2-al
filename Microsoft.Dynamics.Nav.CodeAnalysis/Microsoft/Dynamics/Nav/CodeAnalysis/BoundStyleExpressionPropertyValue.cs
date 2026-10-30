using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundStyleExpressionPropertyValue : BoundCodeExpressionPropertyValue
{
	public BoundStyleExpressionPropertyValue(SyntaxNode syntax, BoundExpression expression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.StyleExpressionPropertyValue, syntax, expression, type, hasErrors || expression.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStyleExpressionPropertyValue(this);
	}

	public new BoundStyleExpressionPropertyValue Update(BoundExpression expression, TypeSymbol type)
	{
		if (expression != Expression || type != Type)
		{
			return new BoundStyleExpressionPropertyValue(Syntax, expression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
