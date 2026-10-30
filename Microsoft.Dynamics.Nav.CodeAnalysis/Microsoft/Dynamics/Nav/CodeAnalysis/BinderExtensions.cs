using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class BinderExtensions
{
	internal static IList<Symbol> FindSymbols(this Binder binder, string name, LookupOptions lookupOptions)
	{
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			binder.LookupSymbols(instance, name, ref useSiteDiagnostics, lookupOptions);
			return instance.Symbols.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static IList<Symbol> FindSymbols(this Binder binder, TypeSymbol container, string name, LookupOptions lookupOptions)
	{
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			binder.LookupSymbolsSimpleName(instance, container, name, lookupOptions, diagnose: false, ref useSiteDiagnostics);
			return instance.Symbols.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}
}
