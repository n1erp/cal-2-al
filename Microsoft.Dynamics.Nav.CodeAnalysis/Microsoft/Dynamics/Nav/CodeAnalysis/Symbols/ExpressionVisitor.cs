namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ExpressionVisitor
{
	internal void Visit(BoundExpression expression)
	{
		switch (expression.Kind)
		{
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)expression;
			VisitBinaryOperator(boundBinaryOperator);
			Visit(boundBinaryOperator.Left);
			Visit(boundBinaryOperator.Right);
			break;
		}
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			VisitConversion(boundConversion);
			Visit(boundConversion.Operand);
			break;
		}
		case BoundKind.FieldAccess:
			VisitFieldAccess((BoundFieldAccess)expression);
			break;
		case BoundKind.Global:
			VisitGlobal((BoundGlobal)expression);
			break;
		case BoundKind.Literal:
			VisitLiteral((BoundLiteral)expression);
			break;
		case BoundKind.OptionAccess:
			VisitOptionAccess((BoundOptionAccess)expression);
			break;
		case BoundKind.ParenthesizedExpression:
		{
			BoundParenthesizedExpression boundParenthesizedExpression = (BoundParenthesizedExpression)expression;
			VisitParenthesizedExpression(boundParenthesizedExpression);
			Visit(boundParenthesizedExpression.Expression);
			break;
		}
		case BoundKind.UnaryOperator:
		{
			BoundUnaryOperator boundUnaryOperator = (BoundUnaryOperator)expression;
			VisitUnaryOperator(boundUnaryOperator);
			Visit(boundUnaryOperator.Operand);
			break;
		}
		default:
			VisitRest(expression);
			break;
		}
	}

	internal virtual void VisitGlobal(BoundGlobal node)
	{
		VisitRest(node);
	}

	internal virtual void VisitBinaryOperator(BoundBinaryOperator node)
	{
		VisitRest(node);
	}

	internal virtual void VisitUnaryOperator(BoundUnaryOperator node)
	{
		VisitRest(node);
	}

	internal virtual void VisitConversion(BoundConversion node)
	{
		VisitRest(node);
	}

	internal virtual void VisitFieldAccess(BoundFieldAccess node)
	{
		VisitRest(node);
	}

	internal virtual void VisitOptionAccess(BoundOptionAccess node)
	{
		VisitRest(node);
	}

	internal virtual void VisitLiteral(BoundLiteral node)
	{
		VisitRest(node);
	}

	internal virtual void VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
		VisitRest(node);
	}

	internal virtual void VisitRest(BoundExpression expression)
	{
	}
}
