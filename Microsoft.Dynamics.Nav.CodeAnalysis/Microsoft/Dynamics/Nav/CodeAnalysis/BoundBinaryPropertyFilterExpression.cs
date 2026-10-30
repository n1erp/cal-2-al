using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBinaryPropertyFilterExpression : BoundPropertyValueExpression
{
	public BinaryOperatorKind OperatorKind { get; private set; }

	public BoundExpression Left { get; private set; }

	public BoundExpression Right { get; private set; }

	public BoundBinaryPropertyFilterExpression(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.BinaryPropertyFilterExpression, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		OperatorKind = operatorKind;
		Left = left;
		Right = right;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBinaryPropertyFilterExpression(this);
	}

	public BoundBinaryPropertyFilterExpression Update(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || left != Left || right != Right || type != Type)
		{
			return new BoundBinaryPropertyFilterExpression(Syntax, operatorKind, left, right, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
