using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public static class PageMemberSymbolHelpers
{
	internal static bool IsContainedInControlOfKind(Symbol objectSymbol, ControlKind controlKind)
	{
		ControlSymbol containingSymbol = GetContainingSymbol<ControlSymbol>(objectSymbol);
		if ((object)containingSymbol != null && containingSymbol.ControlKind == controlKind)
		{
			return true;
		}
		return false;
	}

	internal static bool IsDirectlyContainedInGivenActionArea(ActionSymbol actionSymbol, ActionAreaKind areaKind)
	{
		ActionSymbol containingSymbol = GetContainingSymbol<ActionSymbol>(actionSymbol);
		if ((object)containingSymbol == null)
		{
			return false;
		}
		return containingSymbol.GetActionAreaKind() == areaKind;
	}

	internal static bool IsContainedInGroupInGivenActionArea(ActionSymbol actionSymbol, ActionAreaKind areaKind)
	{
		ActionSymbol containingSymbol = GetContainingSymbol<ActionSymbol>(actionSymbol);
		if ((object)containingSymbol == null || containingSymbol.ActionKind != ActionKind.Group)
		{
			return false;
		}
		return IsContainedInGivenActionArea(containingSymbol, areaKind);
	}

	internal static void CheckControlAreaForValidKind(ImmutableArray<Symbol> members, PageTypeKind pageType, DiagnosticBag diagnostics)
	{
		foreach (Symbol item in members)
		{
			if (!item.IsControlArea())
			{
				continue;
			}
			ControlSymbol controlSymbol = (ControlSymbol)item;
			if (controlSymbol.Locations.IsDefaultOrEmpty)
			{
				continue;
			}
			AreaKind areaKind = SyntaxFacts.GetAreaKind(controlSymbol.Name);
			switch (areaKind)
			{
			case AreaKind.RoleCenter:
				if (pageType != PageTypeKind.RoleCenter)
				{
					diagnostics.Add(ErrorCode.ERR_AreaOnlyValidOnPageOfType, controlSymbol.GetLocation(), controlSymbol.Name, PageTypeKind.RoleCenter);
				}
				break;
			case AreaKind.Content:
				if (pageType == PageTypeKind.RoleCenter)
				{
					diagnostics.Add(ErrorCode.WRN_ERR_AreaNotValidOnPageOfType, controlSymbol.GetLocation(), controlSymbol.Name, PageTypeKind.RoleCenter);
				}
				break;
			case AreaKind.FactBoxes:
				if (pageType.IsPartPageType())
				{
					diagnostics.Add(ErrorCode.ERR_FactBoxesNotAllowedOnParts, controlSymbol.GetLocation(), controlSymbol.Name);
				}
				if (pageType == PageTypeKind.RoleCenter)
				{
					diagnostics.Add(ErrorCode.WRN_ERR_AreaNotValidOnPageOfType, controlSymbol.GetLocation(), controlSymbol.Name, PageTypeKind.RoleCenter);
				}
				break;
			case AreaKind.None:
				diagnostics.Add(ErrorCode.ERR_InvalidAreaKind, controlSymbol.GetLocation(), controlSymbol.Name);
				break;
			}
			CheckSubControls(pageType, areaKind, controlSymbol, diagnostics);
		}
	}

	internal static void CheckActionAreaForValidKind(ImmutableArray<Symbol> members, PageTypeKind pageType, DiagnosticBag diagnostics)
	{
		foreach (Symbol item in members)
		{
			if (!item.IsActionArea())
			{
				continue;
			}
			ActionSymbol actionSymbol = (ActionSymbol)item;
			if (actionSymbol.Locations.IsDefaultOrEmpty)
			{
				continue;
			}
			ActionAreaKind actionAreaKind = SyntaxFacts.GetActionAreaKind(actionSymbol.Name);
			if (pageType.IsPartPageType() && actionAreaKind != ActionAreaKind.Processing)
			{
				diagnostics.Add(ErrorCode.ERR_PartsCanOnlyContainProcessingAreas, actionSymbol.GetLocation(), actionSymbol.Name);
				continue;
			}
			if (actionAreaKind.IsActionAreaDedicatedToRoleCenter() && pageType != PageTypeKind.RoleCenter)
			{
				diagnostics.Add(ErrorCode.ERR_AreaOnlyValidOnPageOfType, actionSymbol.GetLocation(), actionSymbol.Name, PageTypeKind.RoleCenter);
			}
			if (actionAreaKind == ActionAreaKind.Navigation && pageType == PageTypeKind.RoleCenter)
			{
				diagnostics.Add(ErrorCode.WRN_ERR_AreaNotValidOnPageOfType, actionSymbol.GetLocation(), actionSymbol.Name, PageTypeKind.RoleCenter);
			}
			if (actionAreaKind == ActionAreaKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidAreaKind, actionSymbol.GetLocation(), actionSymbol.Name);
			}
			CheckSubActions(actionSymbol, diagnostics);
		}
	}

	private static void CheckSubActions(ActionSymbol parent, DiagnosticBag diagnostics)
	{
		if (parent.Actions.IsDefaultOrEmpty)
		{
			return;
		}
		foreach (ActionSymbol action in parent.Actions)
		{
			if (action.ActionKind != ActionKind.Area && SyntaxFacts.GetActionAreaKind(action.Name) != ActionAreaKind.None)
			{
				diagnostics.Add(ErrorCode.WRN_ReuseOfAreaName, action.GetLocation(), action.Name);
			}
			CheckSubActions(action, diagnostics);
		}
	}

	private static void CheckSubControls(PageTypeKind pageType, AreaKind areaKind, ControlSymbol parent, DiagnosticBag diagnostics)
	{
		if (parent.Controls.IsDefaultOrEmpty)
		{
			return;
		}
		foreach (ControlSymbol control in parent.Controls)
		{
			switch (areaKind)
			{
			case AreaKind.FactBoxes:
				if (!control.ControlKind.IsAnyPartKind())
				{
					diagnostics.Add(ErrorCode.ERR_FactBoxesCanOnlyContainParts, control.GetLocation());
				}
				break;
			case AreaKind.RoleCenter:
				if (control.ControlKind != ControlKind.Group && !control.ControlKind.IsAnyPartKind())
				{
					diagnostics.Add(ErrorCode.ERR_OnlyGroupsAndPartsAreValidInRoleCenters, control.GetLocation());
				}
				break;
			}
			if (control.ControlKind != ControlKind.Area && SyntaxFacts.GetAreaKind(control.Name) != AreaKind.None)
			{
				diagnostics.Add(ErrorCode.WRN_ReuseOfAreaName, control.GetLocation(), control.Name);
			}
			SourceControlSymbol sourceControlSymbol = (SourceControlSymbol)control;
			switch (control.ControlKind)
			{
			case ControlKind.Part:
				if (pageType.IsPartPageType())
				{
					diagnostics.Add(ErrorCode.ERR_PartsCannotContainParts, control.GetLocation());
				}
				break;
			case ControlKind.SystemPart:
			{
				IdentifierNameSyntax systemPartType = ((PageSystemPartSyntax)sourceControlSymbol.ControlBaseSyntax).SystemPartType;
				if (SyntaxFacts.GetSystemPartKind(systemPartType.Identifier.Text) == PageSystemPartKind.None)
				{
					string[] names = Enum.GetNames(typeof(PageSystemPartKind));
					string text = string.Join(", ", names.Where((string n) => n != "None"));
					diagnostics.Add(ErrorCode.ERR_InvalidSystemPartKind, systemPartType.Location, text);
				}
				break;
			}
			}
			CheckParentChildRestrictions(parent, control, diagnostics);
			CheckSubControls(pageType, areaKind, sourceControlSymbol, diagnostics);
		}
	}

	private static void CheckParentChildRestrictions(ControlSymbol parent, ControlSymbol child, DiagnosticBag diagnostics)
	{
		if (parent.ControlKind == ControlKind.Repeater)
		{
			PageObjectMembers.CheckIfControlAllowedInRepeater(child, () => child.GetLocation(), diagnostics);
		}
	}

	internal static bool IsContainedInGivenActionArea(Symbol objectSymbol, ActionAreaKind areaKind)
	{
		ActionSymbol containingSymbol = GetContainingSymbol<ActionSymbol>(objectSymbol);
		if (containingSymbol == null)
		{
			return false;
		}
		if (containingSymbol.GetActionAreaKind() == areaKind)
		{
			return true;
		}
		return IsContainedInGivenActionArea(objectSymbol.ContainingSymbol, areaKind);
	}

	internal static bool IsContainedInGivenControlArea(Symbol objectSymbol, AreaKind areaKind)
	{
		ControlSymbol controlSymbol = objectSymbol as ControlSymbol;
		if (controlSymbol == null)
		{
			return false;
		}
		return controlSymbol.GetArea() == areaKind;
	}

	private static T GetContainingSymbol<T>(Symbol objectSymbol) where T : Symbol
	{
		T val = objectSymbol?.ContainingSymbol as T;
		if (val != null)
		{
			return val;
		}
		ChangeAddSymbol changeAddSymbol = objectSymbol?.ContainingSymbol as ChangeAddSymbol;
		if (changeAddSymbol != null)
		{
			switch (changeAddSymbol.ChangeKind)
			{
			case ChangeKind.AddFirst:
			case ChangeKind.AddLast:
				val = changeAddSymbol?.Anchor as T;
				if (val != null)
				{
					return val;
				}
				break;
			case ChangeKind.AddBefore:
			case ChangeKind.AddAfter:
				val = changeAddSymbol?.Anchor?.ContainingSymbol as T;
				if (val != null)
				{
					return val;
				}
				break;
			}
		}
		return null;
	}

	internal static void CheckHelpLinkAndContextSensitiveHelpPropertyAtTheSameTime(Symbol symbolWithProperties, DiagnosticBag diagnostics)
	{
		if (symbolWithProperties.Kind != SymbolKind.Page && symbolWithProperties.Kind != SymbolKind.RequestPage)
		{
			return;
		}
		PropertySymbol propertySymbol = null;
		PropertySymbol propertySymbol2 = null;
		foreach (PropertySymbol property in symbolWithProperties.Properties)
		{
			if (property.PropertyKind == PropertyKind.HelpLink)
			{
				propertySymbol = property;
			}
			else if (property.PropertyKind == PropertyKind.ContextSensitiveHelpPage)
			{
				propertySymbol2 = property;
			}
			if (propertySymbol != null && propertySymbol2 != null)
			{
				break;
			}
		}
		if (propertySymbol != null && propertySymbol2 != null)
		{
			diagnostics.Add(ErrorCode.ERR_CannotSpecifyLabelAndMultilanguageSameTime, propertySymbol.GetLocation(), propertySymbol.Name, propertySymbol2.Name);
			diagnostics.Add(ErrorCode.ERR_CannotSpecifyLabelAndMultilanguageSameTime, propertySymbol2.GetLocation(), propertySymbol.Name, propertySymbol2.Name);
		}
	}
}
