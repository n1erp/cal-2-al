using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

internal class NavDesignerPageActionListExtension
{
	private readonly List<INavDesignerPageActionChange> pageActionChanges = new List<INavDesignerPageActionChange>();

	public PageExtensionActionListSyntax ActionsSyntax => SyntaxFactory.PageExtensionActionList(default(SyntaxList<ActionChangeBaseSyntax>).AddRange(pageActionChanges.Select((INavDesignerPageActionChange c) => c.Syntax)));

	public NavDesignerPageActionListExtension()
	{
	}

	public NavDesignerPageActionListExtension(PageExtensionActionListSyntax actionListSyntax)
	{
		if (actionListSyntax == null)
		{
			return;
		}
		_ = actionListSyntax.Changes;
		foreach (ActionChangeBaseSyntax change in actionListSyntax.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ActionAddChange:
				pageActionChanges.Add(new NavDesignerPageActionAdd((ActionAddChangeSyntax)change));
				break;
			case SyntaxKind.ActionMoveChange:
				pageActionChanges.Add(new NavDesignerPageActionMove((ActionMoveChangeSyntax)change));
				break;
			case SyntaxKind.ActionModifyChange:
				pageActionChanges.Add(new NavDesignerPageActionModify((ActionModifyChangeSyntax)change));
				break;
			}
		}
	}

	internal ReadOnlyCollection<INavDesignerPageActionChange> GetChanges()
	{
		return pageActionChanges.AsReadOnly();
	}

	public bool AddPageAction(string uniqueActionName, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName, bool isGroup)
	{
		if (isGroup)
		{
			NavDesignerPageActionGroup pageAction = new NavDesignerPageActionGroup(uniqueActionName);
			return AddPageAction(pageAction, actionAreaKind, parentActionName, relativeActionName);
		}
		NavDesignerPageAction pageAction2 = new NavDesignerPageAction(uniqueActionName);
		return AddPageAction(pageAction2, actionAreaKind, parentActionName, relativeActionName);
	}

	private bool AddPageAction(NavDesignerPageAction pageAction, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName)
	{
		if (!string.IsNullOrEmpty(relativeActionName))
		{
			NavDesignerPageActionAdd navDesignerPageActionAdd = pageActionChanges.OfType<NavDesignerPageActionAdd>().LastOrDefault((NavDesignerPageActionAdd a) => !string.IsNullOrEmpty(a.AnchorActionName) && SemanticFacts.IsSameName(a.AnchorActionName, relativeActionName) && a.SyntaxKind.Equals(SyntaxKind.AddAfterKeyword));
			if (navDesignerPageActionAdd != null)
			{
				navDesignerPageActionAdd.Actions = navDesignerPageActionAdd.Actions.Insert(0, pageAction);
				return true;
			}
			navDesignerPageActionAdd = pageActionChanges.OfType<NavDesignerPageActionAdd>().SingleOrDefault((NavDesignerPageActionAdd a) => a.Actions.OfType<NavDesignerPageAction>().SingleOrDefault((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, relativeActionName)) != null);
			if (navDesignerPageActionAdd != null)
			{
				int num = navDesignerPageActionAdd.Actions.FindIndex((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, relativeActionName));
				navDesignerPageActionAdd.Actions = navDesignerPageActionAdd.Actions.Insert(num + 1, pageAction);
				return true;
			}
		}
		else if (!string.IsNullOrEmpty(parentActionName))
		{
			NavDesignerPageActionAdd navDesignerPageActionAdd2 = pageActionChanges.OfType<NavDesignerPageActionAdd>().LastOrDefault((NavDesignerPageActionAdd a) => !string.IsNullOrEmpty(a.ParentName) && SemanticFacts.IsSameName(a.ParentName, parentActionName) && a.SyntaxKind.Equals(SyntaxKind.AddFirstKeyword));
			if (navDesignerPageActionAdd2 != null)
			{
				navDesignerPageActionAdd2.Actions = navDesignerPageActionAdd2.Actions.Insert(0, pageAction);
				return true;
			}
		}
		pageActionChanges.Add(new NavDesignerPageActionAdd(pageAction, actionAreaKind, parentActionName, relativeActionName));
		return true;
	}

	public bool MovePageAction(NavDesignerActionContext actionContext, ActionAreaKind actionAreaKind, string parentActionName, string relativeActionName)
	{
		AdjustChangesForActionMoveOrRemove(actionContext);
		NavDesignerPageActionMove navDesignerPageActionMove = pageActionChanges.OfType<NavDesignerPageActionMove>().SingleOrDefault((NavDesignerPageActionMove c) => SemanticFacts.IsSameName(c.ActionName, actionContext.GetActionALIdentifier()));
		if (navDesignerPageActionMove != null)
		{
			pageActionChanges.Remove(navDesignerPageActionMove);
			if (!string.IsNullOrEmpty(relativeActionName))
			{
				navDesignerPageActionMove.AnchorActionName = relativeActionName;
			}
			else if (!string.IsNullOrEmpty(parentActionName))
			{
				navDesignerPageActionMove.ParentName = parentActionName;
			}
			else
			{
				navDesignerPageActionMove.ParentName = actionAreaKind.ToString();
			}
			pageActionChanges.Add(navDesignerPageActionMove);
			return true;
		}
		NavDesignerPageActionAdd navDesignerPageActionAdd = pageActionChanges.OfType<NavDesignerPageActionAdd>().SingleOrDefault((NavDesignerPageActionAdd a) => a.Actions.SingleOrDefault((NavDesignerPageAction c) => c.ActionName != null && SemanticFacts.IsSameName(c.ActionName, actionContext.GetActionALIdentifier())) != null);
		if (navDesignerPageActionAdd != null)
		{
			NavDesignerPageAction pageAction = navDesignerPageActionAdd.Actions.Single((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, actionContext.GetActionALIdentifier()));
			RemovePageAction(actionContext, hideIfNotAdded: false);
			return AddPageAction(pageAction, actionAreaKind, parentActionName, relativeActionName);
		}
		pageActionChanges.Add(new NavDesignerPageActionMove(actionContext.GetActionALIdentifier(), actionAreaKind, parentActionName, relativeActionName));
		return true;
	}

	public bool RemovePageAction(NavDesignerActionContext actionContext)
	{
		return RemovePageAction(actionContext, hideIfNotAdded: true);
	}

	private bool RemovePageAction(NavDesignerActionContext actionContext, bool hideIfNotAdded)
	{
		NavDesignerPageActionAdd navDesignerPageActionAdd = pageActionChanges.OfType<NavDesignerPageActionAdd>().SingleOrDefault((NavDesignerPageActionAdd a) => a.Actions.SingleOrDefault((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, actionContext.GetActionALIdentifier())) != null);
		if (navDesignerPageActionAdd != null)
		{
			AdjustChangesForActionMoveOrRemove(actionContext);
			if (navDesignerPageActionAdd.Actions.Count == 1)
			{
				pageActionChanges.Remove(navDesignerPageActionAdd);
			}
			else
			{
				NavDesignerPageAction value = navDesignerPageActionAdd.Actions.Single((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, actionContext.GetActionALIdentifier()));
				navDesignerPageActionAdd.Actions = navDesignerPageActionAdd.Actions.Remove(value);
			}
			return true;
		}
		if (hideIfNotAdded)
		{
			return SetPageActionProperty(actionContext.GetActionALIdentifier(), "Visible", false);
		}
		return true;
	}

	public bool SetPageActionProperty(string actionName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForAction(propertyName, out var propertyKind))
		{
			return false;
		}
		PropertySyntax propertySyntax = null;
		if (propertyValue != null)
		{
			propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(propertyKind, propertyValue);
		}
		NavDesignerPageActionModify actionModify = GetActionModify(actionName);
		NavDesignerPageActionAdd actionAdd = GetActionAdd(actionName);
		if (actionModify != null)
		{
			if (propertySyntax != null)
			{
				return actionModify.SetProperty(propertyKind, propertySyntax);
			}
			if (!actionModify.RemoveProperty(propertyKind))
			{
				return false;
			}
			if (!actionModify.HasChanges)
			{
				return pageActionChanges.Remove(actionModify);
			}
			return true;
		}
		if (actionAdd != null)
		{
			if (propertySyntax != null)
			{
				return actionAdd.SetProperty(actionName, propertyKind, propertySyntax);
			}
			return actionAdd.RemoveProperty(actionName, propertyKind);
		}
		if (propertySyntax != null)
		{
			pageActionChanges.Add(new NavDesignerPageActionModify(actionName, propertySyntax));
			return true;
		}
		return false;
	}

	public void ClearChanges()
	{
		pageActionChanges.Clear();
	}

	private void AdjustChangesForActionMoveOrRemove(NavDesignerActionContext actionContext)
	{
		foreach (INavDesignerPageActionChange pageActionChange in pageActionChanges)
		{
			if (pageActionChange is INavDesignerPageActionPositionalChange navDesignerPageActionPositionalChange && !string.IsNullOrEmpty(navDesignerPageActionPositionalChange.AnchorActionName) && SemanticFacts.IsSameName(navDesignerPageActionPositionalChange.AnchorActionName, actionContext.GetActionALIdentifier()))
			{
				if (pageActionChange.SyntaxKind == SyntaxKind.AddBeforeKeyword && actionContext.NextAction != null)
				{
					navDesignerPageActionPositionalChange.AnchorActionName = actionContext.GetActionALIdentifier(actionContext.NextAction);
				}
				else if (pageActionChange.SyntaxKind != SyntaxKind.AddBeforeKeyword && actionContext.PreviousAction != null)
				{
					navDesignerPageActionPositionalChange.AnchorActionName = actionContext.GetActionALIdentifier(actionContext.PreviousAction);
				}
				else
				{
					navDesignerPageActionPositionalChange.ParentName = actionContext.GetActionALIdentifier(actionContext.ParentGroup) ?? NavDesignerUtil.ActionContainerTypeToAreaKind(actionContext.ActionContainerType).ToString();
				}
			}
		}
	}

	private NavDesignerPageActionAdd GetActionAdd(string actionName)
	{
		return pageActionChanges.OfType<NavDesignerPageActionAdd>().SingleOrDefault((NavDesignerPageActionAdd a) => a.Actions.SingleOrDefault((NavDesignerPageAction c) => SemanticFacts.IsSameName(c.ActionName, actionName)) != null);
	}

	private NavDesignerPageActionModify GetActionModify(string actionName)
	{
		return pageActionChanges.OfType<NavDesignerPageActionModify>().FirstOrDefault((NavDesignerPageActionModify c) => SemanticFacts.IsSameName(c.ActionName, actionName));
	}
}
