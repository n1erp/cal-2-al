using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class PageCustomizationObjectMembers : PageExtensionBaseObjectMembers<PageCustomizationTypeSymbol>
{
	internal override ImmutableArray<SourceChangeMoveSymbol> MoveSymbols => ContainingSymbol.MoveSymbols;

	internal override ImmutableArray<SourceChangeModifySymbol> ModifySymbols => ContainingSymbol.ModifySymbols;

	internal override ImmutableArray<SourceChangeAddSymbol> AddSymbols => ContainingSymbol.AddSymbols;

	internal PageCustomizationObjectMembers(PageCustomizationTypeSymbol containingSymbol, PageCustomizationSyntax declaration)
		: base(containingSymbol, (AbstractPageExtensionSyntax)declaration)
	{
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
			case SyntaxKind.ControlMoveChange:
				members.Add(SourceChangeMoveControlSymbol.Create(ContainingSymbol, (ControlMoveChangeSyntax)change));
				break;
			case SyntaxKind.ControlModifyChange:
				members.Add(SourceChangeModifyControlSymbol.Create(ContainingSymbol, (ControlModifyChangeSyntax)change));
				break;
			case SyntaxKind.ControlAddChange:
				diagnostics.Add(ErrorCode.ERR_CannotHaveAddSymbolsOnPageCustomization, change.GetFirstToken().GetLocation());
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
			case SyntaxKind.ActionMoveChange:
				members.Add(SourceChangeMoveActionSymbol.Create(ContainingSymbol, (ActionMoveChangeSyntax)change));
				break;
			case SyntaxKind.ActionAddChange:
				diagnostics.Add(ErrorCode.ERR_CannotHaveAddSymbolsOnPageCustomization, change.GetFirstToken().GetLocation());
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(change.Kind);
			}
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		if (Target == null)
		{
			if (!IsPageCustomizationInPersonalizationMode())
			{
				diagnostics.Add(ErrorCode.ERR_ExtensionTargetNotFound, Declaration.BaseObject.Location, SymbolKind.Page, Declaration.BaseObject.Identifier);
			}
		}
		else
		{
			CheckForMultipleModify(diagnostics);
			CheckForMultipleMoveInSameMove(diagnostics);
			CheckForNoChange(diagnostics);
			CheckForMoveRelativeToItself(diagnostics);
			CheckForMoveAcrossAreas(diagnostics);
			CheckForParentChildRestrictions(diagnostics);
			CheckForOnlyAdd(diagnostics);
			CheckForPageCustomizationsRestrictions(diagnostics);
		}
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	private bool IsPageCustomizationInPersonalizationMode()
	{
		return ContainingSymbol.DeclaringCompilation?.Options.Target.IsPersonalizationTarget() ?? false;
	}

	private void CheckForPageCustomizationsRestrictions(DiagnosticBag diagnostics)
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Method)
			{
				SourceMethodOrTriggerSymbol sourceMethodOrTriggerSymbol = (SourceMethodOrTriggerSymbol)member;
				if (sourceMethodOrTriggerSymbol.MethodKind == MethodKind.Method)
				{
					Location location = ((SourceMethodSymbol)sourceMethodOrTriggerSymbol).GetSyntax().ProcedureKeyword.GetLocation();
					diagnostics.Add(ErrorCode.ERR_CannotHaveMethodsOnPageCustomization, location);
				}
			}
			else if (member.Kind == SymbolKind.GlobalVariable && !member.IsImplicitlyDeclared)
			{
				diagnostics.Add(ErrorCode.ERR_CannotHaveVariablesOnPageCustomization, member.GetLocation());
			}
		}
		if (Declaration.ObjectId != null)
		{
			diagnostics.Add(ErrorCode.ERR_UnexpectedToken, Declaration.ObjectId.Value.GetLocation());
		}
	}
}
