using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundTreeWalker : BoundTreeVisitor
{
	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		VisitList(node.ChildBoundNodes);
		return null;
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		return null;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		return null;
	}

	public override BoundNode VisitGlobal(BoundGlobal node)
	{
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		return null;
	}

	public override BoundNode VisitReturnValue(BoundReturnValue node)
	{
		return null;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		Visit(node.Operand);
		Visit(node.TargetOpt);
		return null;
	}

	public override BoundNode VisitClassType(BoundClassType node)
	{
		return null;
	}

	public override BoundNode VisitEnumType(BoundEnumType node)
	{
		return null;
	}

	public override BoundNode VisitSystemOptionType(BoundSystemOptionType node)
	{
		return null;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitControlAccess(BoundControlAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitQueryElementAccess(BoundQueryElementAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitQueryDataItemAccess(BoundQueryDataItemAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitReportDataItemAccess(BoundReportDataItemAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitXmlPortNodeAccess(BoundXmlPortNodeAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTestFieldAccess(BoundTestFieldAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTestPartAccess(BoundTestPartAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTestFilterAccess(BoundTestFilterAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTestFilterFieldAccess(BoundTestFilterFieldAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTestActionAccess(BoundTestActionAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitOptionAccess(BoundOptionAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitApplicationObjectAccess(BoundApplicationObjectAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitObjectAccess(BoundObjectAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		Visit(node.Expression);
		VisitList(node.Indices);
		return null;
	}

	public override BoundNode VisitTextIndexAccess(BoundTextIndexAccess node)
	{
		Visit(node.Expression);
		Visit(node.Index);
		return null;
	}

	public override BoundNode VisitTableKeyAccess(BoundTableKeyAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitTableFieldGroupAccess(BoundTableFieldGroupAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitReportLabelAccess(BoundReportLabelAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		Visit(node.ReceiverOpt);
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitInListExpression(BoundInListExpression node)
	{
		Visit(node.Expression);
		VisitList(node.List);
		return null;
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitAssignmentStatement(BoundAssignmentStatement node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitCompoundAssignmentStatement(BoundCompoundAssignmentStatement node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		VisitList(node.ChildBoundNodes);
		return null;
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return null;
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		Visit(node.Condition);
		Visit(node.Consequence);
		Visit(node.AlternativeOpt);
		return null;
	}

	public override BoundNode VisitCaseStatement(BoundCaseStatement node)
	{
		Visit(node.Expression);
		VisitList(node.CaseLines);
		Visit(node.CaseElse);
		return null;
	}

	public override BoundNode VisitCaseLine(BoundCaseLine node)
	{
		VisitList(node.Expressions);
		Visit(node.Statement);
		return null;
	}

	public override BoundNode VisitExitStatement(BoundExitStatement node)
	{
		Visit(node.ReturnValue);
		return null;
	}

	public override BoundNode VisitAssertErrorStatement(BoundAssertErrorStatement node)
	{
		Visit(node.Statement);
		return null;
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		Visit(node.LoopVariable);
		Visit(node.InitialValue);
		Visit(node.EndValue);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		Visit(node.IterationVariable);
		Visit(node.Expression);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		Visit(node.Condition);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitRepeatStatement(BoundRepeatStatement node)
	{
		Visit(node.Body);
		Visit(node.Condition);
		return null;
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return null;
	}

	public override BoundNode VisitWithStatement(BoundWithStatement node)
	{
		Visit(node.WithId);
		Visit(node.Statement);
		return null;
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitMethodReference(BoundMethodReference node)
	{
		return null;
	}

	public override BoundNode VisitBadPropertyValue(BoundBadPropertyValue node)
	{
		Visit(node.BadPropertyValue);
		return null;
	}

	public override BoundNode VisitProperty(BoundProperty node)
	{
		Visit(node.PropertyName);
		Visit(node.PropertyValue);
		return null;
	}

	public override BoundNode VisitBadProperty(BoundBadProperty node)
	{
		Visit(node.BadProperty);
		Visit(node.PropertyName);
		Visit(node.PropertyValue);
		return null;
	}

	public override BoundNode VisitLiteralPropertyValue(BoundLiteralPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitIdentifierNamePropertyValue(BoundIdentifierNamePropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitMultilanguagePropertyValue(BoundMultilanguagePropertyValue node)
	{
		VisitList(node.Value);
		return null;
	}

	public override BoundNode VisitLabelPropertyValue(BoundLabelPropertyValue node)
	{
		Visit(node.BoundLabel);
		return null;
	}

	public override BoundNode VisitMultilanguageExpression(BoundMultilanguageExpression node)
	{
		Visit(node.LanguageId);
		return null;
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		return null;
	}

	public override BoundNode VisitPermissionPropertyValue(BoundPermissionPropertyValue node)
	{
		VisitList(node.Value);
		return null;
	}

	public override BoundNode VisitPermissionExpression(BoundPermissionExpression node)
	{
		Visit(node.Target);
		return null;
	}

	public override BoundNode VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitIdentifierListPropertyValue(BoundIdentifierListPropertyValue node)
	{
		VisitList(node.Value);
		return null;
	}

	public override BoundNode VisitConstantValueListPropertyValue(BoundConstantValueListPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitListOfConstantValuePairsPropertyValue(BoundListOfConstantValuePairsPropertyValue node)
	{
		VisitList(node.Value);
		return null;
	}

	public override BoundNode VisitPairOfConstantValues(BoundPairOfConstantValues node)
	{
		return null;
	}

	public override BoundNode VisitEnumPropertyValue(BoundEnumPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitOptionMembersPropertyValue(BoundOptionMembersPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitTableRelationPropertyValue(BoundTableRelationPropertyValue node)
	{
		Visit(node.TableRelationExpression);
		return null;
	}

	public override BoundNode VisitTableRelationExpression(BoundTableRelationExpression node)
	{
		Visit(node.IfExpression);
		Visit(node.FieldAccess);
		Visit(node.TableFilter);
		Visit(node.ElseExpression);
		return null;
	}

	public override BoundNode VisitTableRelationConditionExpression(BoundTableRelationConditionExpression node)
	{
		VisitList(node.Condition);
		return null;
	}

	public override BoundNode VisitCalculationFormulaPropertyValue(BoundCalculationFormulaPropertyValue node)
	{
		Visit(node.FieldOrTableAccess);
		Visit(node.TableFilter);
		return null;
	}

	public override BoundNode VisitEqualsPropertyExpression(BoundEqualsPropertyExpression node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitPropertyRhsExpression(BoundPropertyRhsExpression node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitWildCardPropertyValueExpression(BoundWildCardPropertyValueExpression node)
	{
		Visit(node.Pattern);
		return null;
	}

	public override BoundNode VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitCodeExpressionPropertyValue(BoundCodeExpressionPropertyValue node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitStyleExpressionPropertyValue(BoundStyleExpressionPropertyValue node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitDecimalPlacesPropertyValue(BoundDecimalPlacesPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitTableViewPropertyValue(BoundTableViewPropertyValue node)
	{
		VisitList(node.Sorting);
		Visit(node.TableFilter);
		return null;
	}

	public override BoundNode VisitTableFilterStatementPropertyValue(BoundTableFilterStatementPropertyValue node)
	{
		Visit(node.TableFilter);
		return null;
	}

	public override BoundNode VisitOrderByPropertyValue(BoundOrderByPropertyValue node)
	{
		VisitList(node.Elements);
		return null;
	}

	public override BoundNode VisitOrderByExpression(BoundOrderByExpression node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitDataItemLinkPropertyValue(BoundDataItemLinkPropertyValue node)
	{
		VisitList(node.Joins);
		return null;
	}

	public override BoundNode VisitDataItemLinkExpression(BoundDataItemLinkExpression node)
	{
		Visit(node.SourceField);
		Visit(node.RelatedField);
		return null;
	}

	public override BoundNode VisitNullPropertyValue(BoundNullPropertyValue node)
	{
		return null;
	}

	public override BoundNode VisitTemporary(BoundTemporary node)
	{
		return null;
	}

	public void VisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		if (!list.IsDefault)
		{
			for (int i = 0; i < list.Length; i++)
			{
				Visit(list[i]);
			}
		}
	}
}
