using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class PageExtensionObjectMembers : PageExtensionBaseObjectMembers<PageExtensionTypeSymbol>
{
	private ImmutableArray<Symbol> lazyCodeMembers;

	private ImmutableArray<Symbol> lazyTargetMembers;

	internal override ImmutableArray<SourceChangeMoveSymbol> MoveSymbols => ContainingSymbol.MoveSymbols;

	internal override ImmutableArray<SourceChangeModifySymbol> ModifySymbols => ContainingSymbol.ModifySymbols;

	internal override ImmutableArray<SourceChangeAddSymbol> AddSymbols => ContainingSymbol.AddSymbols;

	internal PageExtensionObjectMembers(PageExtensionTypeSymbol containingSymbol, PageExtensionSyntax declaration)
		: base(containingSymbol, (AbstractPageExtensionSyntax)declaration)
	{
	}

	protected override ImmutableArray<Symbol> GetTargetMembers()
	{
		if (lazyTargetMembers != null)
		{
			return lazyTargetMembers;
		}
		ImmutableArray<Symbol> immutableArray = base.GetTargetMembers();
		TableTypeSymbol relatedTableSymbol = Target.GetRelatedTableSymbol();
		if (relatedTableSymbol != null)
		{
			ImmutableArray<Symbol> members = relatedTableSymbol.GetMembers();
			ImmutableArray<Symbol> members2 = relatedTableSymbol.BaseType.GetMembers();
			int num = relatedTableSymbol.Length + members.Length + members2.Length;
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(num);
			foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingSymbol.ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(relatedTableSymbol))
			{
				instance.AddRange(applicationObjectExtensionTypeSymbolsAcrossModule.GetMembers().Cast<Symbol>());
			}
			instance.EnsureCapacity(instance.Count + num);
			instance.AddRange(immutableArray);
			instance.AddRange(members);
			instance.AddRange(members2);
			immutableArray = instance.ToImmutableAndFree();
			immutableArray.GetEnumerator();
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyTargetMembers, immutableArray, default);
		return lazyTargetMembers;
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetPageExtensionTriggerInfo(name);
	}

	protected override void AddLayoutChanges(ArrayBuilder<Symbol> members, PageExtensionLayoutSyntax layout, DiagnosticBag diagnostics)
	{
		if (layout == null)
		{
			return;
		}
		foreach (ControlChangeBaseSyntax change in layout.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ControlAddChange:
			{
				SourceChangeAddControlSymbol item = SourceChangeAddControlSymbol.Create(ContainingSymbol, (ControlAddChangeSyntax)change);
				members.Add(item);
				break;
			}
			case SyntaxKind.ControlMoveChange:
				members.Add(SourceChangeMoveControlSymbol.Create(ContainingSymbol, (ControlMoveChangeSyntax)change));
				break;
			case SyntaxKind.ControlModifyChange:
				members.Add(SourceChangeModifyControlSymbol.Create(ContainingSymbol, (ControlModifyChangeSyntax)change));
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(change.Kind);
			}
		}
	}

	protected override void AddActionChanges(ArrayBuilder<Symbol> members, PageExtensionActionListSyntax area, DiagnosticBag diagnostics)
	{
		if (area == null)
		{
			return;
		}
		foreach (ActionChangeBaseSyntax change in area.Changes)
		{
			switch (change.Kind)
			{
			case SyntaxKind.ActionModifyChange:
				members.Add(SourceChangeModifyActionSymbol.Create(ContainingSymbol, (ActionModifyChangeSyntax)change));
				break;
			case SyntaxKind.ActionAddChange:
			{
				SourceChangeAddActionSymbol item = SourceChangeAddActionSymbol.Create(ContainingSymbol, (ActionAddChangeSyntax)change);
				members.Add(item);
				break;
			}
			case SyntaxKind.ActionMoveChange:
				members.Add(SourceChangeMoveActionSymbol.Create(ContainingSymbol, (ActionMoveChangeSyntax)change));
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(change.Kind);
			}
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		if (CheckTargetRelation(diagnostics))
		{
			CheckForMultipleModify(diagnostics);
			CheckForMultipleMoveInSameMove(diagnostics);
			CheckForNoChange(diagnostics);
			CheckForMoveRelativeToItself(diagnostics);
			CheckForMoveAcrossAreas(diagnostics);
			CheckForParentChildRestrictions(diagnostics);
		}
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != null)
		{
			return lazyCodeMembers;
		}
		int num = ContainingSymbol.FlattenedControls.Length + ContainingSymbol.FlattenedActions.Length + ContainingSymbol.FlattenedViews.Length;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(num);
		try
		{
			foreach (Symbol member in GetMembers())
			{
				if (member.Kind != SymbolKind.Change)
				{
					instance.Add(member);
				}
			}
			instance.EnsureCapacity(instance.Count + num);
			instance.AddRange(ContainingSymbol.FlattenedControls);
			instance.AddRange(ContainingSymbol.FlattenedActions);
			instance.AddRange(ContainingSymbol.FlattenedViews);
			ImmutableArray<Symbol> value = instance.ToImmutable();
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, value, default);
			return lazyCodeMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidPageExtensionTrigger(name))
		{
			return TriggerEventDefinitions.IsValidPageTriggerEvent(name);
		}
		return true;
	}
}
