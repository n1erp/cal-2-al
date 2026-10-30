using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class RequestPageObjectMembers : ObjectMembers<SourceRequestPageTypeSymbol>
{
	private ImmutableArray<Symbol> lazyCodeMembers;

	internal new RequestPageSyntax Declaration => (RequestPageSyntax)base.Declaration;

	public RequestPageObjectMembers(SourceRequestPageTypeSymbol containingSymbol, RequestPageSyntax declaration)
		: base(containingSymbol, (ObjectSyntax)declaration)
	{
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		if (ContainingSymbol.RelatedTable != null)
		{
			RecordTypeSymbol record = ContainingSymbol.ContainingModule.GetRecord(ContainingSymbol.RelatedTable, isTemporary: false);
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "Rec", isThis: false, hasImplicitWith: true));
			members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "xRec"));
		}
		members.Add(ContainingSymbol.ContainingSymbol.GetCurrentObjectSymbol(ContainingSymbol));
		if (Declaration.Layout != null)
		{
			foreach (PageAreaSyntax area in Declaration.Layout.Areas)
			{
				members.Add(SourceControlSymbol.Create(ContainingSymbol, area));
			}
		}
		if (Declaration.Actions != null)
		{
			foreach (PageActionAreaSyntax area2 in Declaration.Actions.Areas)
			{
				members.Add(SourceActionSymbol.Create(ContainingSymbol, area2));
			}
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		ImmutableArray<Symbol> members = GetMembers();
		PageTypeKind pageType = ContainingSymbol.PageType;
		ObjectMembers<SourceRequestPageTypeSymbol>.CheckMembersForDuplicateNames((from m in GetMembers()
			where m.Kind == SymbolKind.Method
			select m).Concat(ContainingSymbol.FlattenedControls.Where((ControlSymbol m) => !m.IsControlArea())).Concat(ContainingSymbol.FlattenedActions.Where((ActionSymbol m) => !m.IsActionArea())), diagnostics, (Symbol m) => true, ErrorCode.ERR_DuplicateMemberName);
		ObjectMembers<SourceRequestPageTypeSymbol>.CheckMembersForDuplicateIds(ContainingSymbol.FlattenedControls, diagnostics, SymbolExtensions.IsControl, ErrorCode.ERR_DuplicateControlId);
		ObjectMembers<SourceRequestPageTypeSymbol>.CheckMembersForDuplicateIds(ContainingSymbol.FlattenedActions, diagnostics, SymbolExtensions.IsActionWithId, ErrorCode.ERR_DuplicateActionId);
		PageMemberSymbolHelpers.CheckControlAreaForValidKind(members, pageType, diagnostics);
		ObjectMembers<SourceRequestPageTypeSymbol>.CheckMembersForDuplicateNames(GetMembers(), diagnostics, SymbolExtensions.IsControlArea, ErrorCode.ERR_DuplicateAreaKind);
		PageMemberSymbolHelpers.CheckActionAreaForValidKind(members, pageType, diagnostics);
		ObjectMembers<SourceRequestPageTypeSymbol>.CheckMembersForDuplicateNames(GetMembers(), diagnostics, SymbolExtensions.IsActionArea, ErrorCode.ERR_DuplicateAreaKind);
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
}
