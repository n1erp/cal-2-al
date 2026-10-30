using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageModifications : INavDesignerPageOperations
{
	private readonly List<INavDesignerPageChange> pageChanges = new List<INavDesignerPageChange>();

	private readonly SyntaxKind syntaxKind;

	private readonly NavDesignerPageLayoutExtension layoutExtension = new NavDesignerPageLayoutExtension();

	private readonly NavDesignerPageActionListExtension actionList = new NavDesignerPageActionListExtension();

	private readonly NavDesignerPageViewListExtension viewList = new NavDesignerPageViewListExtension();

	internal PropertyListSyntax PropertyListSyntax => SyntaxFactory.PropertyList(default(SyntaxList<PropertySyntaxOrEmpty>).AddRange(pageChanges.Select((INavDesignerPageChange c) => c.Syntax)));

	internal PageExtensionLayoutSyntax LayoutSyntax => layoutExtension.LayoutSyntax;

	internal PageExtensionActionListSyntax ActionsSyntax => actionList.ActionsSyntax;

	internal PageExtensionViewListSyntax ViewsSyntax => viewList.ViewsSyntax;

	internal NavDesignerPageModifications(AbstractPageExtensionSyntax pageExtension)
	{
		syntaxKind = pageExtension.Kind;
		if (pageExtension.PropertyList != null)
		{
			foreach (PropertySyntaxOrEmpty property in pageExtension.PropertyList.Properties)
			{
				if (property.Kind == SyntaxKind.Property)
				{
					pageChanges.Add(new NavDesignerPageModify((PropertySyntax)property));
				}
			}
		}
		layoutExtension = new NavDesignerPageLayoutExtension(pageExtension.Layout);
		actionList = new NavDesignerPageActionListExtension(pageExtension.Actions);
		viewList = new NavDesignerPageViewListExtension(pageExtension.Views);
	}

	public SyntaxKind Kind()
	{
		return syntaxKind;
	}

	public bool AddPageField(string controlExpression, string uniqueControlName, string parentControlName, string relativeControlName)
	{
		return layoutExtension.AddPageField(controlExpression, uniqueControlName, parentControlName, relativeControlName);
	}

	public bool MovePageField(NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		return layoutExtension.MovePageField(controlContext, parentControlName, relativeControlName);
	}

	public bool RemovePageField(NavDesignerControlContext controlContext)
	{
		return layoutExtension.RemovePageField(controlContext);
	}

	public bool SetPageFieldProperty(string controlName, string propertyName, object propertyValue)
	{
		return layoutExtension.SetPageFieldProperty(controlName, propertyName, propertyValue);
	}

	public bool AddPageAction(string uniqueActionName, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName, bool isGroup)
	{
		return actionList.AddPageAction(uniqueActionName, actionAreaKind, parentActionName, relativeActionName, isGroup);
	}

	public bool MovePageAction(NavDesignerActionContext actionContext, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName)
	{
		return actionList.MovePageAction(actionContext, actionAreaKind, parentActionName, relativeActionName);
	}

	public bool RemovePageAction(NavDesignerActionContext actionContext)
	{
		return actionList.RemovePageAction(actionContext);
	}

	public bool SetPageActionProperty(string actionName, string propertyName, object propertyValue)
	{
		return actionList.SetPageActionProperty(actionName, propertyName, propertyValue);
	}

	public bool AddPageView(string viewName, string relativeViewName, PageExtensionLayoutSyntax layoutSyntax, string filterPropertyValue, string orderByPropertyValue)
	{
		return viewList.AddPageView(viewName, relativeViewName, layoutSyntax, filterPropertyValue, orderByPropertyValue);
	}

	public bool MovePageView(NavDesignerActionContext actionViewContext, string relativeViewName)
	{
		return viewList.MovePageView(actionViewContext, relativeViewName);
	}

	public bool RemovePageView(NavDesignerActionContext actionViewContext)
	{
		return viewList.RemovePageView(actionViewContext);
	}

	public bool SetPageViewProperty(string viewName, string propertyName, object propertyValue)
	{
		return viewList.SetPageViewProperty(viewName, propertyName, propertyValue);
	}

	public bool SetPageProperty(string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForPage(propertyName, out var propertyKind))
		{
			return false;
		}
		PropertySyntax propertySyntax = null;
		if (propertyValue != null)
		{
			propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(propertyKind, propertyValue);
		}
		NavDesignerPageModify navDesignerPageModify = pageChanges.OfType<NavDesignerPageModify>().SingleOrDefault((NavDesignerPageModify x) => SemanticFacts.IsSameName(x.PropertyName, propertyName));
		if (navDesignerPageModify != null)
		{
			if (propertySyntax != null)
			{
				if (!pageChanges.Remove(navDesignerPageModify))
				{
					return false;
				}
				pageChanges.Add(new NavDesignerPageModify(propertySyntax));
				return true;
			}
			return pageChanges.Remove(navDesignerPageModify);
		}
		if (propertySyntax != null)
		{
			pageChanges.Add(new NavDesignerPageModify(propertySyntax));
			return true;
		}
		return false;
	}

	public PageViewSyntax GetViewSyntax(string viewName)
	{
		return viewList.GetView(viewName)?.Syntax;
	}

	public bool MovePageViewField(string viewName, NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		NavDesignerPageViewAdd viewAddChange = viewList.GetViewAddChange(viewName);
		if (viewAddChange == null)
		{
			return false;
		}
		if (!viewAddChange.MoveField(viewName, controlContext, parentControlName, relativeControlName))
		{
			return false;
		}
		return true;
	}

	public bool SetPageViewFieldProperty(string viewName, string controlName, string propertyName, object propertyValue)
	{
		NavDesignerPageViewAdd viewAddChange = viewList.GetViewAddChange(viewName);
		if (viewAddChange == null)
		{
			return false;
		}
		if (!viewAddChange.SetPageFieldProperty(viewName, controlName, propertyName, propertyValue))
		{
			return false;
		}
		return true;
	}

	public void ClearViewControlChanges(string viewName)
	{
		NavDesignerPageViewAdd viewAddChange = viewList.GetViewAddChange(viewName);
		viewAddChange.ClearLayoutChanges(viewName);
	}

	public void ClearAllChanges()
	{
		ClearPageLayoutChanges();
		ClearPageActionChanges();
		ClearPagePropertyChanges();
		ClearPageViewChanges();
	}

	public void ClearControlChanges()
	{
		ClearPageLayoutChanges();
	}

	public void ClearActionChanges()
	{
		ClearPageActionChanges();
	}

	public void ClearViewChanges()
	{
		ClearPageViewChanges();
	}

	public ReadOnlyCollection<INavDesignerPageControlChange> GetPageControlChanges()
	{
		return layoutExtension.GetChanges();
	}

	public ReadOnlyCollection<INavDesignerPageActionChange> GetPageActionChanges()
	{
		return actionList.GetChanges();
	}

	public ReadOnlyCollection<INavDesignerPageViewChange> GetPageViewChanges()
	{
		return viewList.GetChanges();
	}

	public ReadOnlyCollection<INavDesignerPageChange> GetPagePropertyChanges()
	{
		return pageChanges.AsReadOnly();
	}

	private void ClearPagePropertyChanges()
	{
		List<INavDesignerPageChange> list = pageChanges;
		foreach (INavDesignerPageChange item in list)
		{
			if (!(item is NavDesignerPageModify navDesignerPageModify) || !SemanticFacts.IsSameName(navDesignerPageModify.PropertyName, "PromotedActionCategories"))
			{
				pageChanges.Remove(item);
			}
		}
	}

	private void ClearPageActionChanges()
	{
		actionList.ClearChanges();
		NavDesignerPageModify navDesignerPageModify = pageChanges.OfType<NavDesignerPageModify>().FirstOrDefault((NavDesignerPageModify x) => SemanticFacts.IsSameName(x.PropertyName, "PromotedActionCategories"));
		if (navDesignerPageModify != null)
		{
			pageChanges.Remove(navDesignerPageModify);
		}
	}

	private void ClearPageLayoutChanges()
	{
		layoutExtension.ClearChanges();
	}

	private void ClearPageViewChanges()
	{
		viewList.ClearChanges();
	}
}
