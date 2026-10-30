using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class LocalScopeBinder : Binder
{
	private ImmutableArray<VariableSymbol> locals;

	private SmallDictionary<string, VariableSymbol> lazyLocalsMap;

	internal sealed override ImmutableArray<VariableSymbol> Locals
	{
		get
		{
			if (locals.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref locals, BuildLocals(), default);
			}
			return locals;
		}
	}

	private SmallDictionary<string, VariableSymbol> LocalsMap
	{
		get
		{
			if (lazyLocalsMap == null && Locals.Length > 0)
			{
				lazyLocalsMap = BuildMap(Locals);
			}
			return lazyLocalsMap;
		}
	}

	internal LocalScopeBinder(Binder next)
		: this(next, next.Flags)
	{
	}

	internal LocalScopeBinder(Binder next, BinderFlags flags)
		: base(next, flags)
	{
	}

	protected virtual ImmutableArray<VariableSymbol> BuildLocals()
	{
		return ImmutableArray<VariableSymbol>.Empty;
	}

	private static SmallDictionary<string, TSymbol> BuildMap<TSymbol>(ImmutableArray<TSymbol> array) where TSymbol : Symbol
	{
		DebugAssertHelper.Assert(array.Length > 0);
		SmallDictionary<string, TSymbol> smallDictionary = new SmallDictionary<string, TSymbol>();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			TSymbol val = array[num];
			smallDictionary[val.Name] = val;
		}
		return smallDictionary;
	}

	protected virtual ImmutableArray<VariableSymbol> BuildLocals(SyntaxList<StatementSyntax> statements)
	{
		return ImmutableArray<VariableSymbol>.Empty;
	}

	protected override SourceLocalVariableSymbol LookupLocal(SyntaxToken nameToken)
	{
		VariableSymbol value = null;
		if (LocalsMap != null && LocalsMap.TryGetValue(nameToken.ValueText, out value))
		{
			SourceLocalVariableSymbol sourceLocalVariableSymbol = value as SourceLocalVariableSymbol;
			DebugAssertHelper.Assert(sourceLocalVariableSymbol != null);
			if (sourceLocalVariableSymbol.IdentifierToken == nameToken)
			{
				return sourceLocalVariableSymbol;
			}
			foreach (VariableSymbol local in Locals)
			{
				SourceLocalVariableSymbol sourceLocalVariableSymbol2 = local as SourceLocalVariableSymbol;
				DebugAssertHelper.Assert(sourceLocalVariableSymbol2 != null);
				if (sourceLocalVariableSymbol2.IdentifierToken == nameToken)
				{
					return sourceLocalVariableSymbol2;
				}
			}
		}
		return base.LookupLocal(nameToken);
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(options.AreValid());
		DebugAssertHelper.Assert(result.IsClear);
		SmallDictionary<string, VariableSymbol> localsMap = LocalsMap;
		if (localsMap != null && localsMap.TryGetValue(name, out var value))
		{
			result.MergeEqual(originalBinder.CheckViability(value, options, null, diagnose, ref useSiteDiagnostics));
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		DebugAssertHelper.Assert(options.AreValid());
		if (LocalsMap == null || !options.CanConsiderLocals())
		{
			return;
		}
		foreach (KeyValuePair<string, VariableSymbol> item in LocalsMap)
		{
			if (originalBinder.CanAddLookupSymbolInfo(item.Value, options, null))
			{
				result.AddSymbol(item.Value, item.Key, 0);
			}
		}
	}

	internal virtual bool EnsureSingleDefinition(Symbol symbol, string name, Location location, DiagnosticBag diagnostics)
	{
		SmallDictionary<string, VariableSymbol> localsMap = LocalsMap;
		if (localsMap != null && localsMap.TryGetValue(name, out var value))
		{
			_ = symbol == value;
			return false;
		}
		return false;
	}

	internal override ImmutableArray<VariableSymbol> GetDeclaredLocalsForScope(SyntaxNode node)
	{
		throw new NotSupportedException();
	}
}
