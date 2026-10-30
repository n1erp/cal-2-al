using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class ChangeSymbolExtensions
{
	public static PageTypeSymbol GetTargetPage(this ChangeSymbol symbol)
	{
		PageTypeSymbol pageTypeSymbol = (symbol.ContainingSymbol as ApplicationObjectExtensionTypeSymbol)?.Target as PageTypeSymbol;
		if (pageTypeSymbol != null)
		{
			return pageTypeSymbol;
		}
		pageTypeSymbol = (symbol.ContainingSymbol as ViewSymbol)?.GetContainingPage() as PageTypeSymbol;
		if (pageTypeSymbol != null)
		{
			return pageTypeSymbol;
		}
		return symbol.GetContainingSymbolOfKind<PageExtensionBaseTypeSymbol>(SymbolKind.PageExtension, SymbolKind.PageCustomization)?.Target as PageTypeSymbol;
	}

	internal static Symbol FindSymbolInTargetPage(this ChangeSymbol changeSymbol, PageTypeSymbol targetPage, IdentifierNameSyntax symbolName, Func<Symbol, bool> filter, ErrorCode errorCode, ErrorCode personalizationErrorCode, DiagnosticBag diagnostics)
	{
		return GetSymbolFromTargetPage(changeSymbol, targetPage, symbolName, filter, errorCode, personalizationErrorCode, diagnostics);
	}

	internal static ImmutableArray<Symbol> GetMovedSymbolsInTargetPage(this ChangeMoveSymbol changeSymbol, PageTypeSymbol targetPage, SeparatedSyntaxList<IdentifierNameSyntax> movedSymbolsNames, Func<Symbol, bool> filter, ErrorCode errorCode, ErrorCode personalizationErrorCode, DiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		try
		{
			foreach (IdentifierNameSyntax item in movedSymbolsNames)
			{
				Symbol symbolFromTargetPage = GetSymbolFromTargetPage(changeSymbol, targetPage, item, filter, errorCode, personalizationErrorCode, diagnostics);
				instance.Add(symbolFromTargetPage);
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}

	public static Symbol GetActionAnchor(this ChangeSymbol changeSymbol, IdentifierNameSyntax anchorNameSyntax, DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = changeSymbol.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		Symbol symbol = changeSymbol.FindSymbolInTargetPage(targetPage, anchorNameSyntax, (Symbol s) => s.Kind == SymbolKind.Action, ErrorCode.ERR_ActionNotFound, ErrorCode.INF_PERS_ActionNotFound, diagnostics);
		if (symbol.Kind == SymbolKind.ErrorType)
		{
			return symbol;
		}
		ActionSymbol actionSymbol = (ActionSymbol)symbol;
		if (IsPositionInsideAnchor(changeSymbol.ChangeKind) && !actionSymbol.ActionKind.IsGroup())
		{
			diagnostics.Add(ErrorCode.ERR_AnchorMustBeGroupType, anchorNameSyntax.Location, anchorNameSyntax);
		}
		else if (actionSymbol.ActionKind == ActionKind.Area && IsPositionRelativeToAnchor(changeSymbol.ChangeKind))
		{
			diagnostics.Add(ErrorCode.ERR_AnchorCannotBeArea, anchorNameSyntax.Location, anchorNameSyntax);
		}
		CheckUseBeforeDeclaration(anchorNameSyntax, diagnostics, actionSymbol, changeSymbol);
		CheckActionGetArea(actionSymbol, anchorNameSyntax, diagnostics);
		return actionSymbol;
	}

	public static Symbol GetViewAnchor(this ChangeSymbol changeSymbol, IdentifierNameSyntax anchorNameSyntax, DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = changeSymbol.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		if (changeSymbol.ChangeKind.IsAbsoluteInAnchor())
		{
			return null;
		}
		Symbol symbol = changeSymbol.FindSymbolInTargetPage(targetPage, anchorNameSyntax, (Symbol s) => s.Kind == SymbolKind.View, ErrorCode.ERR_ViewNotFound, ErrorCode.INF_PERS_ViewNotFound, diagnostics);
		if (symbol.Kind == SymbolKind.ErrorType)
		{
			return symbol;
		}
		ViewSymbol viewSymbol = (ViewSymbol)symbol;
		if (IsPositionInsideAnchor(changeSymbol.ChangeKind))
		{
			diagnostics.Add(ErrorCode.ERR_AnchorMustBeGroupType, anchorNameSyntax.Location, anchorNameSyntax);
		}
		CheckUseBeforeDeclaration(anchorNameSyntax, diagnostics, viewSymbol, changeSymbol);
		return viewSymbol;
	}

	internal static ErrorTypeSymbol CreateErrorPageSymbol(this ChangeSymbol moveSymbol, DiagnosticBag diagnostics, ErrorCode code, IdentifierNameSyntax symbolName)
	{
		NavDiagnosticInfo navDiagnosticInfo = null;
		if (!moveSymbol.DeclaringCompilation.Options.Target.IsPersonalizationTarget())
		{
			navDiagnosticInfo = diagnostics.Add(code, symbolName.Location, symbolName);
		}
		navDiagnosticInfo = navDiagnosticInfo ?? new NavDiagnosticInfo(code, symbolName.Location, symbolName);
		return new ExtendedErrorTypeSymbol(moveSymbol.DeclaringCompilation, symbolName.Unquoted(), navDiagnosticInfo);
	}

	private static void CheckActionGetArea(Symbol anchorSymbol, IdentifierNameSyntax anchorNameSyntax, DiagnosticBag diagnostics)
	{
		ActionSymbol actionSymbol = anchorSymbol as ReferenceActionSymbol;
		if (actionSymbol == null)
		{
			return;
		}
		while (actionSymbol != null && actionSymbol.ActionKind != ActionKind.Area)
		{
			if (actionSymbol.ContainingSymbol.Kind == SymbolKind.Change)
			{
				ChangeAddSymbol obj = actionSymbol.ContainingSymbol as ChangeAddSymbol;
				actionSymbol = obj?.Anchor as ActionSymbol;
				DiagnosticInfo diagnosticInfo = obj?.GetUseSiteDiagnostic();
				if (diagnosticInfo != null)
				{
					Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, anchorNameSyntax.Location);
				}
			}
			else
			{
				actionSymbol = actionSymbol.ContainingSymbol as ActionSymbol;
			}
		}
	}

	private static void CheckUseBeforeDeclaration(IdentifierNameSyntax referenceSyntax, DiagnosticBag diagnostics, Symbol referenceSymbol, Symbol referencingSymbol)
	{
		IRootTypeSymbol containingSymbolOfType = referenceSymbol.GetContainingSymbolOfType<IRootTypeSymbol>();
		if (containingSymbolOfType.Kind != SymbolKind.PageCustomization && containingSymbolOfType == referencingSymbol.GetContainingSymbolOfType<IRootTypeSymbol>())
		{
			SyntaxReference syntaxReference = referenceSymbol.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxReference != null && syntaxReference.SyntaxTree == referenceSyntax.SyntaxTree && syntaxReference.Span.Start > referenceSyntax.SpanStart)
			{
				diagnostics.Add(ErrorCode.ERR_UseBeforeDeclaration, referenceSyntax.Location, referenceSyntax);
			}
		}
	}

	private static Symbol GetSymbolFromTargetPage(ChangeSymbol changeSymbol, PageTypeSymbol targetPage, IdentifierNameSyntax symbolName, Func<Symbol, bool> filter, ErrorCode errorCode, ErrorCode personalizationErrorCode, DiagnosticBag diagnostics)
	{
		PooledList<Symbol> instance = PooledList<Symbol>.GetInstance();
		try
		{
			foreach (Symbol pageElementSymbol in changeSymbol.GetPageElementSymbols(targetPage, symbolName))
			{
				if (filter(pageElementSymbol))
				{
					instance.Add(pageElementSymbol);
				}
			}
			Symbol targetSymbolFromList = GetTargetSymbolFromList(changeSymbol, instance, symbolName, errorCode, personalizationErrorCode, diagnostics);
			_ = targetSymbolFromList.Kind;
			return targetSymbolFromList;
		}
		finally
		{
			instance.Free();
		}
	}

	internal static Symbol GetTargetSymbolFromList(ChangeSymbol changeSymbol, List<Symbol> foundSymbols, IdentifierNameSyntax symbolName, ErrorCode missingSymbolErrorCode, ErrorCode personalizationMissingSymbolErrorCode, DiagnosticBag diagnostics)
	{
		switch (foundSymbols.Count)
		{
		case 0:
		{
			ErrorCode code = (changeSymbol.DeclaringCompilation.Options.Target.IsPersonalizationTarget() ? personalizationMissingSymbolErrorCode : missingSymbolErrorCode);
			return changeSymbol.CreateErrorSymbol(diagnostics, code, symbolName);
		}
		case 1:
			return foundSymbols[0];
		default:
		{
			NavDiagnosticInfo diagnosticInfo = ReportAmbiguousReference(changeSymbol.DeclaringCompilation.Options, foundSymbols, symbolName, diagnostics);
			return changeSymbol.CreateErrorSymbol(diagnosticInfo, symbolName);
		}
		}
	}

	private static NavDiagnosticInfo ReportAmbiguousReference(CompilationOptions options, List<Symbol> symbols, IdentifierNameSyntax symbolName, DiagnosticBag diagnostics)
	{
		ErrorCode code = (options.Target.IsPersonalizationTarget() ? ErrorCode.INF_PERS_AmbigSymbolReference : ErrorCode.ERR_AmbigSymbolReference);
		return diagnostics.Add(code, symbolName.Location, symbolName, symbolName, symbols[0].ContainingModule.ToDisplayString(), symbolName, symbols[1].ContainingModule.ToDisplayString());
	}

	public static Symbol GetControlAnchor(this ChangeSymbol changeSymbol, IdentifierNameSyntax anchorNameSyntax, DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = changeSymbol.GetTargetPage();
		if (targetPage == null || anchorNameSyntax.IsMissing)
		{
			return null;
		}
		Symbol symbol = changeSymbol.FindSymbolInTargetPage(targetPage, anchorNameSyntax, (Symbol s) => s.Kind == SymbolKind.Control, ErrorCode.ERR_ControlNotFound, ErrorCode.INF_PERS_ControlNotFound, diagnostics);
		if (symbol.Kind == SymbolKind.ErrorType)
		{
			return symbol;
		}
		ControlSymbol controlSymbol = (ControlSymbol)symbol;
		if (IsPositionInsideAnchor(changeSymbol.ChangeKind) && !controlSymbol.ControlKind.IsGroup())
		{
			diagnostics.Add(ErrorCode.ERR_AnchorMustBeGroupType, anchorNameSyntax.Location, anchorNameSyntax);
		}
		else if (controlSymbol.ControlKind == ControlKind.Area && IsPositionRelativeToAnchor(changeSymbol.ChangeKind))
		{
			diagnostics.Add(ErrorCode.ERR_AnchorCannotBeArea, anchorNameSyntax.Location, anchorNameSyntax);
		}
		CheckUseBeforeDeclaration(anchorNameSyntax, diagnostics, controlSymbol, changeSymbol);
		CheckControlGetArea(controlSymbol, anchorNameSyntax, diagnostics);
		return controlSymbol;
	}

	private static ErrorTypeSymbol CreateErrorSymbol(this ChangeSymbol changeSymbol, DiagnosticBag diagnostics, ErrorCode code, IdentifierNameSyntax symbolName)
	{
		NavDiagnosticInfo diagnosticInfo = diagnostics.Add(code, symbolName.Location, symbolName);
		return changeSymbol.CreateErrorSymbol(diagnosticInfo, symbolName);
	}

	private static ErrorTypeSymbol CreateErrorSymbol(this ChangeSymbol changeSymbol, NavDiagnosticInfo diagnosticInfo, IdentifierNameSyntax symbolName)
	{
		return new ExtendedErrorTypeSymbol(changeSymbol.DeclaringCompilation, symbolName.Unquoted(), diagnosticInfo);
	}

	private static void CheckControlGetArea(Symbol anchorSymbol, IdentifierNameSyntax anchorNameSyntax, DiagnosticBag diagnostics)
	{
		ControlSymbol controlSymbol = anchorSymbol as ReferenceControlSymbol;
		if (controlSymbol == null)
		{
			return;
		}
		while (controlSymbol != null && controlSymbol.ControlKind != ControlKind.Area)
		{
			if (controlSymbol.ContainingSymbol.Kind == SymbolKind.Change)
			{
				ChangeAddSymbol obj = controlSymbol.ContainingSymbol as ChangeAddSymbol;
				controlSymbol = obj?.Anchor as ControlSymbol;
				DiagnosticInfo diagnosticInfo = obj?.GetUseSiteDiagnostic();
				if (diagnosticInfo != null)
				{
					Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, anchorNameSyntax.Location);
				}
			}
			else
			{
				controlSymbol = controlSymbol.ContainingSymbol as ControlSymbol;
			}
		}
	}

	public static bool IsPositionInsideAnchor(ChangeKind kind)
	{
		if ((uint)(kind - 1) <= 1u || (uint)(kind - 5) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPositionRelativeToAnchor(ChangeKind kind)
	{
		if ((uint)(kind - 3) <= 1u || (uint)(kind - 7) <= 1u)
		{
			return true;
		}
		return false;
	}
}
