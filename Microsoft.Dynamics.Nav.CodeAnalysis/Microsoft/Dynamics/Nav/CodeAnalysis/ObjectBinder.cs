using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ObjectBinder : Binder
{
	private MultiDictionary<string, VariableSymbol> globalMap;

	private bool initialized;

	private ContainerSymbol withSymbol;

	private BoundExpression withReceiver;

	internal ObjectTypeSymbol ObjectTypeSymbol { get; }

	internal ContainerSymbol Container => ObjectTypeSymbol;

	internal override Symbol ContainingMember => ObjectTypeSymbol;

	internal SyntaxNode DeclarationSyntax { get; private set; }

	internal ObjectBinder(ObjectTypeSymbol objectSymbol, Binder next, SyntaxNode declarationSyntax)
		: base(next)
	{
		DebugAssertHelper.Assert(declarationSyntax != null);
		DeclarationSyntax = declarationSyntax;
		DebugAssertHelper.Assert((object)objectSymbol != null);
		ObjectTypeSymbol = objectSymbol;
	}

	private void MakeGlobalMap()
	{
		if (initialized)
		{
			return;
		}
		lock (DeclarationSyntax)
		{
			if (initialized)
			{
				return;
			}
			ImmutableArray<VariableSymbol> globalVariables = ObjectTypeSymbol.GlobalVariables;
			if (!globalVariables.IsDefaultOrEmpty)
			{
				globalMap = new MultiDictionary<string, VariableSymbol>(globalVariables.Length, SemanticFacts.NameEqualityComparer);
				foreach (VariableSymbol item in globalVariables)
				{
					globalMap.Add(item.Name, item);
					SynthesizedGlobalVariableSymbol synthesizedGlobalVariableSymbol = item as SynthesizedGlobalVariableSymbol;
					if (synthesizedGlobalVariableSymbol != null && synthesizedGlobalVariableSymbol.HasImplicitWith)
					{
						DebugAssertHelper.Assert(withSymbol == null);
						withSymbol = synthesizedGlobalVariableSymbol.Type;
						withReceiver = new BoundGlobal(ObjectTypeSymbol.GetFirstSyntax(), item, synthesizedGlobalVariableSymbol.Type);
					}
				}
			}
			initialized = true;
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		MakeGlobalMap();
		if (options.CanConsiderGlobals() && globalMap != null)
		{
			foreach (string key in globalMap.Keys)
			{
				foreach (VariableSymbol item in globalMap[key])
				{
					result.AddSymbol(item, item.Name, 0);
				}
			}
		}
		if (withReceiver != null)
		{
			Binder.AddMemberLookupSymbolsInfo(result, withReceiver.Type, options, originalBinder);
		}
		Binder.AddMemberLookupSymbolsInfo(result, ObjectTypeSymbol, options, originalBinder);
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		MakeGlobalMap();
		DebugAssertHelper.Assert(result.IsClear);
		if (withSymbol != null)
		{
			Binder.LookupMembersInternal(result, withSymbol, name, options, originalBinder, diagnose, ref useSiteDiagnostics, withReceiver);
			if (result.IsSingleViable)
			{
				return;
			}
		}
		if (globalMap != null)
		{
			foreach (VariableSymbol item in globalMap[name])
			{
				result.MergeEqual(originalBinder.CheckViability(item, options, null, diagnose, ref useSiteDiagnostics));
			}
		}
		Binder.LookupMembersInternal(result, Container, name, options, originalBinder, diagnose, ref useSiteDiagnostics);
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
