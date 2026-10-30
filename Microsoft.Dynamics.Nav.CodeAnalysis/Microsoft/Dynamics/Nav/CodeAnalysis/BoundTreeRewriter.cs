using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class BoundTreeRewriter : BoundTreeVisitor
{
	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		ImmutableArray<BoundNode> childBoundNodes = VisitList(node.ChildBoundNodes);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ResultKind, node.Symbols, childBoundNodes, type);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ConstantValueOpt, type);
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.LocalSymbol, type);
	}

	public override BoundNode VisitGlobal(BoundGlobal node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.GlobalSymbol, type);
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ParameterSymbol, type);
	}

	public override BoundNode VisitReturnValue(BoundReturnValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ReturnValueSymbol, type);
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundExpression targetOpt = (BoundExpression)Visit(node.TargetOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.ConversionKind, node.ResultKind, node.SymbolOpt, targetOpt, node.IsArrayIndex, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitClassType(BoundClassType node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ClassSymbol, type);
	}

	public override BoundNode VisitEnumType(BoundEnumType node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.EnumType, type);
	}

	public override BoundNode VisitSystemOptionType(BoundSystemOptionType node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OptionTypeSymbol, type);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.FieldSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitControlAccess(BoundControlAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.ControlSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitQueryElementAccess(BoundQueryElementAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.QueryElementSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitQueryDataItemAccess(BoundQueryDataItemAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.QueryDataItemSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitReportDataItemAccess(BoundReportDataItemAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.ReportDataItemSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitXmlPortNodeAccess(BoundXmlPortNodeAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.XmlPortNodeSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitTestFieldAccess(BoundTestFieldAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.FieldSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitTestPartAccess(BoundTestPartAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.PartSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitTestFilterAccess(BoundTestFilterAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.TestFilter, node.ResultKind, type);
	}

	public override BoundNode VisitTestFilterFieldAccess(BoundTestFilterFieldAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.TestFilterField, node.ResultKind, type);
	}

	public override BoundNode VisitTestActionAccess(BoundTestActionAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.ActionSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitOptionAccess(BoundOptionAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.OptionSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitApplicationObjectAccess(BoundApplicationObjectAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.ApplicationObjectTypeSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitObjectAccess(BoundObjectAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.ObjectTypeSymbol, node.ResultKind, type);
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> indices = VisitList(node.Indices);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, indices, type);
	}

	public override BoundNode VisitTextIndexAccess(BoundTextIndexAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundExpression index = (BoundExpression)Visit(node.Index);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, index, type);
	}

	public override BoundNode VisitTableKeyAccess(BoundTableKeyAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.Key, node.ResultKind, type);
	}

	public override BoundNode VisitTableFieldGroupAccess(BoundTableFieldGroupAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.FieldGroup, node.ResultKind, type);
	}

	public override BoundNode VisitReportLabelAccess(BoundReportLabelAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.Label, node.ResultKind, type);
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.Method, arguments, node.EffectiveParameters, node.ResultKind, node.IsStatement, type);
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, type);
	}

	public override BoundNode VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, node.ResultKind, type);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, left, right, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, type);
	}

	public override BoundNode VisitInListExpression(BoundInListExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> list = VisitList(node.List);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, list, type);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression);
	}

	public override BoundNode VisitAssignmentStatement(BoundAssignmentStatement node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		return node.Update(left, right);
	}

	public override BoundNode VisitCompoundAssignmentStatement(BoundCompoundAssignmentStatement node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		return node.Update(node.Operator, left, right);
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.Name, node.Methods, node.LookupSymbolOpt, node.LookupError, receiverOpt, node.ResultKind);
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		ImmutableArray<BoundNode> childBoundNodes = VisitList(node.ChildBoundNodes);
		return node.Update(childBoundNodes);
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return node;
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement consequence = (BoundStatement)Visit(node.Consequence);
		BoundStatement alternativeOpt = (BoundStatement)Visit(node.AlternativeOpt);
		return node.Update(condition, consequence, alternativeOpt);
	}

	public override BoundNode VisitCaseStatement(BoundCaseStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundCaseLine> caseLines = VisitList(node.CaseLines);
		BoundStatementList caseElse = (BoundStatementList)Visit(node.CaseElse);
		return node.Update(expression, caseLines, caseElse);
	}

	public override BoundNode VisitCaseLine(BoundCaseLine node)
	{
		ImmutableArray<BoundExpression> expressions = VisitList(node.Expressions);
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(expressions, statement);
	}

	public override BoundNode VisitExitStatement(BoundExitStatement node)
	{
		BoundExpression returnValue = (BoundExpression)Visit(node.ReturnValue);
		return node.Update(returnValue);
	}

	public override BoundNode VisitAssertErrorStatement(BoundAssertErrorStatement node)
	{
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(statement);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		BoundExpression loopVariable = (BoundExpression)Visit(node.LoopVariable);
		BoundExpression initialValue = (BoundExpression)Visit(node.InitialValue);
		BoundExpression endValue = (BoundExpression)Visit(node.EndValue);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(loopVariable, initialValue, endValue, body, node.IsDownto, node.BreakLabel);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		BoundExpression iterationVariable = (BoundExpression)Visit(node.IterationVariable);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.ElementConversion, iterationVariable, expression, body, node.BreakLabel);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(condition, body, node.BreakLabel);
	}

	public override BoundNode VisitRepeatStatement(BoundRepeatStatement node)
	{
		BoundStatementList body = (BoundStatementList)Visit(node.Body);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		return node.Update(body, condition, node.BreakLabel);
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return node;
	}

	public override BoundNode VisitWithStatement(BoundWithStatement node)
	{
		BoundExpression withId = (BoundExpression)Visit(node.WithId);
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(withId, statement);
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		return node.Update(arguments);
	}

	public override BoundNode VisitMethodReference(BoundMethodReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Method, type);
	}

	public override BoundNode VisitBadPropertyValue(BoundBadPropertyValue node)
	{
		BoundBadExpression badPropertyValue = (BoundBadExpression)Visit(node.BadPropertyValue);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(badPropertyValue, type);
	}

	public override BoundNode VisitProperty(BoundProperty node)
	{
		BoundLiteral propertyName = (BoundLiteral)Visit(node.PropertyName);
		BoundPropertyValue propertyValue = (BoundPropertyValue)Visit(node.PropertyValue);
		return node.Update(propertyName, propertyValue);
	}

	public override BoundNode VisitBadProperty(BoundBadProperty node)
	{
		BoundBadStatement badProperty = (BoundBadStatement)Visit(node.BadProperty);
		BoundLiteral propertyName = (BoundLiteral)Visit(node.PropertyName);
		BoundPropertyValue propertyValue = (BoundPropertyValue)Visit(node.PropertyValue);
		return node.Update(badProperty, propertyName, propertyValue);
	}

	public override BoundNode VisitLiteralPropertyValue(BoundLiteralPropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Value, type);
	}

	public override BoundNode VisitIdentifierNamePropertyValue(BoundIdentifierNamePropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Value, type);
	}

	public override BoundNode VisitMultilanguagePropertyValue(BoundMultilanguagePropertyValue node)
	{
		ImmutableArray<BoundMultilanguageExpression> value = VisitList(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitLabelPropertyValue(BoundLabelPropertyValue node)
	{
		BoundLabel boundLabel = (BoundLabel)Visit(node.BoundLabel);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(boundLabel, type);
	}

	public override BoundNode VisitMultilanguageExpression(BoundMultilanguageExpression node)
	{
		BoundExpression languageId = (BoundExpression)Visit(node.LanguageId);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(languageId, node.LanguageCultureId, node.LanguageValue, type);
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Text, node.Comment, node.MaxLength, node.Locked, type);
	}

	public override BoundNode VisitPermissionPropertyValue(BoundPermissionPropertyValue node)
	{
		ImmutableArray<BoundPermissionExpression> value = VisitList(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitPermissionExpression(BoundPermissionExpression node)
	{
		BoundExpression target = (BoundExpression)Visit(node.Target);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ObjectType, target, node.Permission, type);
	}

	public override BoundNode VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Value, type);
	}

	public override BoundNode VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node)
	{
		TypeSymbol memberReference = VisitType(node.MemberReference);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(memberReference, type);
	}

	public override BoundNode VisitIdentifierListPropertyValue(BoundIdentifierListPropertyValue node)
	{
		ImmutableArray<BoundExpression> value = VisitList(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitConstantValueListPropertyValue(BoundConstantValueListPropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Values, type);
	}

	public override BoundNode VisitListOfConstantValuePairsPropertyValue(BoundListOfConstantValuePairsPropertyValue node)
	{
		ImmutableArray<BoundPairOfConstantValues> value = VisitList(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitPairOfConstantValues(BoundPairOfConstantValues node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.First, node.Second, type);
	}

	public override BoundNode VisitEnumPropertyValue(BoundEnumPropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Value, type);
	}

	public override BoundNode VisitOptionMembersPropertyValue(BoundOptionMembersPropertyValue node)
	{
		TypeSymbol value = VisitType(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitTableRelationPropertyValue(BoundTableRelationPropertyValue node)
	{
		BoundTableRelationExpression tableRelationExpression = (BoundTableRelationExpression)Visit(node.TableRelationExpression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(tableRelationExpression, type);
	}

	public override BoundNode VisitTableRelationExpression(BoundTableRelationExpression node)
	{
		BoundTableRelationConditionExpression ifExpression = (BoundTableRelationConditionExpression)Visit(node.IfExpression);
		BoundExpression fieldAccess = (BoundExpression)Visit(node.FieldAccess);
		BoundTableRelationConditionExpression tableFilter = (BoundTableRelationConditionExpression)Visit(node.TableFilter);
		BoundTableRelationExpression elseExpression = (BoundTableRelationExpression)Visit(node.ElseExpression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(ifExpression, fieldAccess, tableFilter, elseExpression, type);
	}

	public override BoundNode VisitTableRelationConditionExpression(BoundTableRelationConditionExpression node)
	{
		ImmutableArray<BoundEqualsPropertyExpression> condition = VisitList(node.Condition);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(condition, type);
	}

	public override BoundNode VisitCalculationFormulaPropertyValue(BoundCalculationFormulaPropertyValue node)
	{
		BoundExpression fieldOrTableAccess = (BoundExpression)Visit(node.FieldOrTableAccess);
		BoundTableRelationConditionExpression tableFilter = (BoundTableRelationConditionExpression)Visit(node.TableFilter);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Operator, node.IsMinus, fieldOrTableAccess, tableFilter, type);
	}

	public override BoundNode VisitEqualsPropertyExpression(BoundEqualsPropertyExpression node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundPropertyRhsExpression right = (BoundPropertyRhsExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, type);
	}

	public override BoundNode VisitPropertyRhsExpression(BoundPropertyRhsExpression node)
	{
		BoundPropertyValueExpression value = (BoundPropertyValueExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Operator, value, type);
	}

	public override BoundNode VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, type);
	}

	public override BoundNode VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node)
	{
		BoundPropertyValueExpression expression = (BoundPropertyValueExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitWildCardPropertyValueExpression(BoundWildCardPropertyValueExpression node)
	{
		BoundLiteral pattern = (BoundLiteral)Visit(node.Pattern);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(pattern, type);
	}

	public override BoundNode VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, type);
	}

	public override BoundNode VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, left, right, type);
	}

	public override BoundNode VisitCodeExpressionPropertyValue(BoundCodeExpressionPropertyValue node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitStyleExpressionPropertyValue(BoundStyleExpressionPropertyValue node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitDecimalPlacesPropertyValue(BoundDecimalPlacesPropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Minimum, node.Maximum, type);
	}

	public override BoundNode VisitTableViewPropertyValue(BoundTableViewPropertyValue node)
	{
		ImmutableArray<BoundExpression> sorting = VisitList(node.Sorting);
		BoundTableRelationConditionExpression tableFilter = (BoundTableRelationConditionExpression)Visit(node.TableFilter);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.DeclaringTableSymbol, sorting, node.IsAscending, tableFilter, type);
	}

	public override BoundNode VisitTableFilterStatementPropertyValue(BoundTableFilterStatementPropertyValue node)
	{
		BoundTableRelationConditionExpression tableFilter = (BoundTableRelationConditionExpression)Visit(node.TableFilter);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.DeclaringTableSymbol, tableFilter, type);
	}

	public override BoundNode VisitOrderByPropertyValue(BoundOrderByPropertyValue node)
	{
		ImmutableArray<BoundOrderByExpression> elements = VisitList(node.Elements);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elements, type);
	}

	public override BoundNode VisitOrderByExpression(BoundOrderByExpression node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.IsAscending, value, type);
	}

	public override BoundNode VisitDataItemLinkPropertyValue(BoundDataItemLinkPropertyValue node)
	{
		ImmutableArray<BoundDataItemLinkExpression> joins = VisitList(node.Joins);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(joins, type);
	}

	public override BoundNode VisitDataItemLinkExpression(BoundDataItemLinkExpression node)
	{
		BoundFieldAccess sourceField = (BoundFieldAccess)Visit(node.SourceField);
		BoundFieldAccess relatedField = (BoundFieldAccess)Visit(node.RelatedField);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceField, relatedField, type);
	}

	public override BoundNode VisitNullPropertyValue(BoundNullPropertyValue node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitTemporary(BoundTemporary node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Name, type);
	}

	public virtual TypeSymbol VisitType(TypeSymbol type)
	{
		return type;
	}

	public ImmutableArray<T> VisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		if (list.IsDefault)
		{
			return list;
		}
		return DoVisitList(list);
	}

	private ImmutableArray<T> DoVisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		ArrayBuilder<T> arrayBuilder = null;
		for (int i = 0; i < list.Length; i++)
		{
			T val = list[i];
			DebugAssertHelper.Assert(val != null);
			BoundNode boundNode = Visit(val);
			if (val != boundNode && arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<T>.GetInstance();
				if (i > 0)
				{
					arrayBuilder.AddRange(list, i);
				}
			}
			if (arrayBuilder != null && boundNode != null)
			{
				arrayBuilder.Add((T)boundNode);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? list;
	}
}
