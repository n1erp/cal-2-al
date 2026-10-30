using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public abstract class OperationVisitor
{
	public virtual void Visit(IOperation operation)
	{
		operation?.Accept(this);
	}

	public virtual void DefaultVisit(IOperation operation)
	{
	}

	public virtual void VisitStatementList(IStatementList operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitBlockStatement(IBlockStatement operation)
	{
		DefaultVisit(operation);
	}

	internal virtual void VisitNoneOperation(IOperation operation)
	{
	}

	public virtual void VisitIfStatement(IIfStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitCaseStatement(ICaseStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitCaseLine(ICaseLineOperation operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitEmptyStatement(IEmptyStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitExitStatement(IExitStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitAssertErrorStatement(IAssertErrorStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitBreakStatement(IBreakStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitForLoopStatement(IForLoopStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitForEachLoopStatement(IForEachLoopStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitWhileRepeatLoopStatement(IWhileRepeatLoopStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitWithStatement(IWithStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitExpressionStatement(IExpressionStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitAssignmentStatement(IAssignmentStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitCompoundAssignmentStatement(ICompoundAssignmentStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitInvocationExpression(IInvocationExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitArgument(IArgument operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitLocalReferenceExpression(ILocalReferenceExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitGlobalReferenceExpression(IGlobalReferenceExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitReturnValueReferenceExpression(IReturnValueReferenceExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitParameterReferenceExpression(IParameterReferenceExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitLiteralExpression(ILiteralExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitParenthesizedExpression(IParenthesizedExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitFieldAccess(IFieldAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTextIndexAccess(ITextIndexAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitArrayElementReferenceExpression(IArrayElementReferenceExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitInListExpression(IInListExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitApplicationObjectAccess(IApplicationObjectAccess operation)
	{
		DefaultVisit(operation);
	}

	internal virtual void VisitObjectAccess(IObjectAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitUnaryOperatorExpression(IUnaryOperatorExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitBinaryOperatorExpression(IBinaryOperatorExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitOptionAccess(IOptionAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTestFieldAccess(ITestFieldAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTestPartAccess(ITestPartAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTestActionAccess(ITestActionAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTestFilterAccess(ITestFilterAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTestFilterFieldAccess(ITestFilterFieldAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitConversionExpression(IConversionExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitInvalidStatement(IInvalidStatement operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitInvalidExpression(IInvalidExpression operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitPagePartAccess(IPagePartAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitQueryColumnAccess(IQueryElementAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTableKeyAccess(ITableKeyAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitTableFieldGroupAccess(ITableFieldGroupAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitReportLabelAccess(IReportLabelAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitReportDataItemAccess(IReportDataItemAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitQueryDataItemAccess(IQueryDataItemAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitXmlPortNodeAccess(IXmlPortNodeAccess operation)
	{
		DefaultVisit(operation);
	}

	public virtual void VisitEnumTypeAccess(IOperation operation)
	{
		DefaultVisit(operation);
	}
}
public abstract class OperationVisitor<TArgument, TResult>
{
	public virtual TResult Visit(IOperation operation, TArgument argument)
	{
		if (operation != null)
		{
			return operation.Accept(this, argument);
		}
		return default;
	}

	public virtual TResult DefaultVisit(IOperation operation, TArgument argument)
	{
		return default;
	}

	public virtual TResult VisitStatementList(IStatementList operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitBlockStatement(IBlockStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	internal virtual TResult VisitNoneOperation(IOperation operation, TArgument argument)
	{
		return default;
	}

	public virtual TResult VisitIfStatement(IIfStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitCaseStatement(ICaseStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitCaseLine(ICaseLineOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitEmptyStatement(IEmptyStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitExitStatement(IExitStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitAssertErrorStatement(IAssertErrorStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitBreakStatement(IBreakStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitForLoopStatement(IForLoopStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitForEachLoopStatement(IForEachLoopStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitWhileRepeatLoopStatement(IWhileRepeatLoopStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitWithStatement(IWithStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitExpressionStatement(IExpressionStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitAssignmentStatement(IAssignmentStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitCompoundAssignmentStatement(ICompoundAssignmentStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitInvocationExpression(IInvocationExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitArgument(IArgument operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitLocalReferenceExpression(ILocalReferenceExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitGlobalReferenceExpression(IGlobalReferenceExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitReturnValueReferenceExpression(IReturnValueReferenceExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitParameterReferenceExpression(IParameterReferenceExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitLiteralExpression(ILiteralExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitParenthesizedExpression(IParenthesizedExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitFieldAccess(IFieldAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTextIndexAccess(ITextIndexAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitArrayElementReferenceExpression(IArrayElementReferenceExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitInListExpression(IInListExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitApplicationObjectAccess(IApplicationObjectAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	internal virtual TResult VisitObjectAccess(IObjectAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitUnaryOperatorExpression(IUnaryOperatorExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitBinaryOperatorExpression(IBinaryOperatorExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitOptionAccess(IOptionAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTestFieldAccess(ITestFieldAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTestPartAccess(ITestPartAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTestActionAccess(ITestActionAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTestFilterAccess(ITestFilterAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTestFilterFieldAccess(ITestFilterFieldAccess operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitConversionExpression(IConversionExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitInvalidStatement(IInvalidStatement operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitInvalidExpression(IInvalidExpression operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitPagePartAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitXmlPortDataItemAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitQueryElementAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTableKeyAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitTableFieldGroupAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitReportLabelAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitReportDataItemAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitQueryDataItemAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitXmlPortNodeAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}

	public virtual TResult VisitEnumTypeAccess(IOperation operation, TArgument argument)
	{
		return DefaultVisit(operation, argument);
	}
}
