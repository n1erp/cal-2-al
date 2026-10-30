using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ApplicationObjectExtensionBinder : ObjectBinder
{
	private new ApplicationObjectExtensionTypeSymbol ObjectTypeSymbol => (ApplicationObjectExtensionTypeSymbol)base.ObjectTypeSymbol;

	internal ApplicationObjectExtensionBinder(ApplicationObjectExtensionTypeSymbol applicationObjectExtensionSymbol, Binder next, SyntaxNode declarationSyntax)
		: base(applicationObjectExtensionSymbol, next, declarationSyntax)
	{
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		base.LookupSymbolsInSingleBinder(result, name, options, originalBinder, diagnose, ref useSiteDiagnostics);
		if (!result.IsSingleViable && !result.IsMultiViable)
		{
			Binder.LookupMembersInternal(result, ObjectTypeSymbol.Target, name, options, originalBinder, diagnose, ref useSiteDiagnostics, null, ObjectTypeSymbol);
		}
	}
}
