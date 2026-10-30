using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAssignmentOperator : BoundExpression
{
	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public bool IsRef { get; }

	public BoundAssignmentOperator(SyntaxNode syntax, BoundExpression left, BoundExpression right, TypeSymbol type, bool isRef = false, bool hasErrors = false)
		: this(syntax, left, right, isRef, type, hasErrors)
	{
	}

	public BoundAssignmentOperator(SyntaxNode syntax, BoundExpression left, BoundExpression right, bool isRef, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AssignmentOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Left = left;
		Right = right;
		IsRef = isRef;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAssignmentOperator(this);
	}

	public BoundAssignmentOperator Update(BoundExpression left, BoundExpression right, bool isRef, TypeSymbol type)
	{
		if (left != Left || right != Right || isRef != IsRef || type != Type)
		{
			return new BoundAssignmentOperator(Syntax, left, right, isRef, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
