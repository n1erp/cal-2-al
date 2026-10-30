using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundUnaryPropertyFilterExpression : BoundPropertyValueExpression
{
	public UnaryOperatorKind OperatorKind { get; private set; }

	public BoundExpression Operand { get; private set; }

	public BoundUnaryPropertyFilterExpression(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.UnaryPropertyFilterExpression, syntax, type, hasErrors || operand.HasErrors())
	{
		OperatorKind = operatorKind;
		Operand = operand;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnaryPropertyFilterExpression(this);
	}

	public BoundUnaryPropertyFilterExpression Update(UnaryOperatorKind operatorKind, BoundExpression operand, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || operand != Operand || type != Type)
		{
			return new BoundUnaryPropertyFilterExpression(Syntax, operatorKind, operand, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
