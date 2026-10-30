using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class InContainerBinder : Binder
{
	internal ContainerSymbol Container { get; }

	internal override Symbol ContainingMember => Container;

	internal InContainerBinder(ContainerSymbol container, Binder next)
		: base(next)
	{
		DebugAssertHelper.Assert((object)container != null);
		Container = container;
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		NavCorLib.Instance.AddLookupSymbols(result, options, originalBinder);
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(result.IsClear);
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Enum, name);
		for (int i = 0; i < objectSymbolsByNameAcrossModules.Length; i++)
		{
			Symbol symbol = objectSymbolsByNameAcrossModules[i] as Symbol;
			if (originalBinder.CanAddLookupSymbolInfo(symbol, options, null))
			{
				SingleLookupResult result2 = originalBinder.CheckViability(symbol, options, null, diagnose, ref useSiteDiagnostics);
				result.MergeEqual(result2);
			}
		}
		NavCorLib.Instance.LookupSymbol(result, name, options | LookupOptions.MustNotBeInstance | LookupOptions.ValidateSymbolCompilationScope, diagnose, ref useSiteDiagnostics, originalBinder);
	}

	protected override SourceLocalVariableSymbol LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, LookupOptions options, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Container is TypeSymbol within)
		{
			return IsSymbolAccessibleConditional(symbol, within, accessThroughType, options, out failedThroughTypeCheck, ref useSiteDiagnostics);
		}
		return Next.IsAccessibleHelper(symbol, accessThroughType, options, out failedThroughTypeCheck, ref useSiteDiagnostics);
	}
}
