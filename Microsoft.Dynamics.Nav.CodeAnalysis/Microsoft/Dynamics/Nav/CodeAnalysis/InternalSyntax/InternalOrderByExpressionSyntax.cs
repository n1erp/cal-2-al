using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOrderByExpressionSyntax : InternalExpressionSyntax
{
	internal readonly InternalSyntaxToken _orderSpecifier;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _sortingFields;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => OrderByExpressionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OrderSpecifier => _orderSpecifier;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> SortingFields => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_sortingFields));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalOrderByExpressionSyntax(SyntaxKind kind, InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderSpecifier);
		_orderSpecifier = orderSpecifier;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (sortingFields != null)
		{
			AdjustFlagsAndWidth(sortingFields);
			_sortingFields = sortingFields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalOrderByExpressionSyntax(SyntaxKind kind, InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderSpecifier);
		_orderSpecifier = orderSpecifier;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (sortingFields != null)
		{
			AdjustFlagsAndWidth(sortingFields);
			_sortingFields = sortingFields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalOrderByExpressionSyntax(SyntaxKind kind, InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(orderSpecifier);
		_orderSpecifier = orderSpecifier;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (sortingFields != null)
		{
			AdjustFlagsAndWidth(sortingFields);
			_sortingFields = sortingFields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _orderSpecifier, 
			1 => _openParenthesisToken, 
			2 => _sortingFields, 
			3 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OrderByExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOrderByExpression(this);
	}

	public InternalOrderByExpressionSyntax Update(InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		if (orderSpecifier != OrderSpecifier || openParenthesisToken != OpenParenthesisToken || sortingFields != SortingFields || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.OrderByExpression(orderSpecifier, openParenthesisToken, sortingFields, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOrderByExpressionSyntax(Kind, _orderSpecifier, _openParenthesisToken, _sortingFields, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOrderByExpressionSyntax(Kind, _orderSpecifier, _openParenthesisToken, _sortingFields, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
