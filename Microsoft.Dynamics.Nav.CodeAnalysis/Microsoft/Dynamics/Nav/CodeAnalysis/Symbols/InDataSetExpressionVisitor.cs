using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class InDataSetExpressionVisitor : ExpressionVisitor
{
	private readonly DiagnosticBag diagnostics;

	private readonly bool clientExpressionIsError;

	public InDataSetExpressionVisitor(bool clientExpressionIsError, DiagnosticBag diagnostics)
	{
		this.diagnostics = diagnostics;
		this.clientExpressionIsError = clientExpressionIsError;
	}

	internal override void VisitGlobal(BoundGlobal node)
	{
		CheckClientVariableType(node, diagnostics);
	}

	internal override void VisitBinaryOperator(BoundBinaryOperator node)
	{
	}

	internal override void VisitConversion(BoundConversion node)
	{
		switch (node.ConversionKind)
		{
		case ConversionKind.NoConversion:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitText:
		case ConversionKind.ToDecimal:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.Identity:
			return;
		}
		ReportInvalidClientExpression(node, clientExpressionIsError, diagnostics);
	}

	internal override void VisitLiteral(BoundLiteral node)
	{
	}

	internal override void VisitFieldAccess(BoundFieldAccess node)
	{
	}

	internal override void VisitUnaryOperator(BoundUnaryOperator node)
	{
	}

	internal override void VisitOptionAccess(BoundOptionAccess node)
	{
	}

	internal override void VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
	}

	internal override void VisitRest(BoundExpression expression)
	{
		ReportInvalidClientExpression(expression, clientExpressionIsError, diagnostics);
	}

	private static void CheckClientVariableType(BoundGlobal boundGlobal, DiagnosticBag diagnostics)
	{
		if (!boundGlobal.GlobalSymbol.Type.NavTypeKind.IsValidInDataSetType())
		{
			diagnostics.Add(ErrorCode.ERR_InvalidInDatasetDatatype, boundGlobal.Syntax.Location, boundGlobal.GlobalSymbol.Name);
		}
	}

	private static void ReportInvalidClientExpression(BoundExpression expression, bool clientExpressionIsError, DiagnosticBag diagnostics)
	{
		string text;
		switch (expression.Kind)
		{
		case BoundKind.ArrayAccess:
			text = "Array access";
			break;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			text = string.Format(CultureInfo.CurrentCulture, "Conversion from {0} to {1}", boundConversion.Operand.Type, boundConversion.TargetOpt.Type);
			break;
		}
		case BoundKind.Call:
			text = "Procedure calls";
			break;
		default:
			text = expression.Kind.ToString();
			break;
		}
		diagnostics.Add(clientExpressionIsError ? ErrorCode.ERR_InvalidInClientExpression : ErrorCode.WRN_ERR_InvalidInClientExpression, expression.Syntax.GetLocation(), text);
	}
}
