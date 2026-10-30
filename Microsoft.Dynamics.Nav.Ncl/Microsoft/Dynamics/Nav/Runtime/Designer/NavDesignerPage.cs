using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPage : NavDesignerApplicationObject, INavDesignerPageOperations
{
	private PageSyntax pageSyntax;

	internal override ObjectSyntax Syntax => pageSyntax;

	public SyntaxKind Kind()
	{
		return SyntaxKind.PageObject;
	}

	protected NavDesignerPage(PageSyntax pageSyntax, int id, string name)
		: base(new ApplicationObjectId(ObjectType.Page, id), name)
	{
		this.pageSyntax = pageSyntax;
	}

	public static NavDesignerPage Create(int id, string objectName, string pageType = null, string sourceTable = null)
	{
		PageSyntax pageSyntax = SyntaxFactory.Page(SyntaxFactory.ObjectId(id), objectName);
		if (!string.IsNullOrWhiteSpace(pageType))
		{
			pageSyntax = pageSyntax.AddPropertyListProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.PageType, pageType));
		}
		if (!string.IsNullOrWhiteSpace(sourceTable))
		{
			pageSyntax = pageSyntax.AddPropertyListProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.SourceTable, sourceTable));
		}
		return new NavDesignerPage(pageSyntax, id, objectName);
	}

	public static NavDesignerPage Create(PageSyntax pageSyntax)
	{
		int objectId = pageSyntax.GetObjectId();
		string objectName = pageSyntax.GetObjectName();
		return new NavDesignerPage(pageSyntax, objectId, objectName);
	}

	public bool SetPageProperty(string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForPage(propertyName, out var propertyKind))
		{
			return false;
		}
		pageSyntax = NavDesignerSyntaxHelper.UpdatePropertyList(pageSyntax, propertyKind, propertyValue);
		return true;
	}

	public void AddPageControlGroup(string groupName)
	{
		pageSyntax = AddGroupToLayoutArea(pageSyntax, groupName, AreaKind.Content);
	}

	private static PageSyntax AddGroupToLayoutArea(PageSyntax pageSyntax, string groupName, AreaKind areaKind)
	{
		SyntaxToken controlKeyword = SyntaxFactory.Token(SyntaxKind.PageGroupKeyword);
		IdentifierNameSyntax name = SyntaxFactory.IdentifierName(groupName);
		PageGroupSyntax pageGroupSyntax = SyntaxFactory.PageGroup(controlKeyword, name);
		pageSyntax = pageSyntax.GetOrCreatePageArea(areaKind, out var pageAreaSyntax);
		PageAreaSyntax newNode = pageAreaSyntax.AddControls(pageGroupSyntax);
		return pageSyntax.ReplaceNode(pageAreaSyntax, newNode);
	}

	public bool AddPageField(string controlExpression, string uniqueControlName, string parentControlName, string relativeControlName)
	{
		if (pageSyntax.GetAnyControl(uniqueControlName, allowArea: true) != null)
		{
			return false;
		}
		PropertySyntax propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.ApplicationArea, "All");
		IdentifierNameSyntax expression = SyntaxFactory.IdentifierName(controlExpression);
		PageFieldSyntax field = SyntaxFactory.PageField(uniqueControlName, expression).AddPropertyListProperties(propertySyntax);
		return AddPageField(field, parentControlName, relativeControlName);
	}

	private bool AddPageField(PageFieldSyntax field, string parentControlName, string relativeControlName)
	{
		if (!string.IsNullOrEmpty(parentControlName))
		{
			PageGroupSyntax pageGroupSyntax = pageSyntax.GetGroup(parentControlName);
			if (pageGroupSyntax == null)
			{
				return false;
			}
			if (!AddFieldToControlList(pageGroupSyntax.Controls, field, relativeControlName, out var newControlList))
			{
				return false;
			}
			pageSyntax = pageSyntax.ReplaceNode(pageGroupSyntax, pageGroupSyntax.WithControls(newControlList));
		}
		else
		{
			pageSyntax = pageSyntax.GetOrCreatePageArea(AreaKind.Content, out var pageAreaSyntax);
			if (!AddFieldToControlList(pageAreaSyntax.Controls, field, relativeControlName, out var newControlList2))
			{
				return false;
			}
			PageAreaSyntax newNode = pageAreaSyntax.WithControls(newControlList2);
			pageSyntax = pageSyntax.ReplaceNode(pageAreaSyntax, newNode);
		}
		return true;
	}

	private static bool AddFieldToControlList(SyntaxList<ControlBaseSyntax> controlList, PageFieldSyntax field, string relativeControlName, out SyntaxList<ControlBaseSyntax> newControlList)
	{
		if (!string.IsNullOrEmpty(relativeControlName))
		{
			PageFieldSyntax pageFieldSyntax = NavDesignerSyntaxHelper.FindControlInList<PageFieldSyntax>(controlList, relativeControlName);
			if (pageFieldSyntax != null)
			{
				int num = controlList.IndexOf(pageFieldSyntax);
				if (num != -1)
				{
					newControlList = controlList.Insert(num + 1, field);
					return true;
				}
			}
			newControlList = controlList;
			return false;
		}
		newControlList = controlList.Insert(0, field);
		return true;
	}

	public bool MovePageField(NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		PageFieldSyntax field = pageSyntax.GetField(controlContext.GetControlALIdentifier());
		if (field == null)
		{
			return false;
		}
		if (RemovePageField(controlContext))
		{
			return AddPageField(field, parentControlName, relativeControlName);
		}
		return false;
	}

	public bool RemovePageField(NavDesignerControlContext controlContext)
	{
		ControlBaseSyntax anyControl = pageSyntax.GetAnyControl(controlContext.GetControlALIdentifier(), allowArea: false);
		if (anyControl != null)
		{
			pageSyntax = pageSyntax.ReplaceNode(anyControl, null);
			return true;
		}
		return false;
	}

	public bool SetPageFieldProperty(string controlName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForField(propertyName, out var propertyKind))
		{
			return false;
		}
		PageFieldSyntax field = pageSyntax.GetField(controlName);
		if (field != null)
		{
			PageFieldSyntax newNode = NavDesignerSyntaxHelper.UpdatePropertyList(field, propertyKind, propertyValue);
			pageSyntax = pageSyntax.ReplaceNode(field, newNode);
			return true;
		}
		PageGroupSyntax pageGroupSyntax = pageSyntax.GetGroup(controlName);
		if (pageGroupSyntax != null)
		{
			PageGroupSyntax newNode2 = NavDesignerSyntaxHelper.UpdatePropertyList(pageGroupSyntax, propertyKind, propertyValue);
			pageSyntax = pageSyntax.ReplaceNode(pageGroupSyntax, newNode2);
			return true;
		}
		return false;
	}

	public bool AddPageAction(string uniqueActionName, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName, bool isGroup)
	{
		if (pageSyntax.GetAnyAction(uniqueActionName, allowArea: true) != null)
		{
			return false;
		}
		ActionBaseSyntax action;
		if (isGroup)
		{
			action = SyntaxFactory.PageActionGroup(uniqueActionName);
		}
		else
		{
			PropertySyntax propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.ApplicationArea, "All");
			action = SyntaxFactory.PageAction(uniqueActionName).AddPropertyListProperties(propertySyntax);
		}
		return AddPageAction(action, actionAreaKind, parentActionName, relativeActionName);
	}

	private bool AddPageAction(ActionBaseSyntax action, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName)
	{
		if (!string.IsNullOrEmpty(parentActionName))
		{
			PageActionGroupSyntax actionGroup = pageSyntax.GetActionGroup(parentActionName);
			if (actionGroup == null)
			{
				return false;
			}
			if (!AddActionToActionList(actionGroup.Actions, action, relativeActionName, out var newActionList))
			{
				return false;
			}
			pageSyntax = pageSyntax.ReplaceNode(actionGroup, actionGroup.WithActions(newActionList));
		}
		else
		{
			pageSyntax = pageSyntax.GetOrCreatePageActionArea(actionAreaKind, out var actionAreaSyntax);
			if (!AddActionToActionList(actionAreaSyntax.Actions, action, relativeActionName, out var newActionList2))
			{
				return false;
			}
			PageActionAreaSyntax newActionArea = actionAreaSyntax.WithActions(newActionList2);
			PageActionListSyntax actions = pageSyntax.Actions;
			SyntaxList<PageActionAreaSyntax> areas = ReplaceAreaInList(actions.Areas, actionAreaKind, newActionArea);
			PageActionListSyntax newNode = pageSyntax.Actions.WithAreas(areas);
			pageSyntax = pageSyntax.ReplaceNode(actions, newNode);
		}
		return true;
	}

	private static SyntaxList<PageActionAreaSyntax> ReplaceAreaInList(SyntaxList<PageActionAreaSyntax> areaList, ActionAreaKind actionAreaKind, PageActionAreaSyntax newActionArea)
	{
		PageActionAreaSyntax pageActionAreaSyntax = NavDesignerSyntaxHelper.FindAreaInList(areaList, actionAreaKind);
		if (pageActionAreaSyntax != null)
		{
			areaList = areaList.Remove(pageActionAreaSyntax);
			areaList = areaList.Add(newActionArea);
		}
		return areaList;
	}

	private static bool AddActionToActionList(SyntaxList<ActionBaseSyntax> actionList, ActionBaseSyntax action, string relativeActionName, out SyntaxList<ActionBaseSyntax> newActionList)
	{
		if (!string.IsNullOrEmpty(relativeActionName))
		{
			PageActionSyntax pageActionSyntax = NavDesignerSyntaxHelper.FindActionInList<PageActionSyntax>(actionList, relativeActionName);
			if (pageActionSyntax != null)
			{
				int num = actionList.IndexOf(pageActionSyntax);
				if (num != -1)
				{
					newActionList = actionList.Insert(num + 1, action);
					return true;
				}
			}
			newActionList = actionList;
			return false;
		}
		newActionList = actionList.Insert(0, action);
		return true;
	}

	public bool MovePageAction(NavDesignerActionContext actionContext, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName)
	{
		ActionBaseSyntax anyAction = pageSyntax.GetAnyAction(actionContext.GetActionALIdentifier(), allowArea: false);
		if (anyAction == null)
		{
			return false;
		}
		if (RemovePageAction(actionContext))
		{
			return AddPageAction(anyAction, actionAreaKind, parentActionName, relativeActionName);
		}
		return false;
	}

	public bool RemovePageAction(NavDesignerActionContext actionContext)
	{
		ActionBaseSyntax anyAction = pageSyntax.GetAnyAction(actionContext.GetActionALIdentifier(), allowArea: false);
		if (anyAction != null)
		{
			pageSyntax = pageSyntax.ReplaceNode(anyAction, null);
			return true;
		}
		return false;
	}

	public bool SetPageActionProperty(string actionName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForAction(propertyName, out var propertyKind))
		{
			return false;
		}
		PageActionSyntax action = pageSyntax.GetAction(actionName);
		if (action != null)
		{
			PageActionSyntax newNode = NavDesignerSyntaxHelper.UpdatePropertyList(action, propertyKind, propertyValue);
			pageSyntax = pageSyntax.ReplaceNode(action, newNode);
			return true;
		}
		PageActionGroupSyntax actionGroup = pageSyntax.GetActionGroup(actionName);
		if (actionGroup != null)
		{
			PageActionGroupSyntax newNode2 = NavDesignerSyntaxHelper.UpdatePropertyList(actionGroup, propertyKind, propertyValue);
			pageSyntax = pageSyntax.ReplaceNode(actionGroup, newNode2);
			return true;
		}
		return false;
	}

	public bool AddPageView(string viewName, string relativeViewName, PageExtensionLayoutSyntax layoutSyntax, string filterPropertyValue, string orderByPropertyValue)
	{
		if (pageSyntax.GetView(viewName) != null)
		{
			return false;
		}
		PropertyListSyntax propertyListSyntax = SyntaxFactory.PropertyList();
		if (!string.IsNullOrEmpty(filterPropertyValue))
		{
			propertyListSyntax = propertyListSyntax.AddProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.Filters, filterPropertyValue));
		}
		if (!string.IsNullOrEmpty(orderByPropertyValue))
		{
			propertyListSyntax = propertyListSyntax.AddProperties(NavDesignerSyntaxHelper.CreatePropertySyntax(PropertyKind.OrderBy, orderByPropertyValue));
		}
		PageViewSyntax view = SyntaxFactory.PageView(SyntaxFactory.IdentifierName(viewName), propertyListSyntax, layoutSyntax);
		return AddPageView(view, relativeViewName);
	}

	private bool AddPageView(PageViewSyntax view, string relativeViewName)
	{
		if (pageSyntax.Views == null)
		{
			pageSyntax = pageSyntax.AddViewsViews(view);
		}
		else
		{
			PageViewListSyntax views = pageSyntax.Views;
			if (!AddViewToViewList(views, view, relativeViewName, out var newViewList))
			{
				return false;
			}
			pageSyntax = pageSyntax.ReplaceNode(views, newViewList);
		}
		return true;
	}

	public bool MovePageView(NavDesignerActionContext actionViewContext, string relativeViewName)
	{
		string actionALIdentifier = actionViewContext.GetActionALIdentifier();
		PageViewSyntax view = pageSyntax.GetView(actionALIdentifier);
		if (view == null)
		{
			return false;
		}
		if (RemovePageView(actionViewContext))
		{
			return AddPageView(view, relativeViewName);
		}
		return false;
	}

	public bool RemovePageView(NavDesignerActionContext actionViewContext)
	{
		string actionALIdentifier = actionViewContext.GetActionALIdentifier();
		PageViewSyntax view = pageSyntax.GetView(actionALIdentifier);
		if (view != null)
		{
			pageSyntax = pageSyntax.ReplaceNode(view, null);
			return true;
		}
		return false;
	}

	public bool SetPageViewProperty(string viewName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForView(propertyName, out var propertyKind))
		{
			return false;
		}
		PageViewSyntax view = pageSyntax.GetView(viewName);
		if (view != null)
		{
			PageViewSyntax newNode = NavDesignerSyntaxHelper.UpdatePropertyList(view, propertyKind, propertyValue);
			pageSyntax = pageSyntax.ReplaceNode(view, newNode);
			return true;
		}
		return false;
	}

	private static bool AddViewToViewList(PageViewListSyntax viewList, PageViewSyntax view, string relativeViewName, out PageViewListSyntax newViewList)
	{
		if (!string.IsNullOrEmpty(relativeViewName))
		{
			PageViewSyntax pageViewSyntax = NavDesignerSyntaxHelper.FindViewInList(viewList.Views, relativeViewName);
			if (pageViewSyntax != null)
			{
				int num = viewList.Views.IndexOf(pageViewSyntax);
				if (num != -1)
				{
					newViewList = viewList.WithViews(viewList.Views.Insert(num + 1, view));
					return true;
				}
			}
			newViewList = viewList;
			return false;
		}
		newViewList = viewList.WithViews(viewList.Views.Insert(0, view));
		return true;
	}

	public PageViewSyntax GetViewSyntax(string viewName)
	{
		return pageSyntax.GetView(viewName);
	}

	public bool MovePageViewField(string viewName, NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		PageViewSyntax view = pageSyntax.GetView(viewName);
		if (view == null)
		{
			return false;
		}
		NavDesignerPageLayoutExtension navDesignerPageLayoutExtension = new NavDesignerPageLayoutExtension(view.Layout);
		if (!navDesignerPageLayoutExtension.MovePageField(controlContext, parentControlName, relativeControlName))
		{
			return false;
		}
		PageViewSyntax newNode = view.WithLayout(navDesignerPageLayoutExtension.LayoutSyntax);
		pageSyntax = pageSyntax.ReplaceNode(view, newNode);
		return true;
	}

	public bool SetPageViewFieldProperty(string viewName, string controlName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForField(propertyName, out var _))
		{
			return false;
		}
		PageViewSyntax view = pageSyntax.GetView(viewName);
		if (view == null)
		{
			return false;
		}
		NavDesignerPageLayoutExtension navDesignerPageLayoutExtension = new NavDesignerPageLayoutExtension(view.Layout);
		if (!navDesignerPageLayoutExtension.SetPageFieldProperty(controlName, propertyName, propertyValue))
		{
			return false;
		}
		PageViewSyntax newNode = view.WithLayout(navDesignerPageLayoutExtension.LayoutSyntax);
		pageSyntax = pageSyntax.ReplaceNode(view, newNode);
		return true;
	}

	public void ClearViewControlChanges(string viewName)
	{
		PageViewSyntax view = pageSyntax.GetView(viewName);
		if (view != null)
		{
			PageViewSyntax newNode = view.WithLayout(null);
			pageSyntax = pageSyntax.ReplaceNode(view, newNode);
		}
	}

	public void ClearAllChanges()
	{
		throw new InvalidOperationException("Restore Page Defaults action is not applicable in new page context");
	}

	public void ClearControlChanges()
	{
		throw new InvalidOperationException("Restore Page Control Defaults action is not applicable in new page context");
	}

	public void ClearActionChanges()
	{
		throw new InvalidOperationException("Restore Page Action Defaults action is not applicable in new page context");
	}

	public void ClearViewChanges()
	{
		throw new InvalidOperationException("Restore Page View Defaults action is not applicable in new page context");
	}
}
