using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class InMethodBinder : LocalScopeBinder
{
	private readonly MultiDictionary<string, ParameterSymbol> parameterMap;

	private readonly MultiDictionary<string, VariableSymbol> variableMap;

	private readonly MethodSymbol methodSymbol;

	private SmallDictionary<string, Symbol> definitionMap;

	private readonly ContainerSymbol withSymbol;

	private readonly BoundExpression withReceiver;

	private static readonly HashSet<string> EmptySet = new HashSet<string>();

	internal override Symbol ContainingMember => methodSymbol;

	internal override bool IsInMethodBody => true;

	public InMethodBinder(MethodSymbol owner, Binder enclosing)
		: base(enclosing)
	{
		DebugAssertHelper.Assert((object)owner != null);
		methodSymbol = owner;
		ImmutableArray<ParameterSymbol> parameters = owner.Parameters;
		if (!parameters.IsDefault && !parameters.IsEmpty)
		{
			RecordDefinition(parameters);
			parameterMap = new MultiDictionary<string, ParameterSymbol>(parameters.Length, SemanticFacts.NameEqualityComparer);
			foreach (ParameterSymbol item in parameters)
			{
				parameterMap.Add(item.Name, item);
			}
		}
		if (owner.ReturnValue != null && owner.ReturnValue.IsNamed)
		{
			RecordDefinition(owner.ReturnValue);
		}
		ImmutableArray<VariableSymbol> localVariables = owner.LocalVariables;
		if (localVariables.IsDefault || localVariables.IsEmpty)
		{
			return;
		}
		RecordDefinition(localVariables);
		variableMap = new MultiDictionary<string, VariableSymbol>(localVariables.Length, SemanticFacts.NameEqualityComparer);
		foreach (VariableSymbol item2 in localVariables)
		{
			variableMap.Add(item2.Name, item2);
			SynthesizedLocalVariableSymbol synthesizedLocalVariableSymbol = item2 as SynthesizedLocalVariableSymbol;
			if (synthesizedLocalVariableSymbol != null && synthesizedLocalVariableSymbol.HasImplicitWith)
			{
				DebugAssertHelper.Assert(withSymbol == null);
				withSymbol = synthesizedLocalVariableSymbol.Type;
				withReceiver = new BoundLocal(owner.GetFirstSyntax(), item2, synthesizedLocalVariableSymbol.Type);
			}
		}
	}

	private void RecordDefinition(Symbol definition)
	{
		SmallDictionary<string, Symbol> smallDictionary = definitionMap ?? (definitionMap = new SmallDictionary<string, Symbol>(SemanticFacts.NameEqualityComparer));
		if (!smallDictionary.ContainsKey(definition.Name))
		{
			smallDictionary.Add(definition.Name, definition);
		}
	}

	private void RecordDefinition<T>(ImmutableArray<T> definitions) where T : Symbol
	{
		SmallDictionary<string, Symbol> smallDictionary = definitionMap ?? (definitionMap = new SmallDictionary<string, Symbol>(SemanticFacts.NameEqualityComparer));
		foreach (T item in definitions)
		{
			if (!smallDictionary.ContainsKey(item.Name))
			{
				smallDictionary.Add(item.Name, item);
			}
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(result.IsClear);
		if (withSymbol != null)
		{
			Binder.LookupMembersInternal(result, withSymbol, name, options, originalBinder, diagnose, ref useSiteDiagnostics, withReceiver);
			if (result.IsSingleViable)
			{
				return;
			}
		}
		if (parameterMap != null)
		{
			foreach (ParameterSymbol item in parameterMap[name])
			{
				result.MergeEqual(originalBinder.CheckViability(item, options, null, diagnose, ref useSiteDiagnostics));
			}
		}
		if (methodSymbol.ReturnValue != null && methodSymbol.ReturnValue.IsNamed && SemanticFacts.NameEqualityComparer.Equals(name, methodSymbol.ReturnValue.Name))
		{
			result.MergeEqual(originalBinder.CheckViability(methodSymbol.ReturnValue, options, null, diagnose, ref useSiteDiagnostics));
		}
		if (variableMap == null)
		{
			return;
		}
		foreach (VariableSymbol item2 in variableMap[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item2, options, null, diagnose, ref useSiteDiagnostics));
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (options.CanConsiderMembers())
		{
			foreach (ParameterSymbol parameter in methodSymbol.Parameters)
			{
				if (originalBinder.CanAddLookupSymbolInfo(parameter, options, null))
				{
					result.AddSymbol(parameter, parameter.Name, 0);
				}
			}
		}
		if (options.CanConsiderLocals() && variableMap != null)
		{
			foreach (string key in variableMap.Keys)
			{
				foreach (VariableSymbol item in variableMap[key])
				{
					result.AddSymbol(item, key, 0);
				}
			}
		}
		if (methodSymbol.ReturnValue != null && methodSymbol.ReturnValue.IsNamed)
		{
			result.AddSymbol(methodSymbol.ReturnValue, methodSymbol.ReturnValue.Name, 0);
		}
		if (withReceiver != null)
		{
			Binder.AddMemberLookupSymbolsInfo(result, withReceiver.Type, options, originalBinder);
		}
	}

	internal static bool ReportConflictWithParameter(Symbol parameter, Symbol newSymbol, string name, Location newLocation, DiagnosticBag diagnostics)
	{
		Location location = parameter.Locations[0];
		DebugAssertHelper.Assert(location != newLocation || location == Location.None, "same nonempty location refers to different symbols?");
		SymbolKind kind = parameter.Kind;
		SymbolKind symbolKind = newSymbol?.Kind ?? SymbolKind.Parameter;
		if (symbolKind == SymbolKind.ErrorType)
		{
			return true;
		}
		if (kind == SymbolKind.Parameter && (symbolKind == SymbolKind.Parameter || symbolKind == SymbolKind.LocalVariable))
		{
			diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
			return true;
		}
		DebugAssertHelper.Assert(condition: false, "what else could be defined in a method?");
		return true;
	}

	internal override bool EnsureSingleDefinition(Symbol symbol, string name, Location location, DiagnosticBag diagnostics)
	{
		SmallDictionary<string, Symbol> smallDictionary = definitionMap;
		if (smallDictionary != null && smallDictionary.TryGetValue(name, out var value))
		{
			return ReportConflictWithParameter(value, symbol, name, location, diagnostics);
		}
		return false;
	}
}
