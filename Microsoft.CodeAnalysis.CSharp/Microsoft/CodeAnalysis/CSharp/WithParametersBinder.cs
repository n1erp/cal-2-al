using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithParametersBinder : Binder
{
	private readonly ImmutableArray<ParameterSymbol> _parameters;

	internal WithParametersBinder(ImmutableArray<ParameterSymbol> parameters, Binder next)
		: base(next)
	{
		_parameters = parameters;
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderLocals())
		{
			return;
		}
		foreach (ParameterSymbol parameter in _parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.MustBeInvocableIfMember)) != 0)
		{
			return;
		}
		foreach (ParameterSymbol parameter in _parameters)
		{
			if (parameter.Name == name)
			{
				result.MergeEqual(originalBinder.CheckViability(parameter, arity, options, null, diagnose, ref useSiteDiagnostics));
			}
		}
	}
}
