using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageLabelSyntax : ControlBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageLabelSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageLabelSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageLabelSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageLabelSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageLabelSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal PageLabelSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			5 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageLabel(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageLabel(this);
	}

	public PageLabelSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			PageLabelSyntax pageLabelSyntax = SyntaxFactory.PageLabel(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageLabelSyntax.WithAnnotations(annotations);
			}
			return pageLabelSyntax;
		}
		return this;
	}

	public PageLabelSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public PageLabelSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public PageLabelSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
