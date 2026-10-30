using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

internal class NavDesignerPageLayoutExtension
{
	private readonly List<INavDesignerPageControlChange> pageControlChanges = new List<INavDesignerPageControlChange>();

	public PageExtensionLayoutSyntax LayoutSyntax => SyntaxFactory.PageExtensionLayout(default(SyntaxList<ControlChangeBaseSyntax>).AddRange(pageControlChanges.Select((INavDesignerPageControlChange c) => c.Syntax)));

	public NavDesignerPageLayoutExtension()
	{
	}

	public NavDesignerPageLayoutExtension(PageExtensionLayoutSyntax layoutSyntax)
	{
		if (layoutSyntax == null)
		{
			return;
		}
		_ = layoutSyntax.Changes;
		foreach (ControlChangeBaseSyntax change in layoutSyntax.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ControlAddChange:
				pageControlChanges.Add(new NavDesignerPageControlAdd((ControlAddChangeSyntax)change));
				break;
			case SyntaxKind.ControlMoveChange:
				pageControlChanges.Add(new NavDesignerPageControlMove((ControlMoveChangeSyntax)change));
				break;
			case SyntaxKind.ControlModifyChange:
				pageControlChanges.Add(new NavDesignerPageControlModify((ControlModifyChangeSyntax)change));
				break;
			}
		}
	}

	internal ReadOnlyCollection<INavDesignerPageControlChange> GetChanges()
	{
		return pageControlChanges.AsReadOnly();
	}

	public bool AddPageField(string controlExpression, string uniqueControlName, string parentControlName, string relativeControlName)
	{
		NavDesignerPageField pageField = new NavDesignerPageField(uniqueControlName, controlExpression);
		return AddPageField(pageField, parentControlName, relativeControlName);
	}

	private bool AddPageField(NavDesignerPageField pageField, string parentControlName, string relativeControlName)
	{
		if (!string.IsNullOrEmpty(relativeControlName))
		{
			NavDesignerPageControlAdd navDesignerPageControlAdd = pageControlChanges.OfType<NavDesignerPageControlAdd>().LastOrDefault((NavDesignerPageControlAdd a) => !string.IsNullOrEmpty(a.AnchorControlName) && SemanticFacts.IsSameName(a.AnchorControlName, relativeControlName) && a.SyntaxKind.Equals(SyntaxKind.AddAfterKeyword));
			if (navDesignerPageControlAdd != null)
			{
				navDesignerPageControlAdd.Controls = navDesignerPageControlAdd.Controls.Insert(0, pageField);
				return true;
			}
			navDesignerPageControlAdd = pageControlChanges.OfType<NavDesignerPageControlAdd>().SingleOrDefault((NavDesignerPageControlAdd a) => a.Controls.OfType<NavDesignerPageField>().SingleOrDefault((NavDesignerPageField c) => SemanticFacts.IsSameName(c.ControlName, relativeControlName)) != null);
			if (navDesignerPageControlAdd != null)
			{
				int num = navDesignerPageControlAdd.Controls.FindIndex((NavDesignerPageControl c) => SemanticFacts.IsSameName(c.ControlName, relativeControlName));
				navDesignerPageControlAdd.Controls = navDesignerPageControlAdd.Controls.Insert(num + 1, pageField);
				return true;
			}
		}
		else if (!string.IsNullOrEmpty(parentControlName))
		{
			NavDesignerPageControlAdd navDesignerPageControlAdd2 = pageControlChanges.OfType<NavDesignerPageControlAdd>().LastOrDefault((NavDesignerPageControlAdd a) => !string.IsNullOrEmpty(a.ParentName) && SemanticFacts.IsSameName(a.ParentName, parentControlName) && a.SyntaxKind.Equals(SyntaxKind.AddFirstKeyword));
			if (navDesignerPageControlAdd2 != null)
			{
				navDesignerPageControlAdd2.Controls = navDesignerPageControlAdd2.Controls.Insert(0, pageField);
				return true;
			}
		}
		pageControlChanges.Add(new NavDesignerPageControlAdd(pageField, parentControlName, relativeControlName));
		return true;
	}

	public bool MovePageField(NavDesignerControlContext controlContext, string parentControlName, string relativeControlName)
	{
		AdjustChangesForControlMoveOrRemove(controlContext);
		NavDesignerPageControlMove navDesignerPageControlMove = pageControlChanges.OfType<NavDesignerPageControlMove>().SingleOrDefault((NavDesignerPageControlMove c) => SemanticFacts.IsSameName(c.ControlName, controlContext.GetControlALIdentifier()));
		if (navDesignerPageControlMove != null)
		{
			pageControlChanges.Remove(navDesignerPageControlMove);
			if (!string.IsNullOrEmpty(relativeControlName))
			{
				navDesignerPageControlMove.AnchorControlName = relativeControlName;
			}
			else
			{
				navDesignerPageControlMove.ParentName = parentControlName;
			}
			pageControlChanges.Add(navDesignerPageControlMove);
			return true;
		}
		NavDesignerPageControlAdd navDesignerPageControlAdd = pageControlChanges.OfType<NavDesignerPageControlAdd>().SingleOrDefault((NavDesignerPageControlAdd a) => a.Controls.SingleOrDefault((NavDesignerPageControl c) => c.ControlName != null && SemanticFacts.IsSameName(c.ControlName, controlContext.GetControlALIdentifier())) != null);
		if (navDesignerPageControlAdd != null)
		{
			NavDesignerPageField pageField = navDesignerPageControlAdd.Controls.Single((NavDesignerPageControl c) => SemanticFacts.IsSameName(c.ControlName, controlContext.GetControlALIdentifier())) as NavDesignerPageField;
			RemovePageField(controlContext, hideIfNotAdded: false);
			return AddPageField(pageField, parentControlName, relativeControlName);
		}
		pageControlChanges.Add(new NavDesignerPageControlMove(controlContext.GetControlALIdentifier(), controlContext.ControlContainerType, parentControlName, relativeControlName));
		return true;
	}

	public bool RemovePageField(NavDesignerControlContext controlContext)
	{
		return RemovePageField(controlContext, hideIfNotAdded: true);
	}

	private bool RemovePageField(NavDesignerControlContext controlContext, bool hideIfNotAdded)
	{
		NavDesignerPageControlAdd navDesignerPageControlAdd = pageControlChanges.OfType<NavDesignerPageControlAdd>().SingleOrDefault((NavDesignerPageControlAdd a) => a.Controls.SingleOrDefault((NavDesignerPageControl c) => SemanticFacts.IsSameName(c.ControlName, controlContext.GetControlALIdentifier())) != null);
		if (navDesignerPageControlAdd != null)
		{
			AdjustChangesForControlMoveOrRemove(controlContext);
			if (navDesignerPageControlAdd.Controls.Count == 1)
			{
				pageControlChanges.Remove(navDesignerPageControlAdd);
			}
			else
			{
				NavDesignerPageControl value = navDesignerPageControlAdd.Controls.Single((NavDesignerPageControl c) => SemanticFacts.IsSameName(c.ControlName, controlContext.GetControlALIdentifier()));
				navDesignerPageControlAdd.Controls = navDesignerPageControlAdd.Controls.Remove(value);
			}
			return true;
		}
		if (hideIfNotAdded)
		{
			return SetPageFieldProperty(controlContext.GetControlALIdentifier(), "Visible", false);
		}
		return true;
	}

	public bool SetPageFieldProperty(string controlName, string propertyName, object propertyValue)
	{
		if (!NavDesignerUtil.IsPropertyAllowedForField(propertyName, out var propertyKind))
		{
			return false;
		}
		PropertySyntax propertySyntax = null;
		if (propertyValue != null)
		{
			propertySyntax = NavDesignerSyntaxHelper.CreatePropertySyntax(propertyKind, propertyValue);
		}
		NavDesignerPageControlModify controlModify = GetControlModify(controlName);
		NavDesignerPageControlAdd controlAdd = GetControlAdd(controlName);
		if (controlModify != null)
		{
			if (propertySyntax != null)
			{
				return controlModify.SetProperty(propertyKind, propertySyntax);
			}
			if (!controlModify.RemoveProperty(propertyKind))
			{
				return false;
			}
			if (!controlModify.HasChanges)
			{
				return pageControlChanges.Remove(controlModify);
			}
			return true;
		}
		if (controlAdd != null)
		{
			if (propertySyntax != null)
			{
				return controlAdd.SetProperty(controlName, propertyKind, propertySyntax);
			}
			return controlAdd.RemoveProperty(controlName, propertyKind);
		}
		if (propertySyntax != null)
		{
			pageControlChanges.Add(new NavDesignerPageControlModify(controlName, propertySyntax));
			return true;
		}
		return false;
	}

	public void ClearChanges()
	{
		pageControlChanges.Clear();
	}

	private void AdjustChangesForControlMoveOrRemove(NavDesignerControlContext controlContext)
	{
		foreach (INavDesignerPageControlChange pageControlChange in pageControlChanges)
		{
			if (pageControlChange is INavDesignerPageControlPositionalChange navDesignerPageControlPositionalChange && !string.IsNullOrEmpty(navDesignerPageControlPositionalChange.AnchorControlName) && SemanticFacts.IsSameName(navDesignerPageControlPositionalChange.AnchorControlName, controlContext.GetControlALIdentifier()))
			{
				if (pageControlChange.SyntaxKind == SyntaxKind.AddBeforeKeyword && controlContext.NextControl != null)
				{
					navDesignerPageControlPositionalChange.AnchorControlName = controlContext.GetControlALIdentifier(controlContext.NextControl);
				}
				else if (pageControlChange.SyntaxKind != SyntaxKind.AddBeforeKeyword && controlContext.PreviousControl != null)
				{
					navDesignerPageControlPositionalChange.AnchorControlName = controlContext.GetControlALIdentifier(controlContext.PreviousControl);
				}
				else
				{
					navDesignerPageControlPositionalChange.ParentName = controlContext.GetControlALIdentifier(controlContext.ParentGroup) ?? NavDesignerUtil.ControlContainerTypeToAreaKind(controlContext.ControlContainerType).ToString();
				}
			}
		}
	}

	private NavDesignerPageControlAdd GetControlAdd(string controlName)
	{
		return pageControlChanges.OfType<NavDesignerPageControlAdd>().SingleOrDefault((NavDesignerPageControlAdd a) => a.Controls.SingleOrDefault((NavDesignerPageControl c) => SemanticFacts.IsSameName(c.ControlName, controlName)) != null);
	}

	private NavDesignerPageControlModify GetControlModify(string controlName)
	{
		return pageControlChanges.OfType<NavDesignerPageControlModify>().FirstOrDefault((NavDesignerPageControlModify c) => SemanticFacts.IsSameName(c.ControlName, controlName));
	}
}
