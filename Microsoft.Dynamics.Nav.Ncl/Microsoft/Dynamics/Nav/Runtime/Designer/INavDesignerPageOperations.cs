using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerPageOperations
{
	SyntaxKind Kind();

	bool SetPageProperty(string propertyName, object propertyValue);

	bool AddPageField(string controlExpression, string uniqueControlName, string parentControlName, string relativeControlName);

	bool MovePageField(NavDesignerControlContext controlContext, string parentControlName, string relativeControlName);

	bool RemovePageField(NavDesignerControlContext controlContext);

	bool SetPageFieldProperty(string controlName, string propertyName, object propertyValue);

	bool AddPageAction(string uniqueActionName, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName, bool isGroup);

	bool MovePageAction(NavDesignerActionContext actionContext, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName);

	bool RemovePageAction(NavDesignerActionContext actionContext);

	bool SetPageActionProperty(string actionName, string propertyName, object propertyValue);

	bool AddPageView(string viewName, string relativeViewName, PageExtensionLayoutSyntax layoutSyntax, string filterPropertyValue, string orderByPropertyValue);

	bool MovePageView(NavDesignerActionContext NavDesignerActionContext, string relativeViewName);

	bool RemovePageView(NavDesignerActionContext actionViewContext);

	bool SetPageViewProperty(string viewName, string propertyName, object propertyValue);

	PageViewSyntax GetViewSyntax(string viewName);

	bool MovePageViewField(string viewName, NavDesignerControlContext controlContext, string parentControlName, string relativeControlName);

	bool SetPageViewFieldProperty(string viewName, string controlName, string propertyName, object propertyValue);

	void ClearViewControlChanges(string viewName);

	void ClearAllChanges();

	void ClearControlChanges();

	void ClearActionChanges();

	void ClearViewChanges();
}
