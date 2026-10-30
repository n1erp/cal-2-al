using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSortingSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalSyntaxToken _sortingToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxNode _sortingFields;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => SortingSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken SortingToken => _sortingToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> SortingFields => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_sortingFields));

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalSortingSyntax(SyntaxKind kind, InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(sortingToken);
		_sortingToken = sortingToken;
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

	internal InternalSortingSyntax(SyntaxKind kind, InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(sortingToken);
		_sortingToken = sortingToken;
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

	internal InternalSortingSyntax(SyntaxKind kind, InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxNode sortingFields, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(sortingToken);
		_sortingToken = sortingToken;
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
			0 => _sortingToken, 
			1 => _openParenthesisToken, 
			2 => _sortingFields, 
			3 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SortingSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSorting(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSorting(this);
	}

	public InternalSortingSyntax Update(InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		if (sortingToken != SortingToken || openParenthesisToken != OpenParenthesisToken || sortingFields != SortingFields || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.Sorting(sortingToken, openParenthesisToken, sortingFields, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSortingSyntax(Kind, _sortingToken, _openParenthesisToken, _sortingFields, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSortingSyntax(Kind, _sortingToken, _openParenthesisToken, _sortingFields, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
