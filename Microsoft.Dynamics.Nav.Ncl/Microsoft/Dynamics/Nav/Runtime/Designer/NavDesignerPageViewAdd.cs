using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageViewAdd : INavDesignerPageViewPositionalChange, INavDesignerPageViewChange
{
	private ViewAddChangeSyntax viewAddChangeSyntax;

	public string AnchorViewName
	{
		get
		{
			if (viewAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddAfterKeyword || viewAddChangeSyntax.ChangeKeyword.Kind == SyntaxKind.AddBeforeKeyword)
			{
				return viewAddChangeSyntax.Anchor.Identifier.ValueText.UnquoteIdentifier();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				switch (viewAddChangeSyntax.ChangeKeyword.Kind)
				{
				case SyntaxKind.AddFirstKeyword:
				case SyntaxKind.AddAfterKeyword:
					viewAddChangeSyntax = viewAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				case SyntaxKind.AddLastKeyword:
				case SyntaxKind.AddBeforeKeyword:
					viewAddChangeSyntax = viewAddChangeSyntax.WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddBeforeKeyword)).WithAnchor(SyntaxFactory.IdentifierName(value));
					break;
				}
			}
		}
	}

	internal ImmutableList<NavDesignerPageView> Views
	{
		get
		{
			return viewAddChangeSyntax.Views.Select((PageViewSyntax c) => NavDesignerPageFactory.CreatePageView(c)).ToImmutableList();
		}
		set
		{
			viewAddChangeSyntax = viewAddChangeSyntax.WithViews(default(SyntaxList<PageViewSyntax>).AddRange(value.Select((NavDesignerPageView c) => c.Syntax)));
		}
	}

	public SyntaxKind SyntaxKind => viewAddChangeSyntax.ChangeKeyword.Kind;

	public ViewChangeBaseSyntax Syntax => viewAddChangeSyntax;

	internal NavDesignerPageViewAdd(NavDesignerPageView newView, string previousViewName = null)
		: this(new NavDesignerPageView[1] { newView }, previousViewName)
	{
	}

	internal NavDesignerPageViewAdd(IEnumerable<NavDesignerPageView> viewList, string previousViewName = null)
	{
		SyntaxList<PageViewSyntax> views = default(SyntaxList<PageViewSyntax>).AddRange(viewList.Select((NavDesignerPageView c) => c.Syntax));
		if (!string.IsNullOrEmpty(previousViewName))
		{
			viewAddChangeSyntax = SyntaxFactory.ViewAddChange(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword), SyntaxFactory.Token(SyntaxKind.OpenParenToken), SyntaxFactory.IdentifierName(previousViewName), SyntaxFactory.Token(SyntaxKind.CloseParenToken), SyntaxFactory.Token(SyntaxKind.OpenBraceToken), views, SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
		}
		else
		{
			viewAddChangeSyntax = SyntaxFactory.ViewAddChange(SyntaxFactory.Token(SyntaxKind.AddFirstKeyword), null, views);
		}
	}

	internal NavDesignerPageViewAdd(ViewAddChangeSyntax addSyntax)
	{
		viewAddChangeSyntax = addSyntax;
	}

	internal bool SetProperty(string viewName, PropertyKind propertyKind, PropertySyntax property)
	{
		NavDesignerPageView navDesignerPageView = Views.Single((NavDesignerPageView c) => c.ViewName.Equals(viewName));
		PropertySyntax property2 = GetProperty(navDesignerPageView, propertyKind);
		PropertyListSyntax propertyListSyntax;
		if (property2 != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageView.Syntax.PropertyList.Properties.Replace(property2, property);
			propertyListSyntax = navDesignerPageView.Syntax.PropertyList.Update(properties);
		}
		else
		{
			propertyListSyntax = navDesignerPageView.Syntax.PropertyList.AddProperties(property);
		}
		NavDesignerPageView newValue = NavDesignerPageFactory.CreatePageView(navDesignerPageView.Syntax, propertyListSyntax);
		Views = Views.Replace(navDesignerPageView, newValue);
		return true;
	}

	internal bool RemoveProperty(string viewName, PropertyKind propertyKind)
	{
		NavDesignerPageView navDesignerPageView = Views.Single((NavDesignerPageView c) => c.ViewName.Equals(viewName));
		PropertySyntax property = GetProperty(navDesignerPageView, propertyKind);
		if (property != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = navDesignerPageView.Syntax.PropertyList.Properties.Remove(property);
			PropertyListSyntax propertyListSyntax = navDesignerPageView.Syntax.PropertyList.Update(properties);
			NavDesignerPageView newValue = NavDesignerPageFactory.CreatePageView(navDesignerPageView.Syntax, propertyListSyntax);
			Views = Views.Replace(navDesignerPageView, newValue);
			return true;
		}
		return false;
	}

	internal bool MoveField(string viewName, NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		NavDesignerPageView navDesignerPageView = Views.Single((NavDesignerPageView c) => c.ViewName.Equals(viewName));
		if (!navDesignerPageView.Layout.MovePageField(controlContext, parentControlName, relativeControlName))
		{
			return false;
		}
		NavDesignerPageView newValue = new NavDesignerPageView(navDesignerPageView.Syntax);
		Views = Views.Replace(navDesignerPageView, newValue);
		return true;
	}

	internal bool SetPageFieldProperty(string viewName, string controlName, string propertyName, object propertyValue)
	{
		NavDesignerPageView navDesignerPageView = Views.Single((NavDesignerPageView c) => c.ViewName.Equals(viewName));
		if (!navDesignerPageView.Layout.SetPageFieldProperty(controlName, propertyName, propertyValue))
		{
			return false;
		}
		NavDesignerPageView newValue = new NavDesignerPageView(navDesignerPageView.Syntax);
		Views = Views.Replace(navDesignerPageView, newValue);
		return true;
	}

	internal void ClearLayoutChanges(string viewName)
	{
		NavDesignerPageView navDesignerPageView = Views.Single((NavDesignerPageView c) => c.ViewName.Equals(viewName));
		NavDesignerPageView newValue = new NavDesignerPageView(navDesignerPageView.Syntax.WithLayout(null));
		Views = Views.Replace(navDesignerPageView, newValue);
	}

	private static PropertySyntax GetProperty(NavDesignerPageView view, PropertyKind propertyKind)
	{
		return view.Syntax.GetProperty(propertyKind.ToString());
	}
}
