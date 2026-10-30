using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageViewSyntax : SyntaxNode
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private PageExtensionLayoutSyntax _layout;

	public SyntaxToken ViewKeyword => new SyntaxToken(this, ((InternalPageViewSyntax)Node)._viewKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageViewSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageViewSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageViewSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public PageExtensionLayoutSyntax Layout => GetRed(ref _layout, 6);

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageViewSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal PageViewSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _layout, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			5 => _propertyList, 
			6 => _layout, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageView(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageView(this);
	}

	public PageViewSyntax Update(SyntaxToken viewKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, SyntaxToken closeBraceToken)
	{
		if (viewKeyword != ViewKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || closeBraceToken != CloseBraceToken)
		{
			PageViewSyntax pageViewSyntax = SyntaxFactory.PageView(viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageViewSyntax.WithAnnotations(annotations);
			}
			return pageViewSyntax;
		}
		return this;
	}

	public PageViewSyntax WithViewKeyword(SyntaxToken viewKeyword)
	{
		return Update(viewKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ViewKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ViewKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ViewKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ViewKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ViewKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Layout, CloseBraceToken);
	}

	public PageViewSyntax WithLayout(PageExtensionLayoutSyntax layout)
	{
		return Update(ViewKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, layout, CloseBraceToken);
	}

	public PageViewSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ViewKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Layout, closeBraceToken);
	}

	public PageViewSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageViewSyntax AddLayoutChanges(params ControlChangeBaseSyntax[] items)
	{
		PageExtensionLayoutSyntax pageExtensionLayoutSyntax = Layout ?? SyntaxFactory.PageExtensionLayout();
		return WithLayout(pageExtensionLayoutSyntax.WithChanges(pageExtensionLayoutSyntax.Changes.AddRange(items)));
	}
}
