using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SimpleSymbolBinder<T> : SimpleBinder<T> where T : ContainerSymbol
{
	public SimpleSymbolBinder(T symbol, Binder next)
		: base(symbol, next)
	{
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Binder.LookupMembersInternal(result, Symbol, name, options, originalBinder, diagnose, ref useSiteDiagnostics);
	}
}
