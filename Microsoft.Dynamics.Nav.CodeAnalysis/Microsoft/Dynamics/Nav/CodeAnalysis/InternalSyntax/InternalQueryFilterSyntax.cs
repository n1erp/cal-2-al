using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQueryFilterSyntax : InternalQueryElementSyntax
{
	internal readonly InternalSyntaxToken _filterKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalIdentifierNameSyntax _relatedField;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => QueryFilterSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken FilterKeyword => _filterKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalSyntaxToken SemicolonToken => _semicolonToken;

	public override InternalIdentifierNameSyntax RelatedField => _relatedField;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalQueryFilterSyntax(SyntaxKind kind, InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(filterKeyword);
		_filterKeyword = filterKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryFilterSyntax(SyntaxKind kind, InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(filterKeyword);
		_filterKeyword = filterKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryFilterSyntax(SyntaxKind kind, InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(filterKeyword);
		_filterKeyword = filterKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_filterKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _relatedField, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QueryFilterSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryFilter(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQueryFilter(this);
	}

	public InternalQueryFilterSyntax Update(InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (filterKeyword != FilterKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || relatedField != RelatedField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.QueryFilter(filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQueryFilterSyntax(Kind, _filterKeyword, _openParenthesisToken, _name, _semicolonToken, _relatedField, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQueryFilterSyntax(Kind, _filterKeyword, _openParenthesisToken, _name, _semicolonToken, _relatedField, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
