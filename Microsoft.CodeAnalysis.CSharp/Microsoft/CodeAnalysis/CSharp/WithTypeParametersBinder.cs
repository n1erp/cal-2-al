using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class WithTypeParametersBinder : Binder
{
	protected abstract MultiDictionary<string, TypeParameterSymbol> TypeParameterMap { get; }

	protected virtual LookupOptions LookupMask => LookupOptions.NamespaceAliasesOnly | LookupOptions.MustBeInvocableIfMember;

	internal WithTypeParametersBinder(Binder next)
		: base(next)
	{
	}

	protected bool CanConsiderTypeParameters(LookupOptions options)
	{
		return (options & (LookupMask | LookupOptions.MustBeInstance | LookupOptions.LabelsOnly)) == 0;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((options & LookupMask) != 0)
		{
			return;
		}
		foreach (TypeParameterSymbol item in TypeParameterMap[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item, arity, options, null, diagnose, ref useSiteDiagnostics));
		}
	}
}
