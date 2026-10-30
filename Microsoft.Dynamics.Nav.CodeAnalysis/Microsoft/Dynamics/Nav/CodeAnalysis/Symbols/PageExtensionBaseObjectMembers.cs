using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class PageExtensionBaseObjectMembers<T> : ExtensionObjectMembers<T, PageTypeSymbol> where T : PageExtensionBaseTypeSymbol
{
	private PageTypeSymbol lazyTarget;

	internal override PageTypeSymbol Target
	{
		get
		{
			if (lazyTarget != null)
			{
				return lazyTarget;
			}
			GetMembers();
			return lazyTarget;
		}
	}

	protected override SymbolKind TargetKind => SymbolKind.Page;

	internal new AbstractPageExtensionSyntax Declaration => (AbstractPageExtensionSyntax)base.Declaration;

	internal abstract ImmutableArray<SourceChangeMoveSymbol> MoveSymbols { get; }

	internal abstract ImmutableArray<SourceChangeModifySymbol> ModifySymbols { get; }

	internal abstract ImmutableArray<SourceChangeAddSymbol> AddSymbols { get; }

	internal PageExtensionBaseObjectMembers(T containingSymbol, AbstractPageExtensionSyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageExtensionTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		GetLazyTarget(diagnostics);
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, (lazyTarget != null) ? ((TypeSymbol)PageReferenceTypeSymbol.Create(lazyTarget, isCurrPage: true)) : ((TypeSymbol)ContainingSymbol), "CurrPage"));
		if (lazyTarget?.RelatedTable != null)
		{
			RecordTypeSymbol record = ContainingSymbol.ContainingModule.GetRecord(lazyTarget.RelatedTable, isTemporary: false);
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "Rec", isThis: false, hasImplicitWith: true));
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "xRec"));
		}
		AddLayoutChanges(members, Declaration.Layout, diagnostics);
		AddActionChanges(members, Declaration.Actions, diagnostics);
		AddViewChanges(members, Declaration.Views, diagnostics);
	}

	private void GetLazyTarget(DiagnosticBag diagnostics)
	{
		if (!(lazyTarget != null) && Declaration.BaseObject != null)
		{
			PageTypeSymbol value = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Declaration.SyntaxTree).GetBinder(Declaration.BaseObject).GetApplicationObjectTypeSymbol(SymbolKind.Page, Declaration.BaseObject, diagnostics) as PageTypeSymbol;
			Interlocked.CompareExchange(ref lazyTarget, value, null);
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		PageTypeSymbol target = Target;
		if ((object)target != null && target.PageType == PageTypeKind.API)
		{
			diagnostics.Add(ErrorCode.ERR_ExtensionTargetTypeCannotBeExtended, Declaration.BaseObject.Location, Target.Kind, Declaration.BaseObject.Identifier, PageTypeKind.API);
		}
		CheckForOnlyAdd(diagnostics);
		CheckForMultipleAdd(diagnostics);
		if (Target != null && !Target.PageType.IsViewSupported())
		{
			PageExtensionViewListSyntax views = Declaration.Views;
			if (views != null && views.Changes.Count > 0)
			{
				Location location = Declaration.Views.ViewsKeyword.GetLocation();
				diagnostics.Add(ErrorCode.ERR_ViewsOnlySupportedOnPagesOfType, location, "List");
				return;
			}
		}
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Change && member is SourceChangeAddViewSymbol { AddedSymbols: var addedSymbols })
			{
				foreach (ViewSymbol item in addedSymbols)
				{
					PageObjectMembers.ValidateView(item, diagnostics);
				}
			}
		}
	}

	protected abstract void AddLayoutChanges(ArrayBuilder<Symbol> members, PageExtensionLayoutSyntax layout, DiagnosticBag diagnostics);

	protected abstract void AddActionChanges(ArrayBuilder<Symbol> members, PageExtensionActionListSyntax area, DiagnosticBag diagnostics);

	protected virtual void AddViewChanges(ArrayBuilder<Symbol> members, PageExtensionViewListSyntax area, DiagnosticBag diagnostics)
	{
		if (area == null)
		{
			return;
		}
		foreach (ViewChangeBaseSyntax change in area.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ViewModifyChange:
				members.Add(SourceChangeModifyViewSymbol.Create(ContainingSymbol, (ViewModifyChangeSyntax)change));
				break;
			case SyntaxKind.ViewMoveChange:
				members.Add(SourceChangeMoveViewSymbol.Create(ContainingSymbol, (ViewMoveChangeSyntax)change));
				break;
			case SyntaxKind.ViewAddChange:
				members.Add(SourceChangeAddViewSymbol.Create(ContainingSymbol, (ViewAddChangeSyntax)change));
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(change.Kind);
			}
		}
	}

	internal void CheckForParentChildRestrictions(DiagnosticBag diagnostics)
	{
		foreach (ChangeSymbolInformation changeInformation in GetTargetSymbolAndItsParentAndChangeSymbolsTuple())
		{
			if (changeInformation.TargetSymbol.Kind == SymbolKind.Control && changeInformation.ParentSymbol.Kind == SymbolKind.Control && ((ControlSymbol)changeInformation.ParentSymbol).ControlKind == ControlKind.Repeater)
			{
				PageObjectMembers.CheckIfControlAllowedInRepeater((ControlSymbol)changeInformation.TargetSymbol, () => changeInformation.DiagnosticLocation, diagnostics);
			}
		}
	}

	protected ImmutableArray<ChangeSymbolInformation> GetTargetSymbolAndItsParentAndChangeSymbolsTuple()
	{
		return ChangeSymbolHelpers.GetTargetSymbolAndItsParentAndChangeSymbolsTuple(MoveSymbols, AddSymbols);
	}

	protected void CheckForMoveAcrossAreas(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForMoveAcrossAreas(ImmutableArray<ChangeMoveSymbol>.CastUp(MoveSymbols), diagnostics);
	}

	protected void CheckForMultipleMoveInSameMove(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForMultipleMoveInSameMove(ImmutableArray<ChangeMoveSymbol>.CastUp(MoveSymbols), diagnostics);
	}

	protected void CheckForMoveRelativeToItself(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForMoveRelativeToItself(ImmutableArray<ChangeMoveSymbol>.CastUp(MoveSymbols), diagnostics);
	}

	protected void CheckForNoChange(DiagnosticBag diagnostics)
	{
		CompilationOptions options = ContainingSymbol.DeclaringCompilation.Options;
		ChangeObjectMembersHelpers.CheckForNoChange(ImmutableArray<ChangeModifySymbol>.CastUp(ModifySymbols), ImmutableArray<ChangeMoveSymbol>.CastUp(MoveSymbols), options, diagnostics);
	}

	protected void CheckForOnlyAdd(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForOnlyAdd(ImmutableArray<ChangeAddSymbol>.CastUp(AddSymbols), ImmutableArray<ChangeModifySymbol>.CastUp(ModifySymbols), ImmutableArray<ChangeMoveSymbol>.CastUp(MoveSymbols), diagnostics);
	}

	protected void CheckForMultipleModify(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForMultipleModify(ImmutableArray<ChangeModifySymbol>.CastUp(ModifySymbols), diagnostics);
	}

	protected void CheckForMultipleAdd(DiagnosticBag diagnostics)
	{
		ChangeObjectMembersHelpers.CheckForMultipleAdd(ImmutableArray<ChangeAddSymbol>.CastUp(AddSymbols), diagnostics);
	}
}
