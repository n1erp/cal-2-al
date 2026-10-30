using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ClientSideExpressionGlobalsRewriter : BoundTreeWalkerWithStackGuard, IDisposable
{
	private readonly PooledStringBuilder builder;

	private ClientSideExpressionGlobalsRewriter()
	{
		builder = PooledStringBuilder.GetInstance();
	}

	public void Dispose()
	{
		builder?.Free();
	}

	public override BoundNode VisitGlobal(BoundGlobal node)
	{
		string value = PageMetadataEmitHelper.GetPrefixedExpressionName(node.GlobalSymbol.Name, node.ExpressionSymbol).QuoteIdentifierIfNeeded();
		builder.Builder.Append(value);
		return base.VisitGlobal(node);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		builder.Builder.Append(node.Syntax);
		return null;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		Visit(node.Left);
		builder.Builder.Append(" ");
		BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)node.Syntax;
		builder.Builder.Append(binaryExpressionSyntax.OperatorToken.ToString());
		builder.Builder.Append(" ");
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitOptionAccess(BoundOptionAccess node)
	{
		Visit(node.ReceiverOpt);
		OptionAccessExpressionSyntax optionAccessExpressionSyntax = (OptionAccessExpressionSyntax)node.Syntax;
		builder.Builder.Append(optionAccessExpressionSyntax.ColonColonToken);
		builder.Builder.Append(optionAccessExpressionSyntax.Name);
		return null;
	}

	public override BoundNode VisitParenthesizedExpression(BoundParenthesizedExpression node)
	{
		ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = (ParenthesizedExpressionSyntax)node.Syntax;
		builder.Builder.Append(parenthesizedExpressionSyntax.OpenParenthesisToken);
		builder.Builder.Append(" ");
		Visit(node.Expression);
		builder.Builder.Append(" ");
		builder.Builder.Append(parenthesizedExpressionSyntax.CloseParenthesisToken);
		return null;
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		UnaryExpressionSyntax unaryExpressionSyntax = (UnaryExpressionSyntax)node.Syntax;
		builder.Builder.Append(unaryExpressionSyntax.OperatorToken);
		builder.Builder.Append(" ");
		return base.VisitUnaryOperator(node);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		builder.Builder.Append(node.Syntax);
		return base.VisitLiteral(node);
	}

	public override string ToString()
	{
		return builder.Builder.ToString();
	}

	internal static string RewrittenSyntax(BoundProperty property)
	{
		using ClientSideExpressionGlobalsRewriter clientSideExpressionGlobalsRewriter = new ClientSideExpressionGlobalsRewriter();
		clientSideExpressionGlobalsRewriter.Visit(property.PropertyValue);
		return clientSideExpressionGlobalsRewriter.ToString();
	}
}
