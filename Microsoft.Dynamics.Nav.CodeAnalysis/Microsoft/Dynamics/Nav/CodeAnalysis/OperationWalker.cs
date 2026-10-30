using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public abstract class OperationWalker : OperationVisitor
{
	private int recursionDepth;

	public override void Visit(IOperation operation)
	{
		if (operation != null)
		{
			recursionDepth++;
			try
			{
				StackGuard.EnsureSufficientExecutionStack(recursionDepth);
				operation.Accept(this);
			}
			finally
			{
				recursionDepth--;
			}
		}
	}

	internal void VisitArray<T>(ImmutableArray<T> list) where T : IOperation
	{
		if (!list.IsDefault)
		{
			foreach (T item in list)
			{
				Visit(item);
			}
		}
	}

	public override void VisitBlockStatement(IBlockStatement operation)
	{
		VisitArray(operation.Statements);
	}

	public override void VisitApplicationObjectAccess(IApplicationObjectAccess operation)
	{
		Visit(operation.Instance);
	}

	public override void VisitArgument(IArgument operation)
	{
		Visit(operation.Value);
	}

	public override void VisitArrayElementReferenceExpression(IArrayElementReferenceExpression operation)
	{
		Visit(operation.ArrayReference);
		VisitArray(operation.Indexes);
	}

	public override void VisitAssertErrorStatement(IAssertErrorStatement operation)
	{
		Visit(operation.Statement);
	}

	public override void VisitAssignmentStatement(IAssignmentStatement operation)
	{
		Visit(operation.Target);
		Visit(operation.Value);
	}

	public override void VisitBinaryOperatorExpression(IBinaryOperatorExpression operation)
	{
		Visit(operation.LeftOperand);
		Visit(operation.RightOperand);
	}

	public override void VisitCaseStatement(ICaseStatement operation)
	{
		Visit(operation.Value);
		VisitArray(operation.CaseLines);
		Visit(operation.ElseStatement);
	}

	public override void VisitCaseLine(ICaseLineOperation operation)
	{
		VisitArray(operation.Expressions);
		Visit(operation.Statement);
	}

	public override void VisitConversionExpression(IConversionExpression operation)
	{
		Visit(operation.Operand);
	}

	public override void VisitCompoundAssignmentStatement(ICompoundAssignmentStatement operation)
	{
		Visit(operation.Target);
		Visit(operation.Value);
	}

	public override void VisitExitStatement(IExitStatement operation)
	{
		Visit(operation.ReturnedValue);
	}

	public override void VisitExpressionStatement(IExpressionStatement operation)
	{
		Visit(operation.Expression);
	}

	public override void VisitFieldAccess(IFieldAccess operation)
	{
		Visit(operation.Instance);
	}

	public override void VisitForLoopStatement(IForLoopStatement operation)
	{
		Visit(operation.LoopVariable);
		Visit(operation.InitialValue);
		Visit(operation.EndValue);
		Visit(operation.Body);
	}

	public override void VisitForEachLoopStatement(IForEachLoopStatement operation)
	{
		Visit(operation.IterationVariable);
		Visit(operation.Expression);
		Visit(operation.Body);
	}

	public override void VisitIfStatement(IIfStatement operation)
	{
		Visit(operation.Condition);
		Visit(operation.IfTrueStatement);
		Visit(operation.IfFalseStatement);
	}

	public override void VisitInListExpression(IInListExpression operation)
	{
		Visit(operation.Value);
		VisitArray(operation.List);
	}

	public override void VisitInvocationExpression(IInvocationExpression operation)
	{
		Visit(operation.Instance);
		VisitArray(operation.Arguments);
	}

	public override void VisitOptionAccess(IOptionAccess operation)
	{
		Visit(operation.Instance);
	}

	public override void VisitParenthesizedExpression(IParenthesizedExpression operation)
	{
		Visit(operation.Operand);
	}

	public override void VisitStatementList(IStatementList operation)
	{
		VisitArray(operation.Statements);
	}

	public override void VisitTestActionAccess(ITestActionAccess operation)
	{
		Visit(operation.Instance);
	}

	public override void VisitTestFieldAccess(ITestFieldAccess operation)
	{
		Visit(operation.Instance);
	}

	public override void VisitTextIndexAccess(ITextIndexAccess operation)
	{
		Visit(operation.TextExpression);
		Visit(operation.Index);
	}

	public override void VisitUnaryOperatorExpression(IUnaryOperatorExpression operation)
	{
		Visit(operation.Operand);
	}

	public override void VisitWhileRepeatLoopStatement(IWhileRepeatLoopStatement operation)
	{
		Visit(operation.Condition);
		Visit(operation.Body);
	}

	public override void VisitWithStatement(IWithStatement operation)
	{
		Visit(operation.Value);
		Visit(operation.Body);
	}
}
