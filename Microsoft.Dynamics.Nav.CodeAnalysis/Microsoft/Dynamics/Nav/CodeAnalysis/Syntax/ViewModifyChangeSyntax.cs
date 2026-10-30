using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ViewModifyChangeSyntax : ViewChangeBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalViewModifyChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalViewModifyChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalViewModifyChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalViewModifyChangeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalViewModifyChangeSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal ViewModifyChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitViewModifyChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitViewModifyChange(this);
	}

	public ViewModifyChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			ViewModifyChangeSyntax viewModifyChangeSyntax = SyntaxFactory.ViewModifyChange(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return viewModifyChangeSyntax.WithAnnotations(annotations);
			}
			return viewModifyChangeSyntax;
		}
		return this;
	}

	public ViewModifyChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public ViewModifyChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public ViewModifyChangeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
