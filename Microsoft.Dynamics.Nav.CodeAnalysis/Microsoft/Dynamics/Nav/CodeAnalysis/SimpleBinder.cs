using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SimpleBinder<T> : Binder where T : Symbol
{
	private readonly MultiDictionary<string, MethodSymbol> triggerMap;

	internal override Symbol ContainingMember => Symbol;

	internal T Symbol { get; }

	internal SyntaxNode DeclarationSyntax { get; private set; }

	internal SimpleBinder(T symbol, Binder next, SyntaxNode declarationSyntax)
		: this(symbol, next)
	{
		DebugAssertHelper.Assert(declarationSyntax != null);
		DeclarationSyntax = declarationSyntax;
	}

	internal SimpleBinder(T symbol, Binder next)
		: base(next)
	{
		DebugAssertHelper.Assert((object)symbol != null);
		Symbol = symbol;
		if (symbol is ISymbolWithTriggers { Triggers: { IsDefaultOrEmpty: false } triggers })
		{
			triggerMap = new MultiDictionary<string, MethodSymbol>(triggers.Length, SemanticFacts.NameEqualityComparer);
			foreach (IMethodSymbol item in triggers)
			{
				triggerMap.Add(item.Name, (MethodSymbol)item);
			}
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(result.IsClear);
		if (triggerMap == null)
		{
			return;
		}
		foreach (MethodSymbol item in triggerMap[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item, options, null, diagnose, ref useSiteDiagnostics));
		}
	}
}
