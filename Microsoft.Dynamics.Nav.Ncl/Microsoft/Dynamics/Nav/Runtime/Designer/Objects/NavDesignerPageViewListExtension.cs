using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

internal class NavDesignerPageViewListExtension
{
	private readonly List<INavDesignerPageViewChange> pageViewChanges = new List<INavDesignerPageViewChange>();

	public PageExtensionViewListSyntax ViewsSyntax => SyntaxFactory.PageExtensionViewList(default(SyntaxList<ViewChangeBaseSyntax>).AddRange(pageViewChanges.Select((INavDesignerPageViewChange c) => c.Syntax)));

	public NavDesignerPageViewListExtension()
	{
	}

	public NavDesignerPageViewListExtension(PageExtensionViewListSyntax viewListSyntax)
	{
		if (viewListSyntax == null)
		{
			return;
		}
		_ = viewListSyntax.Changes;
		foreach (ViewChangeBaseSyntax change in viewListSyntax.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ViewAddChange:
				pageViewChanges.Add(new NavDesignerPageViewAdd((ViewAddChangeSyntax)change));
				break;
			case SyntaxKind.ViewMoveChange:
				pageViewChanges.Add(new NavDesignerPageViewMove((ViewMoveChangeSyntax)change));
				break;
			case SyntaxKind.ViewModifyChange:
				pageViewChanges.Add(new NavDesignerPageViewModify((ViewModifyChangeSyntax)change));
				break;
			}
		}
	}

	internal ReadOnlyCollection<INavDesignerPageViewChange> GetChanges()
	{
		return pageViewChanges.AsReadOnly();
	}

	public NavDesignerPageViewAdd GetViewAddChange(string viewName)
	{
		foreach (INavDesignerPageViewChange pageViewChange in pageViewChanges)
		{
			if (!(pageViewChange is NavDesignerPageViewAdd navDesignerPageViewAdd))
			{
				continue;
			}
			foreach (NavDesignerPageView view in navDesignerPageViewAdd.Views)
			{
				if (SemanticFacts.IsSameName(view.ViewName, viewName))
				{
					return navDesignerPageViewAdd;
				}
			}
		}
		return null;
	}

	public NavDesignerPageView GetView(string viewName)
	{
		foreach (INavDesignerPageViewChange pageViewChange in pageViewChanges)
		{
			if (!(pageViewChange is NavDesignerPageViewAdd navDesignerPageViewAdd))
			{
				continue;
			}
			foreach (NavDesignerPageView view in navDesignerPageViewAdd.Views)
			{
				if (SemanticFacts.IsSameName(view.ViewName, viewName))
				{
					return view;
				}
			}
		}
		return null;
	}

	public bool AddPageView(string viewName, string relativeViewName, PageExtensionLayoutSyntax layoutSyntax, string filterPropertyValue, string orderByPropertyValue)
	{
		PropertyListSyntax propertyListSyntax = SyntaxFactory.PropertyList();
		if (!string.IsNullOrEmpty(filterPropertyValue))
		{
			propertyListSyntax = propertyListSyntax.AddProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.Filters, filterPropertyValue));
		}
		if (!string.IsNullOrEmpty(orderByPropertyValue))
		{
			propertyListSyntax = propertyListSyntax.AddProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.OrderBy, orderByPropertyValue));
		}
		NavDesignerPageView navDesignerPageView = new NavDesignerPageView(viewName, propertyListSyntax);
		if (layoutSyntax != null)
		{
			navDesignerPageView.Layout = new NavDesignerPageLayoutExtension(layoutSyntax);
		}
		return AddPageView(navDesignerPageView, relativeViewName);
	}

	private bool AddPageView(NavDesignerPageView pageView, string relativeViewName)
	{
		if (!string.IsNullOrEmpty(relativeViewName))
		{
			NavDesignerPageViewAdd navDesignerPageViewAdd = pageViewChanges.OfType<NavDesignerPageViewAdd>().LastOrDefault((NavDesignerPageViewAdd a) => !string.IsNullOrEmpty(a.AnchorViewName) && SemanticFacts.IsSameName(a.AnchorViewName, relativeViewName) && a.SyntaxKind.Equals(SyntaxKind.AddAfterKeyword));
			if (navDesignerPageViewAdd != null)
			{
				navDesignerPageViewAdd.Views = navDesignerPageViewAdd.Views.Insert(0, pageView);
				return true;
			}
			navDesignerPageViewAdd = pageViewChanges.OfType<NavDesignerPageViewAdd>().SingleOrDefault((NavDesignerPageViewAdd a) => a.Views.OfType<NavDesignerPageView>().SingleOrDefault((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, relativeViewName)) != null);
			if (navDesignerPageViewAdd != null)
			{
				int num = navDesignerPageViewAdd.Views.FindIndex((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, relativeViewName));
				navDesignerPageViewAdd.Views = navDesignerPageViewAdd.Views.Insert(num + 1, pageView);
				return true;
			}
		}
		else
		{
			NavDesignerPageViewAdd navDesignerPageViewAdd2 = pageViewChanges.OfType<NavDesignerPageViewAdd>().LastOrDefault((NavDesignerPageViewAdd a) => a.SyntaxKind.Equals(SyntaxKind.AddFirstKeyword));
			if (navDesignerPageViewAdd2 != null)
			{
				navDesignerPageViewAdd2.Views = navDesignerPageViewAdd2.Views.Insert(0, pageView);
				return true;
			}
		}
		pageViewChanges.Add(new NavDesignerPageViewAdd(pageView, relativeViewName));
		return true;
	}

	public bool MovePageView(NavDesignerActionContext actionViewContext, string relativeViewName)
	{
		AdjustChangesForViewMoveOrRemove(actionViewContext);
		string viewName = actionViewContext.GetActionALIdentifier();
		NavDesignerPageViewMove navDesignerPageViewMove = pageViewChanges.OfType<NavDesignerPageViewMove>().SingleOrDefault((NavDesignerPageViewMove c) => SemanticFacts.IsSameName(c.ViewName, viewName));
		if (navDesignerPageViewMove != null)
		{
			pageViewChanges.Remove(navDesignerPageViewMove);
			if (!string.IsNullOrEmpty(relativeViewName))
			{
				navDesignerPageViewMove.AnchorViewName = relativeViewName;
			}
			pageViewChanges.Add(navDesignerPageViewMove);
			return true;
		}
		NavDesignerPageViewAdd navDesignerPageViewAdd = pageViewChanges.OfType<NavDesignerPageViewAdd>().SingleOrDefault((NavDesignerPageViewAdd a) => a.Views.SingleOrDefault((NavDesignerPageView c) => c.ViewName != null && SemanticFacts.IsSameName(c.ViewName, viewName)) != null);
		if (navDesignerPageViewAdd != null)
		{
			NavDesignerPageView pageView = navDesignerPageViewAdd.Views.Single((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, viewName));
			RemovePageView(actionViewContext, hideIfNotAdded: false);
			return AddPageView(pageView, relativeViewName);
		}
		pageViewChanges.Add(new NavDesignerPageViewMove(viewName, relativeViewName));
		return true;
	}

	public bool RemovePageView(NavDesignerActionContext actionViewContext)
	{
		return RemovePageView(actionViewContext, hideIfNotAdded: true);
	}

	private bool RemovePageView(NavDesignerActionContext actionViewContext, bool hideIfNotAdded)
	{
		string viewName = actionViewContext.GetActionALIdentifier();
		NavDesignerPageViewAdd navDesignerPageViewAdd = pageViewChanges.OfType<NavDesignerPageViewAdd>().SingleOrDefault((NavDesignerPageViewAdd a) => a.Views.SingleOrDefault((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, viewName)) != null);
		if (navDesignerPageViewAdd != null)
		{
			AdjustChangesForViewMoveOrRemove(actionViewContext);
			if (navDesignerPageViewAdd.Views.Count == 1)
			{
				pageViewChanges.Remove(navDesignerPageViewAdd);
			}
			else
			{
				NavDesignerPageView value = navDesignerPageViewAdd.Views.Single((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, viewName));
				navDesignerPageViewAdd.Views = navDesignerPageViewAdd.Views.Remove(value);
			}
			return true;
		}
		if (hideIfNotAdded)
		{
			return SetPageViewProperty(viewName, "Visible", false);
		}
		return true;
	}

	public bool SetPageViewProperty(string viewName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForView(propertyName, out var propertyKind))
		{
			return false;
		}
		PropertySyntax propertySyntax = null;
		if (propertyValue != null)
		{
			propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(propertyKind, propertyValue);
		}
		NavDesignerPageViewModify viewModify = GetViewModify(viewName);
		NavDesignerPageViewAdd viewAdd = GetViewAdd(viewName);
		if (viewModify != null)
		{
			if (propertySyntax != null)
			{
				return viewModify.SetProperty(propertyKind, propertySyntax);
			}
			if (!viewModify.RemoveProperty(propertyKind))
			{
				return false;
			}
			if (!viewModify.HasChanges)
			{
				return pageViewChanges.Remove(viewModify);
			}
			return true;
		}
		if (viewAdd != null)
		{
			if (propertySyntax != null)
			{
				return viewAdd.SetProperty(viewName, propertyKind, propertySyntax);
			}
			return viewAdd.RemoveProperty(viewName, propertyKind);
		}
		if (propertySyntax != null)
		{
			pageViewChanges.Add(new NavDesignerPageViewModify(viewName, propertySyntax));
			return true;
		}
		return false;
	}

	public void ClearChanges()
	{
		pageViewChanges.Clear();
	}

	private void AdjustChangesForViewMoveOrRemove(NavDesignerActionContext actionViewContext)
	{
		foreach (INavDesignerPageViewChange pageViewChange in pageViewChanges)
		{
			if (pageViewChange is INavDesignerPageViewPositionalChange navDesignerPageViewPositionalChange && !string.IsNullOrEmpty(navDesignerPageViewPositionalChange.AnchorViewName) && SemanticFacts.IsSameName(navDesignerPageViewPositionalChange.AnchorViewName, actionViewContext.GetActionALIdentifier()))
			{
				if (pageViewChange.SyntaxKind == SyntaxKind.AddBeforeKeyword && actionViewContext.NextAction != null)
				{
					navDesignerPageViewPositionalChange.AnchorViewName = GetALNextViewName(actionViewContext);
				}
				else if (pageViewChange.SyntaxKind != SyntaxKind.AddBeforeKeyword && actionViewContext.PreviousAction != null)
				{
					navDesignerPageViewPositionalChange.AnchorViewName = GetALPreviousViewName(actionViewContext);
				}
			}
		}
	}

	private static string GetALPreviousViewName(NavDesignerActionContext actionViewContext)
	{
		return NavDesignerManagementTasks.GetALViewName(actionViewContext.GetActionALIdentifier(actionViewContext.PreviousAction));
	}

	private static string GetALNextViewName(NavDesignerActionContext actionViewContext)
	{
		return NavDesignerManagementTasks.GetALViewName(actionViewContext.GetActionALIdentifier(actionViewContext.NextAction));
	}

	private NavDesignerPageViewModify GetViewModify(string viewName)
	{
		return pageViewChanges.OfType<NavDesignerPageViewModify>().FirstOrDefault((NavDesignerPageViewModify c) => SemanticFacts.IsSameName(c.ViewName, viewName));
	}

	private NavDesignerPageViewAdd GetViewAdd(string viewName)
	{
		return pageViewChanges.OfType<NavDesignerPageViewAdd>().SingleOrDefault((NavDesignerPageViewAdd a) => a.Views.SingleOrDefault((NavDesignerPageView c) => SemanticFacts.IsSameName(c.ViewName, viewName)) != null);
	}
}
