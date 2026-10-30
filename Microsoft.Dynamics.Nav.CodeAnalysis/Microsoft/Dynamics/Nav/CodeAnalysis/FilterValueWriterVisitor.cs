using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class FilterValueWriterVisitor : BoundTreeWalkerWithStackGuard
{
	private readonly bool emitBooleanAsNumeral;

	internal StringBuilder Output { get; }

	internal FilterValueWriterVisitor(bool emitBooleanAsNumeral = false)
	{
		Output = new StringBuilder();
		this.emitBooleanAsNumeral = emitBooleanAsNumeral;
	}

	internal static string CreateFilterOutput(BoundPropertyRhsExpression rhs, bool emitBooleanAsNumeral = false)
	{
		FilterValueWriterVisitor filterValueWriterVisitor = new FilterValueWriterVisitor(emitBooleanAsNumeral);
		filterValueWriterVisitor.Visit(rhs);
		return filterValueWriterVisitor.Output.ToString();
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		if (node == null || node.ConstantValue == null)
		{
			return null;
		}
		if (node.ConstantValue.IsBoolean)
		{
			Output.Append(emitBooleanAsNumeral ? node.ConstantValue.Int32Value.ToString(CultureInfo.InvariantCulture) : node.ConstantValue.BooleanValue.ToString().ToLowerInvariant());
		}
		else if (node.ConstantValue.IsString && !string.IsNullOrEmpty(node.ConstantValue.StringValue))
		{
			string text = node.ConstantValue.StringValue.UnquoteLiteral();
			Output.Append((text.Trim().Length > 0) ? text : node.ConstantValue.StringValue);
		}
		else
		{
			switch (node.Type.NavTypeKind)
			{
			case NavTypeKind.Date:
				Output.Append(ObjectDisplay.FormatDate(node.ConstantValue.DateValue));
				break;
			case NavTypeKind.Time:
				Output.Append(ObjectDisplay.FormatTime(node.ConstantValue.TimeValue));
				break;
			default:
				Output.Append(node.ConstantValue.Value);
				break;
			}
		}
		return base.VisitLiteral(node);
	}

	public override BoundNode VisitLiteralPropertyValue(BoundLiteralPropertyValue node)
	{
		if (node == null)
		{
			return null;
		}
		Output.Append(node.ValueText);
		return base.VisitLiteralPropertyValue(node);
	}

	public override BoundNode VisitConstOrFieldAccessValueExpression(BoundConstOrFieldAccessValueExpression node)
	{
		if (node == null)
		{
			return null;
		}
		return base.VisitConstOrFieldAccessValueExpression(node);
	}

	public override BoundNode VisitOptionAccess(BoundOptionAccess node)
	{
		if (node == null)
		{
			return null;
		}
		Output.Append(node.OptionSymbol.Ordinal);
		return base.VisitOptionAccess(node);
	}

	public override BoundNode VisitUnaryPropertyFilterExpression(BoundUnaryPropertyFilterExpression node)
	{
		if (node == null)
		{
			return null;
		}
		if (node.OperatorKind == UnaryOperatorKind.FilterRangeFrom)
		{
			RangeFromFilterExpressionValueSyntax rangeFromFilterExpressionValueSyntax = (RangeFromFilterExpressionValueSyntax)node.Syntax;
			SyntaxToken? syntaxToken = rangeFromFilterExpressionValueSyntax.OperatorToken;
			base.VisitUnaryPropertyFilterExpression(node);
			Output.Append(syntaxToken.Value.Text);
			return null;
		}
		if (node.Syntax is UnaryFilterExpressionValueSyntax unaryFilterExpressionValueSyntax)
		{
			SyntaxToken? syntaxToken = unaryFilterExpressionValueSyntax.OperatorToken;
			Output.Append(syntaxToken.Value.Text);
			return base.VisitUnaryPropertyFilterExpression(node);
		}
		return base.VisitUnaryPropertyFilterExpression(node);
	}

	public override BoundNode VisitBinaryPropertyFilterExpression(BoundBinaryPropertyFilterExpression node)
	{
		if (node == null)
		{
			return null;
		}
		BinaryFilterExpressionValueSyntax binaryFilterExpressionValueSyntax = (BinaryFilterExpressionValueSyntax)node.Syntax;
		Visit(node.Left);
		Output.Append(binaryFilterExpressionValueSyntax.OperatorToken.Text);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitParenthesizedFilterExpression(BoundParenthesizedFilterExpression node)
	{
		if (node == null)
		{
			return null;
		}
		ParenthesizedFilterExpressionValueSyntax parenthesizedFilterExpressionValueSyntax = (ParenthesizedFilterExpressionValueSyntax)node.Syntax;
		Output.Append(parenthesizedFilterExpressionValueSyntax.OpenParenthesisToken.Text);
		Visit(node.Expression);
		Output.Append(parenthesizedFilterExpressionValueSyntax.CloseParenthesisToken.Text);
		return null;
	}
}
