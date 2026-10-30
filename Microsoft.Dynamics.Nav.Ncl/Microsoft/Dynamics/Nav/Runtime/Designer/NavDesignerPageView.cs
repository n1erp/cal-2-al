using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageView
{
	private PageViewSyntax ViewSyntax { get; set; }

	internal NavDesignerPageLayoutExtension Layout { get; set; }

	internal string ViewName { get; }

	internal PageViewSyntax Syntax => GetSyntax();

	internal NavDesignerPageView(string viewName)
	{
		ViewName = viewName;
		ViewSyntax = SyntaxFactory.PageView(viewName);
		Layout = new NavDesignerPageLayoutExtension();
	}

	internal NavDesignerPageView(string viewName, PropertyListSyntax propertyList)
	{
		ViewName = viewName;
		ViewSyntax = SyntaxFactory.PageView(SyntaxFactory.IdentifierName(viewName), propertyList, null);
		Layout = new NavDesignerPageLayoutExtension();
	}

	internal NavDesignerPageView(PageViewSyntax viewSyntax)
	{
		ViewSyntax = viewSyntax;
		ViewName = viewSyntax?.Name?.Identifier.Text?.UnquoteIdentifier();
		Layout = new NavDesignerPageLayoutExtension(viewSyntax.Layout);
	}

	private PageViewSyntax GetSyntax()
	{
		PageViewSyntax pageViewSyntax = ViewSyntax;
		PageExtensionLayoutSyntax layoutSyntax = Layout.LayoutSyntax;
		if (layoutSyntax != null && layoutSyntax.Changes.Count > 0)
		{
			pageViewSyntax = pageViewSyntax.WithLayout(Layout.LayoutSyntax);
		}
		return pageViewSyntax;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((NavDesignerPageView)obj);
	}

	public bool Equals(NavDesignerPageView other)
	{
		return string.Equals(ViewName, other.ViewName);
	}

	public override int GetHashCode()
	{
		return ViewName?.GetHashCode() ?? 0;
	}

	public static bool operator ==(NavDesignerPageView left, NavDesignerPageView right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(NavDesignerPageView left, NavDesignerPageView right)
	{
		return !object.Equals(left, right);
	}
}
