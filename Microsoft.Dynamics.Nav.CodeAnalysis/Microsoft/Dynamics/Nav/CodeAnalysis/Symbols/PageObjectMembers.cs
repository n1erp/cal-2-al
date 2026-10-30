using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class PageObjectMembers : ApplicationObjectMembers<PageTypeSymbol>
{
	private const string CommentML = "Comment";

	private ImmutableArray<Symbol> lazyCodeMembers;

	private ImmutableArray<Symbol> lazyBaseMembers;

	internal new PageSyntax Declaration => (PageSyntax)base.Declaration;

	internal PageObjectMembers(PageTypeSymbol containingSymbol, PageSyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageTriggerInfo(name);
	}

	protected override ImmutableArray<Symbol> GetBaseMembers()
	{
		if (lazyBaseMembers != null)
		{
			return lazyBaseMembers;
		}
		ImmutableArray<Symbol> immutableArray = base.GetBaseMembers();
		TableTypeSymbol relatedTable = ContainingSymbol.RelatedTable;
		if (relatedTable != null)
		{
			ImmutableArray<Symbol> members = relatedTable.GetMembers();
			ImmutableArray<Symbol> members2 = relatedTable.BaseType.GetMembers();
			int num = immutableArray.Length + members.Length + members2.Length;
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(num);
			foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingSymbol.ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(relatedTable))
			{
				instance.AddRange(applicationObjectExtensionTypeSymbolsAcrossModule.GetMembers().Cast<Symbol>());
			}
			instance.EnsureCapacity(instance.Count + num);
			instance.AddRange(immutableArray);
			instance.AddRange(members);
			instance.AddRange(members2);
			immutableArray = instance.ToImmutableAndFree();
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyBaseMembers, immutableArray, default);
		return lazyBaseMembers;
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, PageReferenceTypeSymbol.Create(ContainingSymbol, isCurrPage: true), "CurrPage"));
		if (ContainingSymbol.RelatedTable != null)
		{
			RecordTypeSymbol record = ContainingSymbol.ContainingModule.GetRecord(ContainingSymbol.RelatedTable, isTemporary: false);
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "Rec", isThis: false, hasImplicitWith: true));
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "xRec"));
		}
		if (Declaration.Layout != null)
		{
			foreach (PageAreaSyntax area in Declaration.Layout.Areas)
			{
				members.Add(SourceControlSymbol.Create(ContainingSymbol, area));
			}
		}
		AddStubAreas(ContainingSymbol, ContainingSymbol.PageType, members);
		if (Declaration.Actions != null)
		{
			foreach (PageActionAreaSyntax area2 in Declaration.Actions.Areas)
			{
				members.Add(SourceActionSymbol.Create(ContainingSymbol, area2));
			}
		}
		AddStubActionAreas(ContainingSymbol, ContainingSymbol.PageType, members);
		if (Declaration.Views != null)
		{
			foreach (PageViewSyntax view in Declaration.Views.Views)
			{
				members.Add(SourceViewSymbol.Create(ContainingSymbol, view));
			}
		}
	}

	internal static void AddStubAreas(Symbol owner, PageTypeKind pageType, ArrayBuilder<Symbol> members, Func<int> getNextMemberId = null)
	{
		PooledList<AreaKind> instance = PooledList<AreaKind>.GetInstance();
		try
		{
			foreach (Symbol member in members)
			{
				if (member.IsControlArea())
				{
					AreaKind areaKind = SyntaxFacts.GetAreaKind(member.Name);
					if (!instance.Contains(areaKind))
					{
						instance.Add(areaKind);
					}
				}
			}
			foreach (AreaKind item in Enumerable.Except(SyntaxFacts.AreaValues, instance))
			{
				if ((item != AreaKind.RoleCenter || pageType == PageTypeKind.RoleCenter) && (item == AreaKind.RoleCenter || pageType != PageTypeKind.RoleCenter) && (item != AreaKind.FactBoxes || !pageType.IsPartPageType()))
				{
					members.Add(SynthesizedControlSymbol.Create(owner, item, getNextMemberId?.Invoke() ?? 0));
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	internal static void AddStubActionAreas(Symbol owner, PageTypeKind pageType, ArrayBuilder<Symbol> members, Func<int> getNextMemberId = null)
	{
		PooledList<ActionAreaKind> instance = PooledList<ActionAreaKind>.GetInstance();
		try
		{
			foreach (Symbol member in members)
			{
				if (member.IsActionArea())
				{
					ActionAreaKind actionAreaKind = SyntaxFacts.GetActionAreaKind(member.Name);
					if (!instance.Contains(actionAreaKind))
					{
						instance.Add(actionAreaKind);
					}
				}
			}
			foreach (ActionAreaKind item in Enumerable.Except(SyntaxFacts.ActionAreaValues, instance))
			{
				if ((!item.IsActionAreaDedicatedToRoleCenter() || pageType == PageTypeKind.RoleCenter) && (item == ActionAreaKind.Processing || !pageType.IsPartPageType()))
				{
					members.Add(SynthesizedActionSymbol.Create(owner, item, getNextMemberId?.Invoke() ?? 0));
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		PageTypeKind pageType = ContainingSymbol.PageType;
		ImmutableArray<Symbol> members = GetMembers();
		ImmutableArray<ControlSymbol> flattenedControls = ContainingSymbol.FlattenedControls;
		ImmutableArray<ActionSymbol> flattenedActions = ContainingSymbol.FlattenedActions;
		ImmutableArray<ViewSymbol> flattenedViews = ContainingSymbol.FlattenedViews;
		IEnumerable<Symbol> enumerable = members.Where((Symbol m) => m.Kind == SymbolKind.Method).Distinct(SymbolNameEqualityComparer.Instance).Cast<Symbol>()
			.Concat(flattenedControls.Where((ControlSymbol m) => !m.IsControlArea()))
			.Concat(flattenedActions.Where((ActionSymbol m) => !m.IsActionArea()))
			.Concat(flattenedViews);
		if (!enumerable.All((Symbol m) => m.Kind == SymbolKind.Method))
		{
			ObjectMembers<PageTypeSymbol>.CheckMembersForDuplicateNames(enumerable, diagnostics, (Symbol _) => true, ErrorCode.ERR_DuplicateMemberName, null, crossModule: false, ErrorCode.WRN_ERR_DuplicateMemberName, ReportDuplicateMemberAsWarning);
		}
		ObjectMembers<PageTypeSymbol>.CheckMembersForDuplicateIds(flattenedControls, diagnostics, SymbolExtensions.IsControl, ErrorCode.ERR_DuplicateControlId);
		ObjectMembers<PageTypeSymbol>.CheckMembersForDuplicateIds(flattenedActions, diagnostics, SymbolExtensions.IsActionWithId, ErrorCode.ERR_DuplicateActionId);
		PageMemberSymbolHelpers.CheckControlAreaForValidKind(members, pageType, diagnostics);
		ObjectMembers<PageTypeSymbol>.CheckMembersForDuplicateNames(members, diagnostics, SymbolExtensions.IsControlArea, ErrorCode.ERR_DuplicateAreaKind);
		PageMemberSymbolHelpers.CheckActionAreaForValidKind(members, pageType, diagnostics);
		ObjectMembers<PageTypeSymbol>.CheckMembersForDuplicateNames(members, diagnostics, SymbolExtensions.IsActionArea, ErrorCode.ERR_DuplicateAreaKind);
		CheckViewRestrictions(members, pageType, diagnostics);
		CheckRoleCenterRestrictions(pageType, members, diagnostics);
		CheckAPIRestrictions(pageType, diagnostics);
		PageMemberSymbolHelpers.CheckHelpLinkAndContextSensitiveHelpPropertyAtTheSameTime(ContainingSymbol, diagnostics);
	}

	internal static void CheckIfControlAllowedInRepeater(ControlSymbol control, Func<Location> getLocation, DiagnosticBag diagnostics)
	{
		if (!IsControlAllowedInRepeater(control))
		{
			diagnostics.Add(ErrorCode.ERR_ControlTypeNotAllowedInParent, getLocation(), control.ControlKind, ControlKind.Repeater);
		}
	}

	private static bool IsControlAllowedInRepeater(ControlSymbol control)
	{
		ControlKind controlKind = control.ControlKind;
		if (controlKind == ControlKind.Part || (uint)(controlKind - 8) <= 1u)
		{
			return true;
		}
		return false;
	}

	private bool ReportDuplicateMemberAsWarning(IEnumerable<Symbol> duplicateSymbols, Symbol symbol)
	{
		if (symbol.IsCueAction())
		{
			return true;
		}
		if (duplicateSymbols.Any((Symbol s) => s.IsCueAction()) && duplicateSymbols.Where((Symbol s) => s.Kind == SymbolKind.Control || (s.Kind == SymbolKind.Action && !s.IsCueAction())).HasLessThan(2))
		{
			return true;
		}
		if (ContainingSymbol.DeclaringCompilation.Options.IsValidationRelaxedForBackwardsCompatibility())
		{
			return duplicateSymbols.Where((Symbol s) => s.Kind == symbol.Kind).HasLessThan(2);
		}
		return false;
	}

	private void CheckRoleCenterRestrictions(PageTypeKind pageType, ImmutableArray<Symbol> members, DiagnosticBag diagnostics)
	{
		if (pageType == PageTypeKind.RoleCenter)
		{
			CheckIfRoleCenterPageHasTriggers(members, diagnostics);
			CheckRunObjectPropertyForRoleCenter(ContainingSymbol.FlattenedActions, diagnostics);
			CheckSectionsAreaDoesNotHaveActionsAndGroupsOnSameLevel(ContainingSymbol.FlattenedActions, diagnostics);
		}
	}

	private void CheckAPIRestrictions(PageTypeKind pageType, DiagnosticBag diagnostics)
	{
		if (pageType == PageTypeKind.API)
		{
			CheckDelayedInsertPropertyForAPIPages(diagnostics);
			CheckFieldNamesForPagesOfTypeAPI(diagnostics);
		}
	}

	private void CheckDelayedInsertPropertyForAPIPages(DiagnosticBag diagnostics)
	{
		if (ContainingSymbol.GetBooleanPropertyValue(PropertyKind.DelayedInsert) != true)
		{
			PropertySymbol symbol = ContainingSymbol.GetProperty(PropertyKind.DelayedInsert) ?? ContainingSymbol.GetProperty(PropertyKind.PageType);
			diagnostics.Add(ErrorCode.ERR_ApiPageShouldHaveDelayedInsert, symbol.GetLocation());
		}
	}

	private void CheckFieldNamesForPagesOfTypeAPI(DiagnosticBag diagnostics)
	{
		foreach (ControlSymbol flattenedControl in ContainingSymbol.FlattenedControls)
		{
			if (flattenedControl.ControlKind == ControlKind.Field && (string.IsNullOrEmpty(flattenedControl.Name) || !flattenedControl.Name.IsValidAlphaNumeric()))
			{
				diagnostics.Add(ErrorCode.ERR_ApiPageFieldsShouldHaveAlphaNumericName, flattenedControl.GetLocation());
			}
		}
	}

	private void CheckIfRoleCenterPageHasTriggers(ImmutableArray<Symbol> members, DiagnosticBag diagnostics)
	{
		foreach (Symbol item in members.Where((Symbol m) => m.Kind == SymbolKind.Method))
		{
			if (((SourceMethodOrTriggerSymbol)item).MethodKind == MethodKind.Trigger)
			{
				diagnostics.Add(ErrorCode.ERR_RoleCenterShouldNotHaveTriggers, item.GetLocation());
			}
		}
		CheckSymbolWithTriggerForRoleCenter(ContainingSymbol.FlattenedActions, diagnostics);
		CheckSymbolWithTriggerForRoleCenter(ContainingSymbol.FlattenedControls, diagnostics);
	}

	private static void CheckSymbolWithTriggerForRoleCenter<T>(ImmutableArray<T> members, DiagnosticBag diagnostics) where T : ISymbolWithTriggers
	{
		foreach (T item in members)
		{
			if (item.Triggers != null)
			{
				foreach (MethodSymbol trigger in item.Triggers)
				{
					diagnostics.Add(ErrorCode.ERR_RoleCenterShouldNotHaveTriggers, trigger.GetLocation());
				}
			}
		}
	}

	private static void CheckRunObjectPropertyForRoleCenter(ImmutableArray<ActionSymbol> members, DiagnosticBag diagnostics)
	{
		foreach (ActionSymbol item in members)
		{
			if (PageMemberSymbolHelpers.IsContainedInGivenActionArea(item, ActionAreaKind.Embedding))
			{
				VerifyRunObjectPropertyForRoleCenter(item, ErrorCode.ERR_EmbeddingAreaShouldOnlyRunListPages, diagnostics);
			}
			else if (PageMemberSymbolHelpers.IsContainedInGroupInGivenActionArea(item, ActionAreaKind.Sections))
			{
				VerifyRunObjectPropertyForRoleCenter(item, ErrorCode.WRN_ERR_ActionInGroupInSectionsAreaShouldOnlyRunListPages, diagnostics);
			}
		}
	}

	private static void VerifyRunObjectPropertyForRoleCenter(ActionSymbol actionSymbol, ErrorCode errorCode, DiagnosticBag diagnostics)
	{
		PropertySymbol property = actionSymbol.GetProperty(PropertyKind.RunObject);
		if ((object)property != null && property.Value != null)
		{
			PageTypeSymbol obj = property.Value as PageTypeSymbol;
			if ((object)obj == null || obj.PageType != PageTypeKind.List)
			{
				diagnostics.Add(errorCode, property.GetLocation());
			}
		}
	}

	private static void CheckSectionsAreaDoesNotHaveActionsAndGroupsOnSameLevel(ImmutableArray<ActionSymbol> members, DiagnosticBag diagnostics)
	{
		ActionSymbol actionSymbol = members.FirstOrDefault((ActionSymbol x) => x.GetActionAreaKind() == ActionAreaKind.Sections);
		if (actionSymbol != null)
		{
			VerifyActionsAndGroupsOnSameLevel(actionSymbol, diagnostics);
		}
	}

	private static void VerifyActionsAndGroupsOnSameLevel(ActionSymbol action, DiagnosticBag diagnostics)
	{
		ActionSymbol actionSymbol = action.Actions.FirstOrDefault((ActionSymbol x) => x.ActionKind == ActionKind.Group);
		ActionSymbol actionSymbol2 = action.Actions.FirstOrDefault((ActionSymbol x) => x.ActionKind == ActionKind.Action);
		if (actionSymbol != null && actionSymbol2 != null)
		{
			diagnostics.Add(ErrorCode.ERR_ActionAndGroupInSectionsAreaOnSameLevel, actionSymbol2.GetLocation());
			diagnostics.Add(ErrorCode.ERR_ActionAndGroupInSectionsAreaOnSameLevel, actionSymbol.GetLocation());
		}
		foreach (ActionSymbol action2 in action.Actions)
		{
			VerifyActionsAndGroupsOnSameLevel(action2, diagnostics);
		}
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != null)
		{
			return lazyCodeMembers;
		}
		ImmutableArray<Symbol> value = GetMembers().Concat(ContainingSymbol.FlattenedControls.Where((ControlSymbol m) => !m.IsControlArea())).Concat(ContainingSymbol.FlattenedActions.Where((ActionSymbol m) => !m.IsActionArea())).ToImmutableArray();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, value, default);
		return lazyCodeMembers;
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidPageTrigger(name) && !TriggerEventDefinitions.IsValidPageTriggerEvent(name) && !TriggerDefinitions.IsValidActionTrigger(name))
		{
			return TriggerDefinitions.IsValidControlTrigger(name);
		}
		return true;
	}

	private void CheckViewRestrictions(ImmutableArray<Symbol> members, PageTypeKind pageType, DiagnosticBag diagnostics)
	{
		if (!members.Any((Symbol x) => x.Kind == SymbolKind.View))
		{
			return;
		}
		if (!pageType.IsViewSupported())
		{
			Location location = Declaration.Views.ViewsKeyword.GetLocation();
			diagnostics.Add(ErrorCode.ERR_ViewsOnlySupportedOnPagesOfType, location, "List");
			return;
		}
		foreach (Symbol item in members)
		{
			if (item.Kind == SymbolKind.View)
			{
				ValidateView((ViewSymbol)item, diagnostics);
			}
		}
	}

	internal static void ValidateView(ViewSymbol view, DiagnosticBag diagnostics)
	{
		if (!view.ModifySymbols.IsDefaultOrEmpty)
		{
			ChangeObjectMembersHelpers.CheckForNoChange(view.ModifySymbols, view.MoveSymbols, view.DeclaringCompilation.Options, diagnostics);
			ChangeObjectMembersHelpers.CheckForMultipleModify(view.ModifySymbols, diagnostics);
			CheckForFactBoxModify(view.ModifySymbols, diagnostics);
		}
		if (!view.MoveSymbols.IsDefaultOrEmpty)
		{
			ChangeObjectMembersHelpers.CheckForMultipleMoveInSameMove(view.MoveSymbols, diagnostics);
			ChangeObjectMembersHelpers.CheckForMoveRelativeToItself(view.MoveSymbols, diagnostics);
			ChangeObjectMembersHelpers.CheckForMoveAcrossAreas(view.MoveSymbols, diagnostics);
			CheckForFactBoxMove(view.MoveSymbols, diagnostics);
		}
	}

	private static void CheckForFactBoxModify(ImmutableArray<ChangeModifySymbol> modifySymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeModifySymbol item in modifySymbols)
		{
			VerifyContentAreaChanges(item.Target, item, diagnostics);
		}
	}

	private static void CheckForFactBoxMove(ImmutableArray<ChangeMoveSymbol> moveSymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeMoveSymbol item in moveSymbols)
		{
			VerifyContentAreaChanges(item.Anchor, item, diagnostics);
		}
	}

	private static void VerifyContentAreaChanges(ISymbol symbol, ChangeSymbol changeSymbol, DiagnosticBag diagnostics)
	{
		if (symbol != null && symbol.Kind == SymbolKind.Control)
		{
			ControlSymbol controlSymbol = (ControlSymbol)symbol;
			if (controlSymbol.GetArea() != AreaKind.Content && !PageMemberSymbolHelpers.IsContainedInGivenControlArea(controlSymbol, AreaKind.Content))
			{
				diagnostics.Add(ErrorCode.ERR_ViewsLayoutOnlySupportedOnContent, changeSymbol.GetLocation(), controlSymbol.Name);
			}
		}
	}
}
