using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundTreeVisitor<A, R>
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	internal R VisitInternal(BoundNode node, A arg)
	{
		return node.Kind switch
		{
			BoundKind.StatementList => VisitStatementList(node as BoundStatementList, arg), 
			BoundKind.Block => VisitBlock(node as BoundBlock, arg), 
			BoundKind.BadExpression => VisitBadExpression(node as BoundBadExpression, arg), 
			BoundKind.Literal => VisitLiteral(node as BoundLiteral, arg), 
			BoundKind.Local => VisitLocal(node as BoundLocal, arg), 
			BoundKind.Global => VisitGlobal(node as BoundGlobal, arg), 
			BoundKind.Parameter => VisitParameter(node as BoundParameter, arg), 
			BoundKind.ReturnValue => VisitReturnValue(node as BoundReturnValue, arg), 
			BoundKind.Conversion => VisitConversion(node as BoundConversion, arg), 
			BoundKind.ClassType => VisitClassType(node as BoundClassType, arg), 
			BoundKind.EnumType => VisitEnumType(node as BoundEnumType, arg), 
			BoundKind.SystemOptionType => VisitSystemOptionType(node as BoundSystemOptionType, arg), 
			BoundKind.FieldAccess => VisitFieldAccess(node as BoundFieldAccess, arg), 
			BoundKind.ControlAccess => VisitControlAccess(node as BoundControlAccess, arg), 
			BoundKind.QueryElementAccess => VisitQueryElementAccess(node as BoundQueryElementAccess, arg), 
			BoundKind.QueryDataItemAccess => VisitQueryDataItemAccess(node as BoundQueryDataItemAccess, arg), 
			BoundKind.ReportDataItemAccess => VisitReportDataItemAccess(node as BoundReportDataItemAccess, arg), 
			BoundKind.XmlPortNodeAccess => VisitXmlPortNodeAccess(node as BoundXmlPortNodeAccess, arg), 
			BoundKind.TestFieldAccess => VisitTestFieldAccess(node as BoundTestFieldAccess, arg), 
			BoundKind.TestPartAccess => VisitTestPartAccess(node as BoundTestPartAccess, arg), 
			BoundKind.TestFilterAccess => VisitTestFilterAccess(node as BoundTestFilterAccess, arg), 
			BoundKind.TestFilterFieldAccess => VisitTestFilterFieldAccess(node as BoundTestFilterFieldAccess, arg), 
			BoundKind.TestActionAccess => VisitTestActionAccess(node as BoundTestActionAccess, arg), 
			BoundKind.OptionAccess => VisitOptionAccess(node as BoundOptionAccess, arg), 
			BoundKind.ApplicationObjectAccess => VisitApplicationObjectAccess(node as BoundApplicationObjectAccess, arg), 
			BoundKind.ObjectAccess => VisitObjectAccess(node as BoundObjectAccess, arg), 
			BoundKind.ArrayAccess => VisitArrayAccess(node as BoundArrayAccess, arg), 
			BoundKind.TextIndexAccess => VisitTextIndexAccess(node as BoundTextIndexAccess, arg), 
			BoundKind.TableKeyAccess => VisitTableKeyAccess(node as BoundTableKeyAccess, arg), 
			BoundKind.TableFieldGroupAccess => VisitTableFieldGroupAccess(node as BoundTableFieldGroupAccess, arg), 
			BoundKind.ReportLabelAccess => VisitReportLabelAccess(node as BoundReportLabelAccess, arg), 
			BoundKind.Call => VisitCall(node as BoundCall, arg), 
			BoundKind.UnaryOperator => VisitUnaryOperator(node as BoundUnaryOperator, arg), 
			BoundKind.ParenthesizedExpression => VisitParenthesizedExpression(node as BoundParenthesizedExpression, arg), 
			BoundKind.BinaryOperator => VisitBinaryOperator(node as BoundBinaryOperator, arg), 
			BoundKind.InListExpression => VisitInListExpression(node as BoundInListExpression, arg), 
			BoundKind.ExpressionStatement => VisitExpressionStatement(node as BoundExpressionStatement, arg), 
			BoundKind.AssignmentStatement => VisitAssignmentStatement(node as BoundAssignmentStatement, arg), 
			BoundKind.CompoundAssignmentStatement => VisitCompoundAssignmentStatement(node as BoundCompoundAssignmentStatement, arg), 
			BoundKind.MethodGroup => VisitMethodGroup(node as BoundMethodGroup, arg), 
			BoundKind.BadStatement => VisitBadStatement(node as BoundBadStatement, arg), 
			BoundKind.NoOpStatement => VisitNoOpStatement(node as BoundNoOpStatement, arg), 
			BoundKind.IfStatement => VisitIfStatement(node as BoundIfStatement, arg), 
			BoundKind.CaseStatement => VisitCaseStatement(node as BoundCaseStatement, arg), 
			BoundKind.CaseLine => VisitCaseLine(node as BoundCaseLine, arg), 
			BoundKind.ExitStatement => VisitExitStatement(node as BoundExitStatement, arg), 
			BoundKind.AssertErrorStatement => VisitAssertErrorStatement(node as BoundAssertErrorStatement, arg), 
			BoundKind.ForStatement => VisitForStatement(node as BoundForStatement, arg), 
			BoundKind.ForEachStatement => VisitForEachStatement(node as BoundForEachStatement, arg), 
			BoundKind.WhileStatement => VisitWhileStatement(node as BoundWhileStatement, arg), 
			BoundKind.RepeatStatement => VisitRepeatStatement(node as BoundRepeatStatement, arg), 
			BoundKind.BreakStatement => VisitBreakStatement(node as BoundBreakStatement, arg), 
			BoundKind.WithStatement => VisitWithStatement(node as BoundWithStatement, arg), 
			BoundKind.Attribute => VisitAttribute(node as BoundAttribute, arg), 
			BoundKind.MethodReference => VisitMethodReference(node as BoundMethodReference, arg), 
			BoundKind.BadPropertyValue => VisitBadPropertyValue(node as BoundBadPropertyValue, arg), 
			BoundKind.Property => VisitProperty(node as BoundProperty, arg), 
			BoundKind.BadProperty => VisitBadProperty(node as BoundBadProperty, arg), 
			BoundKind.LiteralPropertyValue => VisitLiteralPropertyValue(node as BoundLiteralPropertyValue, arg), 
			BoundKind.IdentifierNamePropertyValue => VisitIdentifierNamePropertyValue(node as BoundIdentifierNamePropertyValue, arg), 
			BoundKind.MultilanguagePropertyValue => VisitMultilanguagePropertyValue(node as BoundMultilanguagePropertyValue, arg), 
			BoundKind.LabelPropertyValue => VisitLabelPropertyValue(node as BoundLabelPropertyValue, arg), 
			BoundKind.MultilanguageExpression => VisitMultilanguageExpression(node as BoundMultilanguageExpression, arg), 
			BoundKind.Label => VisitLabel(node as BoundLabel, arg), 
			BoundKind.PermissionPropertyValue => VisitPermissionPropertyValue(node as BoundPermissionPropertyValue, arg), 
			BoundKind.PermissionExpression => VisitPermissionExpression(node as BoundPermissionExpression, arg), 
			BoundKind.ApplicationObjectReferencePropertyValue => VisitApplicationObjectReferencePropertyValue(node as BoundApplicationObjectReferencePropertyValue, arg), 
			BoundKind.MemberReferencePropertyValue => VisitMemberReferencePropertyValue(node as BoundMemberReferencePropertyValue, arg), 
			BoundKind.IdentifierListPropertyValue => VisitIdentifierListPropertyValue(node as BoundIdentifierListPropertyValue, arg), 
			BoundKind.ConstantValueListPropertyValue => VisitConstantValueListPropertyValue(node as BoundConstantValueListPropertyValue, arg), 
			BoundKind.ListOfConstantValuePairsPropertyValue => VisitListOfConstantValuePairsPropertyValue(node as BoundListOfConstantValuePairsPropertyValue, arg), 
			BoundKind.PairOfConstantValues => VisitPairOfConstantValues(node as BoundPairOfConstantValues, arg), 
			BoundKind.EnumPropertyValue => VisitEnumPropertyValue(node as BoundEnumPropertyValue, arg), 
			BoundKind.OptionMembersPropertyValue => VisitOptionMembersPropertyValue(node as BoundOptionMembersPropertyValue, arg), 
			BoundKind.TableRelationPropertyValue => VisitTableRelationPropertyValue(node as BoundTableRelationPropertyValue, arg), 
			BoundKind.TableRelationExpression => VisitTableRelationExpression(node as BoundTableRelationExpression, arg), 
			BoundKind.TableRelationConditionExpression => VisitTableRelationConditionExpression(node as BoundTableRelationConditionExpression, arg), 
			BoundKind.CalculationFormulaPropertyValue => VisitCalculationFormulaPropertyValue(node as BoundCalculationFormulaPropertyValue, arg), 
			BoundKind.EqualsPropertyExpression => VisitEqualsPropertyExpression(node as BoundEqualsPropertyExpression, arg), 
			BoundKind.PropertyRhsExpression => VisitPropertyRhsExpression(node as BoundPropertyRhsExpression, arg), 
			BoundKind.ConstOrFieldAccessValueExpression => VisitConstOrFieldAccessValueExpression(node as BoundConstOrFieldAccessValueExpression, arg), 
			BoundKind.ParenthesizedFilterExpression => VisitParenthesizedFilterExpression(node as BoundParenthesizedFilterExpression, arg), 
			BoundKind.WildCardPropertyValueExpression => VisitWildCardPropertyValueExpression(node as BoundWildCardPropertyValueExpression, arg), 
			BoundKind.UnaryPropertyFilterExpression => VisitUnaryPropertyFilterExpression(node as BoundUnaryPropertyFilterExpression, arg), 
			BoundKind.BinaryPropertyFilterExpression => VisitBinaryPropertyFilterExpression(node as BoundBinaryPropertyFilterExpression, arg), 
			BoundKind.CodeExpressionPropertyValue => VisitCodeExpressionPropertyValue(node as BoundCodeExpressionPropertyValue, arg), 
			BoundKind.StyleExpressionPropertyValue => VisitStyleExpressionPropertyValue(node as BoundStyleExpressionPropertyValue, arg), 
			BoundKind.DecimalPlacesPropertyValue => VisitDecimalPlacesPropertyValue(node as BoundDecimalPlacesPropertyValue, arg), 
			BoundKind.TableViewPropertyValue => VisitTableViewPropertyValue(node as BoundTableViewPropertyValue, arg), 
			BoundKind.TableFilterStatementPropertyValue => VisitTableFilterStatementPropertyValue(node as BoundTableFilterStatementPropertyValue, arg), 
			BoundKind.OrderByPropertyValue => VisitOrderByPropertyValue(node as BoundOrderByPropertyValue, arg), 
			BoundKind.OrderByExpression => VisitOrderByExpression(node as BoundOrderByExpression, arg), 
			BoundKind.DataItemLinkPropertyValue => VisitDataItemLinkPropertyValue(node as BoundDataItemLinkPropertyValue, arg), 
			BoundKind.DataItemLinkExpression => VisitDataItemLinkExpression(node as BoundDataItemLinkExpression, arg), 
			BoundKind.NullPropertyValue => VisitNullPropertyValue(node as BoundNullPropertyValue, arg), 
			BoundKind.Temporary => VisitTemporary(node as BoundTemporary, arg), 
			_ => default, 
		};
	}

	public virtual R VisitStatementList(BoundStatementList node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBlock(BoundBlock node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadExpression(BoundBadExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLiteral(BoundLiteral node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLocal(BoundLocal node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitGlobal(BoundGlobal node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitParameter(BoundParameter node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitReturnValue(BoundReturnValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConversion(BoundConversion node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitClassType(BoundClassType node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitEnumType(BoundEnumType node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSystemOptionType(BoundSystemOptionType node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitFieldAccess(BoundFieldAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitControlAccess(BoundControlAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitQueryElementAccess(BoundQueryElementAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitQueryDataItemAccess(BoundQueryDataItemAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitReportDataItemAccess(BoundReportDataItemAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitXmlPortNodeAccess(BoundXmlPortNodeAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTestFieldAccess(BoundTestFieldAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTestPartAccess(BoundTestPartAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTestFilterAccess(BoundTestFilterAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTestFilterFieldAccess(BoundTestFilterFieldAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTestActionAccess(BoundTestActionAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOptionAccess(BoundOptionAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitApplicationObjectAccess(BoundApplicationObjectAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitObjectAccess(BoundObjectAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArrayAccess(BoundArrayAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTextIndexAccess(BoundTextIndexAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableKeyAccess(BoundTableKeyAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableFieldGroupAccess(BoundTableFieldGroupAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitReportLabelAccess(BoundReportLabelAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCall(BoundCall node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUnaryOperator(BoundUnaryOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitParenthesizedExpression(BoundParenthesizedExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBinaryOperator(BoundBinaryOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitInListExpression(BoundInListExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitExpressionStatement(BoundExpressionStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAssignmentStatement(BoundAssignmentStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCompoundAssignmentStatement(BoundCompoundAssignmentStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMethodGroup(BoundMethodGroup node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadStatement(BoundBadStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNoOpStatement(BoundNoOpStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIfStatement(BoundIfStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCaseStatement(BoundCaseStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCaseLine(BoundCaseLine node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitExitStatement(BoundExitStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAssertErrorStatement(BoundAssertErrorStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitForStatement(BoundForStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitForEachStatement(BoundForEachStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitWhileStatement(BoundWhileStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitRepeatStatement(BoundRepeatStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBreakStatement(BoundBreakStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitWithStatement(BoundWithStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAttribute(BoundAttribute node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMethodReference(BoundMethodReference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadPropertyValue(BoundBadPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitProperty(BoundProperty node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadProperty(BoundBadProperty node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLiteralPropertyValue(BoundLiteralPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIdentifierNamePropertyValue(BoundIdentifierNamePropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMultilanguagePropertyValue(BoundMultilanguagePropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLabelPropertyValue(BoundLabelPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMultilanguageExpression(BoundMultilanguageExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLabel(BoundLabel node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPermissionPropertyValue(BoundPermissionPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPermissionExpression(BoundPermissionExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIdentifierListPropertyValue(BoundIdentifierListPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConstantValueListPropertyValue(BoundConstantValueListPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitListOfConstantValuePairsPropertyValue(BoundListOfConstantValuePairsPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPairOfConstantValues(BoundPairOfConstantValues node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitEnumPropertyValue(BoundEnumPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOptionMembersPropertyValue(BoundOptionMembersPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableRelationPropertyValue(BoundTableRelationPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableRelationExpression(BoundTableRelationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableRelationConditionExpression(BoundTableRelationConditionExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCalculationFormulaPropertyValue(BoundCalculationFormulaPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitEqualsPropertyExpression(BoundEqualsPropertyExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPropertyRhsExpression(BoundPropertyRhsExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitWildCardPropertyValueExpression(BoundWildCardPropertyValueExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCodeExpressionPropertyValue(BoundCodeExpressionPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitStyleExpressionPropertyValue(BoundStyleExpressionPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDecimalPlacesPropertyValue(BoundDecimalPlacesPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableViewPropertyValue(BoundTableViewPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTableFilterStatementPropertyValue(BoundTableFilterStatementPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOrderByPropertyValue(BoundOrderByPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOrderByExpression(BoundOrderByExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDataItemLinkPropertyValue(BoundDataItemLinkPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDataItemLinkExpression(BoundDataItemLinkExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNullPropertyValue(BoundNullPropertyValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTemporary(BoundTemporary node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R Visit(BoundNode node, A arg)
	{
		if (node == null)
		{
			return default;
		}
		return node.Kind switch
		{
			BoundKind.ReturnValue => VisitReturnValue(node as BoundReturnValue, arg), 
			BoundKind.BinaryOperator => VisitBinaryOperator(node as BoundBinaryOperator, arg), 
			BoundKind.Block => VisitBlock(node as BoundBlock, arg), 
			BoundKind.NoOpStatement => VisitNoOpStatement(node as BoundNoOpStatement, arg), 
			BoundKind.ExitStatement => VisitExitStatement(node as BoundExitStatement, arg), 
			BoundKind.CaseStatement => VisitCaseStatement(node as BoundCaseStatement, arg), 
			BoundKind.BreakStatement => VisitBreakStatement(node as BoundBreakStatement, arg), 
			BoundKind.IfStatement => VisitIfStatement(node as BoundIfStatement, arg), 
			BoundKind.Literal => VisitLiteral(node as BoundLiteral, arg), 
			BoundKind.Global => VisitGlobal(node as BoundGlobal, arg), 
			BoundKind.Local => VisitLocal(node as BoundLocal, arg), 
			BoundKind.Parameter => VisitParameter(node as BoundParameter, arg), 
			BoundKind.StatementList => VisitStatementList(node as BoundStatementList, arg), 
			BoundKind.Call => VisitCall(node as BoundCall, arg), 
			BoundKind.FieldAccess => VisitFieldAccess(node as BoundFieldAccess, arg), 
			_ => VisitInternal(node, arg), 
		};
	}

	public virtual R DefaultVisit(BoundNode node, A arg)
	{
		return default;
	}
}
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class BoundTreeVisitor
{
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	[SuppressMessage("Nav.Reliability", "DN0005:DontDeriveFromException", MessageId = "DN0005")]
	public class CancelledByStackGuardException : Exception
	{
		public readonly BoundNode Node;

		public CancelledByStackGuardException(Exception inner, BoundNode node)
			: base(inner.Message, inner)
		{
			Node = node;
		}

		public static Location GetTooLongOrComplexExpressionErrorLocation(BoundNode node)
		{
			SyntaxNode syntaxNode = node.Syntax;
			if (!(syntaxNode is ExpressionSyntax))
			{
				syntaxNode = syntaxNode.DescendantNodes((SyntaxNode n) => !(n is ExpressionSyntax)).OfType<ExpressionSyntax>().FirstOrDefault() ?? syntaxNode;
			}
			return syntaxNode.GetFirstToken().GetLocation();
		}
	}

	public virtual BoundNode VisitStatementList(BoundStatementList node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBlock(BoundBlock node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLiteral(BoundLiteral node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLocal(BoundLocal node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitGlobal(BoundGlobal node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitParameter(BoundParameter node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitReturnValue(BoundReturnValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConversion(BoundConversion node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitClassType(BoundClassType node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitEnumType(BoundEnumType node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSystemOptionType(BoundSystemOptionType node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitControlAccess(BoundControlAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitQueryElementAccess(BoundQueryElementAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitQueryDataItemAccess(BoundQueryDataItemAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitReportDataItemAccess(BoundReportDataItemAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitXmlPortNodeAccess(BoundXmlPortNodeAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTestFieldAccess(BoundTestFieldAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTestPartAccess(BoundTestPartAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTestFilterAccess(BoundTestFilterAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTestFilterFieldAccess(BoundTestFilterFieldAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTestActionAccess(BoundTestActionAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOptionAccess(BoundOptionAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitApplicationObjectAccess(BoundApplicationObjectAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitObjectAccess(BoundObjectAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTextIndexAccess(BoundTextIndexAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableKeyAccess(BoundTableKeyAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableFieldGroupAccess(BoundTableFieldGroupAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitReportLabelAccess(BoundReportLabelAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCall(BoundCall node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitInListExpression(BoundInListExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAssignmentStatement(BoundAssignmentStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCompoundAssignmentStatement(BoundCompoundAssignmentStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadStatement(BoundBadStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIfStatement(BoundIfStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCaseStatement(BoundCaseStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCaseLine(BoundCaseLine node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitExitStatement(BoundExitStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAssertErrorStatement(BoundAssertErrorStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitForStatement(BoundForStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitRepeatStatement(BoundRepeatStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitWithStatement(BoundWithStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAttribute(BoundAttribute node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMethodReference(BoundMethodReference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadPropertyValue(BoundBadPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitProperty(BoundProperty node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadProperty(BoundBadProperty node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLiteralPropertyValue(BoundLiteralPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIdentifierNamePropertyValue(BoundIdentifierNamePropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMultilanguagePropertyValue(BoundMultilanguagePropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLabelPropertyValue(BoundLabelPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMultilanguageExpression(BoundMultilanguageExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLabel(BoundLabel node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPermissionPropertyValue(BoundPermissionPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPermissionExpression(BoundPermissionExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIdentifierListPropertyValue(BoundIdentifierListPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConstantValueListPropertyValue(BoundConstantValueListPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitListOfConstantValuePairsPropertyValue(BoundListOfConstantValuePairsPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPairOfConstantValues(BoundPairOfConstantValues node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitEnumPropertyValue(BoundEnumPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOptionMembersPropertyValue(BoundOptionMembersPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableRelationPropertyValue(BoundTableRelationPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableRelationExpression(BoundTableRelationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableRelationConditionExpression(BoundTableRelationConditionExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCalculationFormulaPropertyValue(BoundCalculationFormulaPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitEqualsPropertyExpression(BoundEqualsPropertyExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPropertyRhsExpression(BoundPropertyRhsExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitWildCardPropertyValueExpression(BoundWildCardPropertyValueExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCodeExpressionPropertyValue(BoundCodeExpressionPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitStyleExpressionPropertyValue(BoundStyleExpressionPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDecimalPlacesPropertyValue(BoundDecimalPlacesPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableViewPropertyValue(BoundTableViewPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTableFilterStatementPropertyValue(BoundTableFilterStatementPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOrderByPropertyValue(BoundOrderByPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOrderByExpression(BoundOrderByExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDataItemLinkPropertyValue(BoundDataItemLinkPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDataItemLinkExpression(BoundDataItemLinkExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNullPropertyValue(BoundNullPropertyValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTemporary(BoundTemporary node)
	{
		return DefaultVisit(node);
	}

	[DebuggerHidden]
	public virtual BoundNode Visit(BoundNode node)
	{
		return node?.Accept(this);
	}

	[DebuggerHidden]
	public virtual BoundNode DefaultVisit(BoundNode node)
	{
		return null;
	}

	protected BoundExpression VisitExpressionWithStackGuard(ref int recursionDepth, BoundExpression node)
	{
		recursionDepth++;
		BoundExpression result;
		if (recursionDepth > 1 || !ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException())
		{
			StackGuard.EnsureSufficientExecutionStack(recursionDepth);
			result = VisitExpressionWithoutStackGuard(node);
		}
		else
		{
			result = VisitExpressionWithStackGuard(node);
		}
		recursionDepth--;
		return result;
	}

	protected virtual bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return true;
	}

	private BoundExpression VisitExpressionWithStackGuard(BoundExpression node)
	{
		try
		{
			return VisitExpressionWithoutStackGuard(node);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			throw new CancelledByStackGuardException(ex, node);
		}
	}

	protected abstract BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node);
}
