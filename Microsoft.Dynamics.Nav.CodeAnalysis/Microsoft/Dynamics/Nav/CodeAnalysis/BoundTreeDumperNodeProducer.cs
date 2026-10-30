using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTreeDumperNodeProducer : BoundTreeVisitor<object, TreeDumperNode>
{
	private BoundTreeDumperNodeProducer()
	{
	}

	public static TreeDumperNode MakeTree(BoundNode node)
	{
		return new BoundTreeDumperNodeProducer().Visit(node, null);
	}

	public override TreeDumperNode VisitStatementList(BoundStatementList node, object arg)
	{
		return new TreeDumperNode("statementList", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("statements", null, node.Statements.Select((BoundStatement x) => Visit(x, null)))
		});
	}

	public override TreeDumperNode VisitBlock(BoundBlock node, object arg)
	{
		return new TreeDumperNode("block", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("statements", null, node.Statements.Select((BoundStatement x) => Visit(x, null)))
		});
	}

	public override TreeDumperNode VisitBadExpression(BoundBadExpression node, object arg)
	{
		return new TreeDumperNode("badExpression", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("symbols", node.Symbols, null),
			new TreeDumperNode("childBoundNodes", null, node.ChildBoundNodes.Select((BoundNode x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitLiteral(BoundLiteral node, object arg)
	{
		return new TreeDumperNode("literal", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("constantValueOpt", node.ConstantValueOpt, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitLocal(BoundLocal node, object arg)
	{
		return new TreeDumperNode("local", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("localSymbol", node.LocalSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitGlobal(BoundGlobal node, object arg)
	{
		return new TreeDumperNode("global", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("globalSymbol", node.GlobalSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitParameter(BoundParameter node, object arg)
	{
		return new TreeDumperNode("parameter", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("parameterSymbol", node.ParameterSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitReturnValue(BoundReturnValue node, object arg)
	{
		return new TreeDumperNode("returnValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("returnValueSymbol", node.ReturnValueSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitConversion(BoundConversion node, object arg)
	{
		return new TreeDumperNode("conversion", null, new TreeDumperNode[8]
		{
			new TreeDumperNode("operand", null, new TreeDumperNode[1] { Visit(node.Operand, null) }),
			new TreeDumperNode("conversionKind", node.ConversionKind, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("symbolOpt", node.SymbolOpt, null),
			new TreeDumperNode("targetOpt", null, new TreeDumperNode[1] { Visit(node.TargetOpt, null) }),
			new TreeDumperNode("isArrayIndex", node.IsArrayIndex, null),
			new TreeDumperNode("constantValueOpt", node.ConstantValueOpt, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitClassType(BoundClassType node, object arg)
	{
		return new TreeDumperNode("classType", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("classSymbol", node.ClassSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitEnumType(BoundEnumType node, object arg)
	{
		return new TreeDumperNode("enumType", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("enumType", node.EnumType, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitSystemOptionType(BoundSystemOptionType node, object arg)
	{
		return new TreeDumperNode("systemOptionType", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("optionTypeSymbol", node.OptionTypeSymbol, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitFieldAccess(BoundFieldAccess node, object arg)
	{
		return new TreeDumperNode("fieldAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("fieldSymbol", node.FieldSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitControlAccess(BoundControlAccess node, object arg)
	{
		return new TreeDumperNode("controlAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("controlSymbol", node.ControlSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitQueryElementAccess(BoundQueryElementAccess node, object arg)
	{
		return new TreeDumperNode("queryElementAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("queryElementSymbol", node.QueryElementSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitQueryDataItemAccess(BoundQueryDataItemAccess node, object arg)
	{
		return new TreeDumperNode("queryDataItemAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("queryDataItemSymbol", node.QueryDataItemSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitReportDataItemAccess(BoundReportDataItemAccess node, object arg)
	{
		return new TreeDumperNode("reportDataItemAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("reportDataItemSymbol", node.ReportDataItemSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitXmlPortNodeAccess(BoundXmlPortNodeAccess node, object arg)
	{
		return new TreeDumperNode("xmlPortNodeAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("xmlPortNodeSymbol", node.XmlPortNodeSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTestFieldAccess(BoundTestFieldAccess node, object arg)
	{
		return new TreeDumperNode("testFieldAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("fieldSymbol", node.FieldSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTestPartAccess(BoundTestPartAccess node, object arg)
	{
		return new TreeDumperNode("testPartAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("partSymbol", node.PartSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTestFilterAccess(BoundTestFilterAccess node, object arg)
	{
		return new TreeDumperNode("testFilterAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("testFilter", node.TestFilter, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTestFilterFieldAccess(BoundTestFilterFieldAccess node, object arg)
	{
		return new TreeDumperNode("testFilterFieldAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("testFilterField", node.TestFilterField, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTestActionAccess(BoundTestActionAccess node, object arg)
	{
		return new TreeDumperNode("testActionAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("actionSymbol", node.ActionSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitOptionAccess(BoundOptionAccess node, object arg)
	{
		return new TreeDumperNode("optionAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("optionSymbol", node.OptionSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitApplicationObjectAccess(BoundApplicationObjectAccess node, object arg)
	{
		return new TreeDumperNode("applicationObjectAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("applicationObjectTypeSymbol", node.ApplicationObjectTypeSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitObjectAccess(BoundObjectAccess node, object arg)
	{
		return new TreeDumperNode("objectAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("objectTypeSymbol", node.ObjectTypeSymbol, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitArrayAccess(BoundArrayAccess node, object arg)
	{
		return new TreeDumperNode("arrayAccess", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("indices", null, node.Indices.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTextIndexAccess(BoundTextIndexAccess node, object arg)
	{
		return new TreeDumperNode("textIndexAccess", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("index", null, new TreeDumperNode[1] { Visit(node.Index, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableKeyAccess(BoundTableKeyAccess node, object arg)
	{
		return new TreeDumperNode("tableKeyAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("key", node.Key, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableFieldGroupAccess(BoundTableFieldGroupAccess node, object arg)
	{
		return new TreeDumperNode("tableFieldGroupAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("fieldGroup", node.FieldGroup, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitReportLabelAccess(BoundReportLabelAccess node, object arg)
	{
		return new TreeDumperNode("reportLabelAccess", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("label", node.Label, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitCall(BoundCall node, object arg)
	{
		return new TreeDumperNode("call", null, new TreeDumperNode[7]
		{
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("method", node.Method, null),
			new TreeDumperNode("arguments", null, node.Arguments.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("effectiveParameters", node.EffectiveParameters, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("isStatement", node.IsStatement, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitUnaryOperator(BoundUnaryOperator node, object arg)
	{
		return new TreeDumperNode("unaryOperator", null, new TreeDumperNode[6]
		{
			new TreeDumperNode("operatorKind", node.OperatorKind, null),
			new TreeDumperNode("operand", null, new TreeDumperNode[1] { Visit(node.Operand, null) }),
			new TreeDumperNode("constantValueOpt", node.ConstantValueOpt, null),
			new TreeDumperNode("methodOpt", node.MethodOpt, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitParenthesizedExpression(BoundParenthesizedExpression node, object arg)
	{
		return new TreeDumperNode("parenthesizedExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitBinaryOperator(BoundBinaryOperator node, object arg)
	{
		return new TreeDumperNode("binaryOperator", null, new TreeDumperNode[7]
		{
			new TreeDumperNode("operatorKind", node.OperatorKind, null),
			new TreeDumperNode("left", null, new TreeDumperNode[1] { Visit(node.Left, null) }),
			new TreeDumperNode("right", null, new TreeDumperNode[1] { Visit(node.Right, null) }),
			new TreeDumperNode("constantValueOpt", node.ConstantValueOpt, null),
			new TreeDumperNode("methodOpt", node.MethodOpt, null),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitInListExpression(BoundInListExpression node, object arg)
	{
		return new TreeDumperNode("inListExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("list", null, node.List.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitExpressionStatement(BoundExpressionStatement node, object arg)
	{
		return new TreeDumperNode("expressionStatement", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) })
		});
	}

	public override TreeDumperNode VisitAssignmentStatement(BoundAssignmentStatement node, object arg)
	{
		return new TreeDumperNode("assignmentStatement", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("left", null, new TreeDumperNode[1] { Visit(node.Left, null) }),
			new TreeDumperNode("right", null, new TreeDumperNode[1] { Visit(node.Right, null) })
		});
	}

	public override TreeDumperNode VisitCompoundAssignmentStatement(BoundCompoundAssignmentStatement node, object arg)
	{
		return new TreeDumperNode("compoundAssignmentStatement", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("@operator", node.Operator, null),
			new TreeDumperNode("left", null, new TreeDumperNode[1] { Visit(node.Left, null) }),
			new TreeDumperNode("right", null, new TreeDumperNode[1] { Visit(node.Right, null) })
		});
	}

	public override TreeDumperNode VisitMethodGroup(BoundMethodGroup node, object arg)
	{
		return new TreeDumperNode("methodGroup", null, new TreeDumperNode[7]
		{
			new TreeDumperNode("name", node.Name, null),
			new TreeDumperNode("methods", node.Methods, null),
			new TreeDumperNode("lookupSymbolOpt", node.LookupSymbolOpt, null),
			new TreeDumperNode("lookupError", node.LookupError, null),
			new TreeDumperNode("receiverOpt", null, new TreeDumperNode[1] { Visit(node.ReceiverOpt, null) }),
			new TreeDumperNode("resultKind", node.ResultKind, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitBadStatement(BoundBadStatement node, object arg)
	{
		return new TreeDumperNode("badStatement", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("childBoundNodes", null, node.ChildBoundNodes.Select((BoundNode x) => Visit(x, null)))
		});
	}

	public override TreeDumperNode VisitNoOpStatement(BoundNoOpStatement node, object arg)
	{
		return new TreeDumperNode("noOpStatement", null, SpecializedCollections.EmptyArray<TreeDumperNode>());
	}

	public override TreeDumperNode VisitIfStatement(BoundIfStatement node, object arg)
	{
		return new TreeDumperNode("ifStatement", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("condition", null, new TreeDumperNode[1] { Visit(node.Condition, null) }),
			new TreeDumperNode("consequence", null, new TreeDumperNode[1] { Visit(node.Consequence, null) }),
			new TreeDumperNode("alternativeOpt", null, new TreeDumperNode[1] { Visit(node.AlternativeOpt, null) })
		});
	}

	public override TreeDumperNode VisitCaseStatement(BoundCaseStatement node, object arg)
	{
		return new TreeDumperNode("caseStatement", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("caseLines", null, node.CaseLines.Select((BoundCaseLine x) => Visit(x, null))),
			new TreeDumperNode("caseElse", null, new TreeDumperNode[1] { Visit(node.CaseElse, null) })
		});
	}

	public override TreeDumperNode VisitCaseLine(BoundCaseLine node, object arg)
	{
		return new TreeDumperNode("caseLine", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("expressions", null, node.Expressions.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("statement", null, new TreeDumperNode[1] { Visit(node.Statement, null) })
		});
	}

	public override TreeDumperNode VisitExitStatement(BoundExitStatement node, object arg)
	{
		return new TreeDumperNode("exitStatement", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("returnValue", null, new TreeDumperNode[1] { Visit(node.ReturnValue, null) })
		});
	}

	public override TreeDumperNode VisitAssertErrorStatement(BoundAssertErrorStatement node, object arg)
	{
		return new TreeDumperNode("assertErrorStatement", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("statement", null, new TreeDumperNode[1] { Visit(node.Statement, null) })
		});
	}

	public override TreeDumperNode VisitForStatement(BoundForStatement node, object arg)
	{
		return new TreeDumperNode("forStatement", null, new TreeDumperNode[6]
		{
			new TreeDumperNode("loopVariable", null, new TreeDumperNode[1] { Visit(node.LoopVariable, null) }),
			new TreeDumperNode("initialValue", null, new TreeDumperNode[1] { Visit(node.InitialValue, null) }),
			new TreeDumperNode("endValue", null, new TreeDumperNode[1] { Visit(node.EndValue, null) }),
			new TreeDumperNode("body", null, new TreeDumperNode[1] { Visit(node.Body, null) }),
			new TreeDumperNode("isDownto", node.IsDownto, null),
			new TreeDumperNode("breakLabel", node.BreakLabel, null)
		});
	}

	public override TreeDumperNode VisitForEachStatement(BoundForEachStatement node, object arg)
	{
		return new TreeDumperNode("forEachStatement", null, new TreeDumperNode[5]
		{
			new TreeDumperNode("elementConversion", node.ElementConversion, null),
			new TreeDumperNode("iterationVariable", null, new TreeDumperNode[1] { Visit(node.IterationVariable, null) }),
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("body", null, new TreeDumperNode[1] { Visit(node.Body, null) }),
			new TreeDumperNode("breakLabel", node.BreakLabel, null)
		});
	}

	public override TreeDumperNode VisitWhileStatement(BoundWhileStatement node, object arg)
	{
		return new TreeDumperNode("whileStatement", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("condition", null, new TreeDumperNode[1] { Visit(node.Condition, null) }),
			new TreeDumperNode("body", null, new TreeDumperNode[1] { Visit(node.Body, null) }),
			new TreeDumperNode("breakLabel", node.BreakLabel, null)
		});
	}

	public override TreeDumperNode VisitRepeatStatement(BoundRepeatStatement node, object arg)
	{
		return new TreeDumperNode("repeatStatement", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("body", null, new TreeDumperNode[1] { Visit(node.Body, null) }),
			new TreeDumperNode("condition", null, new TreeDumperNode[1] { Visit(node.Condition, null) }),
			new TreeDumperNode("breakLabel", node.BreakLabel, null)
		});
	}

	public override TreeDumperNode VisitBreakStatement(BoundBreakStatement node, object arg)
	{
		return new TreeDumperNode("breakStatement", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("label", node.Label, null)
		});
	}

	public override TreeDumperNode VisitWithStatement(BoundWithStatement node, object arg)
	{
		return new TreeDumperNode("withStatement", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("withId", null, new TreeDumperNode[1] { Visit(node.WithId, null) }),
			new TreeDumperNode("statement", null, new TreeDumperNode[1] { Visit(node.Statement, null) })
		});
	}

	public override TreeDumperNode VisitAttribute(BoundAttribute node, object arg)
	{
		return new TreeDumperNode("attribute", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("arguments", null, node.Arguments.Select((BoundExpression x) => Visit(x, null)))
		});
	}

	public override TreeDumperNode VisitMethodReference(BoundMethodReference node, object arg)
	{
		return new TreeDumperNode("methodReference", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("method", node.Method, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitBadPropertyValue(BoundBadPropertyValue node, object arg)
	{
		return new TreeDumperNode("badPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("badPropertyValue", null, new TreeDumperNode[1] { Visit(node.BadPropertyValue, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitProperty(BoundProperty node, object arg)
	{
		return new TreeDumperNode("property", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("propertyName", null, new TreeDumperNode[1] { Visit(node.PropertyName, null) }),
			new TreeDumperNode("propertyValue", null, new TreeDumperNode[1] { Visit(node.PropertyValue, null) })
		});
	}

	public override TreeDumperNode VisitBadProperty(BoundBadProperty node, object arg)
	{
		return new TreeDumperNode("badProperty", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("badProperty", null, new TreeDumperNode[1] { Visit(node.BadProperty, null) }),
			new TreeDumperNode("propertyName", null, new TreeDumperNode[1] { Visit(node.PropertyName, null) }),
			new TreeDumperNode("propertyValue", null, new TreeDumperNode[1] { Visit(node.PropertyValue, null) })
		});
	}

	public override TreeDumperNode VisitLiteralPropertyValue(BoundLiteralPropertyValue node, object arg)
	{
		return new TreeDumperNode("literalPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", node.Value, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitIdentifierNamePropertyValue(BoundIdentifierNamePropertyValue node, object arg)
	{
		return new TreeDumperNode("identifierNamePropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", node.Value, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitMultilanguagePropertyValue(BoundMultilanguagePropertyValue node, object arg)
	{
		return new TreeDumperNode("multilanguagePropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", null, node.Value.Select((BoundMultilanguageExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitLabelPropertyValue(BoundLabelPropertyValue node, object arg)
	{
		return new TreeDumperNode("labelPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("boundLabel", null, new TreeDumperNode[1] { Visit(node.BoundLabel, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitMultilanguageExpression(BoundMultilanguageExpression node, object arg)
	{
		return new TreeDumperNode("multilanguageExpression", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("languageId", null, new TreeDumperNode[1] { Visit(node.LanguageId, null) }),
			new TreeDumperNode("languageCultureId", node.LanguageCultureId, null),
			new TreeDumperNode("languageValue", node.LanguageValue, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitLabel(BoundLabel node, object arg)
	{
		return new TreeDumperNode("label", null, new TreeDumperNode[5]
		{
			new TreeDumperNode("text", node.Text, null),
			new TreeDumperNode("comment", node.Comment, null),
			new TreeDumperNode("maxLength", node.MaxLength, null),
			new TreeDumperNode("locked", node.Locked, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitPermissionPropertyValue(BoundPermissionPropertyValue node, object arg)
	{
		return new TreeDumperNode("permissionPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", null, node.Value.Select((BoundPermissionExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitPermissionExpression(BoundPermissionExpression node, object arg)
	{
		return new TreeDumperNode("permissionExpression", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("objectType", node.ObjectType, null),
			new TreeDumperNode("target", null, new TreeDumperNode[1] { Visit(node.Target, null) }),
			new TreeDumperNode("permission", node.Permission, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node, object arg)
	{
		return new TreeDumperNode("applicationObjectReferencePropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", node.Value, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node, object arg)
	{
		return new TreeDumperNode("memberReferencePropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("memberReference", node.MemberReference, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitIdentifierListPropertyValue(BoundIdentifierListPropertyValue node, object arg)
	{
		return new TreeDumperNode("identifierListPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", null, node.Value.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitConstantValueListPropertyValue(BoundConstantValueListPropertyValue node, object arg)
	{
		return new TreeDumperNode("constantValueListPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("values", node.Values, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitListOfConstantValuePairsPropertyValue(BoundListOfConstantValuePairsPropertyValue node, object arg)
	{
		return new TreeDumperNode("listOfConstantValuePairsPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", null, node.Value.Select((BoundPairOfConstantValues x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitPairOfConstantValues(BoundPairOfConstantValues node, object arg)
	{
		return new TreeDumperNode("pairOfConstantValues", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("first", node.First, null),
			new TreeDumperNode("second", node.Second, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitEnumPropertyValue(BoundEnumPropertyValue node, object arg)
	{
		return new TreeDumperNode("enumPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", node.Value, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitOptionMembersPropertyValue(BoundOptionMembersPropertyValue node, object arg)
	{
		return new TreeDumperNode("optionMembersPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", node.Value, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableRelationPropertyValue(BoundTableRelationPropertyValue node, object arg)
	{
		return new TreeDumperNode("tableRelationPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("tableRelationExpression", null, new TreeDumperNode[1] { Visit(node.TableRelationExpression, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableRelationExpression(BoundTableRelationExpression node, object arg)
	{
		return new TreeDumperNode("tableRelationExpression", null, new TreeDumperNode[5]
		{
			new TreeDumperNode("ifExpression", null, new TreeDumperNode[1] { Visit(node.IfExpression, null) }),
			new TreeDumperNode("fieldAccess", null, new TreeDumperNode[1] { Visit(node.FieldAccess, null) }),
			new TreeDumperNode("tableFilter", null, new TreeDumperNode[1] { Visit(node.TableFilter, null) }),
			new TreeDumperNode("elseExpression", null, new TreeDumperNode[1] { Visit(node.ElseExpression, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableRelationConditionExpression(BoundTableRelationConditionExpression node, object arg)
	{
		return new TreeDumperNode("tableRelationConditionExpression", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("condition", null, node.Condition.Select((BoundEqualsPropertyExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitCalculationFormulaPropertyValue(BoundCalculationFormulaPropertyValue node, object arg)
	{
		return new TreeDumperNode("calculationFormulaPropertyValue", null, new TreeDumperNode[5]
		{
			new TreeDumperNode("@operator", node.Operator, null),
			new TreeDumperNode("isMinus", node.IsMinus, null),
			new TreeDumperNode("fieldOrTableAccess", null, new TreeDumperNode[1] { Visit(node.FieldOrTableAccess, null) }),
			new TreeDumperNode("tableFilter", null, new TreeDumperNode[1] { Visit(node.TableFilter, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitEqualsPropertyExpression(BoundEqualsPropertyExpression node, object arg)
	{
		return new TreeDumperNode("equalsPropertyExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("left", null, new TreeDumperNode[1] { Visit(node.Left, null) }),
			new TreeDumperNode("right", null, new TreeDumperNode[1] { Visit(node.Right, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitPropertyRhsExpression(BoundPropertyRhsExpression node, object arg)
	{
		return new TreeDumperNode("propertyRhsExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("@operator", node.Operator, null),
			new TreeDumperNode("value", null, new TreeDumperNode[1] { Visit(node.Value, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node, object arg)
	{
		return new TreeDumperNode("constOrFieldAccessValueExpression", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("value", null, new TreeDumperNode[1] { Visit(node.Value, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node, object arg)
	{
		return new TreeDumperNode("parenthesizedFilterExpression", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitWildCardPropertyValueExpression(BoundWildCardPropertyValueExpression node, object arg)
	{
		return new TreeDumperNode("wildCardPropertyValueExpression", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("pattern", null, new TreeDumperNode[1] { Visit(node.Pattern, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node, object arg)
	{
		return new TreeDumperNode("unaryPropertyFilterExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("operatorKind", node.OperatorKind, null),
			new TreeDumperNode("operand", null, new TreeDumperNode[1] { Visit(node.Operand, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node, object arg)
	{
		return new TreeDumperNode("binaryPropertyFilterExpression", null, new TreeDumperNode[4]
		{
			new TreeDumperNode("operatorKind", node.OperatorKind, null),
			new TreeDumperNode("left", null, new TreeDumperNode[1] { Visit(node.Left, null) }),
			new TreeDumperNode("right", null, new TreeDumperNode[1] { Visit(node.Right, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitCodeExpressionPropertyValue(BoundCodeExpressionPropertyValue node, object arg)
	{
		return new TreeDumperNode("codeExpressionPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitStyleExpressionPropertyValue(BoundStyleExpressionPropertyValue node, object arg)
	{
		return new TreeDumperNode("styleExpressionPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("expression", null, new TreeDumperNode[1] { Visit(node.Expression, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitDecimalPlacesPropertyValue(BoundDecimalPlacesPropertyValue node, object arg)
	{
		return new TreeDumperNode("decimalPlacesPropertyValue", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("minimum", node.Minimum, null),
			new TreeDumperNode("maximum", node.Maximum, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableViewPropertyValue(BoundTableViewPropertyValue node, object arg)
	{
		return new TreeDumperNode("tableViewPropertyValue", null, new TreeDumperNode[5]
		{
			new TreeDumperNode("declaringTableSymbol", node.DeclaringTableSymbol, null),
			new TreeDumperNode("sorting", null, node.Sorting.Select((BoundExpression x) => Visit(x, null))),
			new TreeDumperNode("isAscending", node.IsAscending, null),
			new TreeDumperNode("tableFilter", null, new TreeDumperNode[1] { Visit(node.TableFilter, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTableFilterStatementPropertyValue(BoundTableFilterStatementPropertyValue node, object arg)
	{
		return new TreeDumperNode("tableFilterStatementPropertyValue", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("declaringTableSymbol", node.DeclaringTableSymbol, null),
			new TreeDumperNode("tableFilter", null, new TreeDumperNode[1] { Visit(node.TableFilter, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitOrderByPropertyValue(BoundOrderByPropertyValue node, object arg)
	{
		return new TreeDumperNode("orderByPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("elements", null, node.Elements.Select((BoundOrderByExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitOrderByExpression(BoundOrderByExpression node, object arg)
	{
		return new TreeDumperNode("orderByExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("isAscending", node.IsAscending, null),
			new TreeDumperNode("value", null, new TreeDumperNode[1] { Visit(node.Value, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitDataItemLinkPropertyValue(BoundDataItemLinkPropertyValue node, object arg)
	{
		return new TreeDumperNode("dataItemLinkPropertyValue", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("joins", null, node.Joins.Select((BoundDataItemLinkExpression x) => Visit(x, null))),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitDataItemLinkExpression(BoundDataItemLinkExpression node, object arg)
	{
		return new TreeDumperNode("dataItemLinkExpression", null, new TreeDumperNode[3]
		{
			new TreeDumperNode("sourceField", null, new TreeDumperNode[1] { Visit(node.SourceField, null) }),
			new TreeDumperNode("relatedField", null, new TreeDumperNode[1] { Visit(node.RelatedField, null) }),
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitNullPropertyValue(BoundNullPropertyValue node, object arg)
	{
		return new TreeDumperNode("nullPropertyValue", null, new TreeDumperNode[1]
		{
			new TreeDumperNode("type", node.Type, null)
		});
	}

	public override TreeDumperNode VisitTemporary(BoundTemporary node, object arg)
	{
		return new TreeDumperNode("temporary", null, new TreeDumperNode[2]
		{
			new TreeDumperNode("name", node.Name, null),
			new TreeDumperNode("type", node.Type, null)
		});
	}
}
