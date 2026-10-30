using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldGroupAddChangeSyntax : FieldGroupChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _fields;

	private PropertyListSyntax _propertyList;

	public SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public SyntaxToken SemicolonToken1 => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._semicolonToken1, GetChildPosition(3), GetChildIndex(3));

	public SeparatedSyntaxList<IdentifierNameSyntax> Fields
	{
		get
		{
			SyntaxNode red = GetRed(ref _fields, 4);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(4));
			}
			return default;
		}
	}

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldGroupAddChangeSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal FieldGroupAddChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			4 => GetRed(ref _fields, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			4 => _fields, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupAddChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupAddChange(this);
	}

	public FieldGroupAddChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken1, SeparatedSyntaxList<IdentifierNameSyntax> fields, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken1 != SemicolonToken1 || fields != Fields || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			FieldGroupAddChangeSyntax fieldGroupAddChangeSyntax = SyntaxFactory.FieldGroupAddChange(changeKeyword, openParenthesisToken, anchor, semicolonToken1, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldGroupAddChangeSyntax.WithAnnotations(annotations);
			}
			return fieldGroupAddChangeSyntax;
		}
		return this;
	}

	public FieldGroupAddChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, SemicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, SemicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithSemicolonToken1(SyntaxToken semicolonToken1)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, semicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithFields(SeparatedSyntaxList<IdentifierNameSyntax> fields)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, Fields, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, Fields, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public FieldGroupAddChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken1, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public FieldGroupAddChangeSyntax AddFields(params IdentifierNameSyntax[] items)
	{
		return WithFields(Fields.AddRange(items));
	}

	public FieldGroupAddChangeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
