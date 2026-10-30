using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundAssignmentStatement : BoundStatement, IAssignmentStatement, IOperation
{
	public BoundExpression Left { get; private set; }

	public BoundExpression Right { get; private set; }

	IOperation IAssignmentStatement.Target => Left;

	IOperation IAssignmentStatement.Value => Right;

	protected override OperationKind StatementKind => OperationKind.AssignmentStatement;

	public BoundAssignmentStatement(SyntaxNode syntax, BoundExpression left, BoundExpression right, bool hasErrors = false)
		: base(BoundKind.AssignmentStatement, syntax, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Left = left;
		Right = right;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAssignmentStatement(this);
	}

	public BoundAssignmentStatement Update(BoundExpression left, BoundExpression right)
	{
		if (left != Left || right != Right)
		{
			return new BoundAssignmentStatement(Syntax, left, right, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAssignmentStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAssignmentStatement(this, argument);
	}
}
