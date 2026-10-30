using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundCompoundAssignmentStatement : BoundStatement, ICompoundAssignmentStatement, IAssignmentStatement, IOperation
{
	public CompoundAssignmentOperatorKind Operator { get; private set; }

	public BoundExpression Left { get; private set; }

	public BoundExpression Right { get; private set; }

	IOperation IAssignmentStatement.Target => Left;

	IOperation IAssignmentStatement.Value => Right;

	CompoundAssignmentKind ICompoundAssignmentStatement.CompoundAssignmentKind => CompoundAssignmentKind;

	protected override OperationKind StatementKind => OperationKind.CompoundAssignmentStatement;

	private CompoundAssignmentKind CompoundAssignmentKind => (Operator & CompoundAssignmentOperatorKind.OpMask) switch
	{
		CompoundAssignmentOperatorKind.Addition => CompoundAssignmentKind.Addition, 
		CompoundAssignmentOperatorKind.Division => CompoundAssignmentKind.Division, 
		CompoundAssignmentOperatorKind.IDivision => CompoundAssignmentKind.IDivision, 
		CompoundAssignmentOperatorKind.Multiplication => CompoundAssignmentKind.Multiplication, 
		CompoundAssignmentOperatorKind.Remainder => CompoundAssignmentKind.Remainder, 
		CompoundAssignmentOperatorKind.Subtraction => CompoundAssignmentKind.Subtraction, 
		_ => throw ExceptionUtilities.UnexpectedValue(Operator & CompoundAssignmentOperatorKind.OpMask), 
	};

	public BoundCompoundAssignmentStatement(SyntaxNode syntax, CompoundAssignmentOperatorKind @operator, BoundExpression left, BoundExpression right, bool hasErrors = false)
		: base(BoundKind.CompoundAssignmentStatement, syntax, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Operator = @operator;
		Left = left;
		Right = right;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCompoundAssignmentStatement(this);
	}

	public BoundCompoundAssignmentStatement Update(CompoundAssignmentOperatorKind @operator, BoundExpression left, BoundExpression right)
	{
		if (@operator != Operator || left != Left || right != Right)
		{
			return new BoundCompoundAssignmentStatement(Syntax, @operator, left, right, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCompoundAssignmentStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCompoundAssignmentStatement(this, argument);
	}
}
