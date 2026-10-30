using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithClassTypeParametersBinder : WithTypeParametersBinder
{
	private readonly NamedTypeSymbol _namedType;

	private MultiDictionary<string, TypeParameterSymbol> _lazyTypeParameterMap;

	protected override MultiDictionary<string, TypeParameterSymbol> TypeParameterMap
	{
		get
		{
			if (_lazyTypeParameterMap == null)
			{
				MultiDictionary<string, TypeParameterSymbol> multiDictionary = new MultiDictionary<string, TypeParameterSymbol>();
				foreach (TypeParameterSymbol typeParameter in _namedType.TypeParameters)
				{
					multiDictionary.Add(typeParameter.Name, typeParameter);
				}
				Interlocked.CompareExchange(ref _lazyTypeParameterMap, multiDictionary, null);
			}
			return _lazyTypeParameterMap;
		}
	}

	internal WithClassTypeParametersBinder(NamedTypeSymbol container, Binder next)
		: base(next)
	{
		_namedType = container;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved)
	{
		return IsSymbolAccessibleConditional(symbol, _namedType, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!CanConsiderTypeParameters(options))
		{
			return;
		}
		foreach (TypeParameterSymbol typeParameter in _namedType.TypeParameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(typeParameter, options, result, null))
			{
				result.AddSymbol(typeParameter, typeParameter.Name, 0);
			}
		}
	}
}
