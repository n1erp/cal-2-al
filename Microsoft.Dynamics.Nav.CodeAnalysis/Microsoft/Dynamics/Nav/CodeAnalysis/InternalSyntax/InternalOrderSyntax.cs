using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOrderSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _orderToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _orderBy;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => OrderSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OrderToken => _orderToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSyntaxToken OrderBy => _orderBy;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalOrderSyntax(SyntaxKind kind, InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderToken);
		_orderToken = orderToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(orderBy);
		_orderBy = orderBy;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalOrderSyntax(SyntaxKind kind, InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderToken);
		_orderToken = orderToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(orderBy);
		_orderBy = orderBy;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalOrderSyntax(SyntaxKind kind, InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderToken);
		_orderToken = orderToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(orderBy);
		_orderBy = orderBy;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_orderToken, 
			1 => _openParenthesisToken, 
			2 => _orderBy, 
			3 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OrderSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrder(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOrder(this);
	}

	public InternalOrderSyntax Update(InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken)
	{
		if (orderToken != OrderToken || openParenthesisToken != OpenParenthesisToken || orderBy != OrderBy || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.Order(orderToken, openParenthesisToken, orderBy, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOrderSyntax(Kind, _orderToken, _openParenthesisToken, _orderBy, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOrderSyntax(Kind, _orderToken, _openParenthesisToken, _orderBy, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
