using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldGroupAddChangeSyntax : InternalFieldGroupChangeBaseSyntax
{
	internal readonly InternalSyntaxToken _changeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _anchor;

	internal readonly InternalSyntaxToken _semicolonToken1;

	internal readonly InternalSyntaxNode _fields;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => FieldGroupAddChangeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ChangeKeyword => _changeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Anchor => _anchor;

	public InternalSyntaxToken SemicolonToken1 => _semicolonToken1;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Fields => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_fields));

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalFieldGroupAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
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
			0 => _changeKeyword, 
			1 => _openParenthesisToken, 
			2 => _anchor, 
			3 => _semicolonToken1, 
			4 => _fields, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldGroupAddChangeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupAddChange(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupAddChange(this);
	}

	public InternalFieldGroupAddChangeSyntax Update(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken1 != SemicolonToken1 || fields != Fields || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.FieldGroupAddChange(changeKeyword, openParenthesisToken, anchor, semicolonToken1, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldGroupAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken1, _fields, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldGroupAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken1, _fields, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
