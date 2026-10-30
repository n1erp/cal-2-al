using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class WithLambdaParametersBinder : LocalScopeBinder
{
	protected readonly LambdaSymbol lambdaSymbol;

	protected readonly MultiDictionary<string, ParameterSymbol> parameterMap;

	private SmallDictionary<string, ParameterSymbol> _definitionMap;

	internal override Symbol ContainingMemberOrLambda => lambdaSymbol;

	internal override bool IsDirectlyInIterator => false;

	internal override uint LocalScopeDepth => 1u;

	public WithLambdaParametersBinder(LambdaSymbol lambdaSymbol, Binder enclosing)
		: base(enclosing)
	{
		this.lambdaSymbol = lambdaSymbol;
		parameterMap = new MultiDictionary<string, ParameterSymbol>();
		ImmutableArray<ParameterSymbol> parameters = lambdaSymbol.Parameters;
		if (!parameters.IsDefaultOrEmpty)
		{
			RecordDefinitions(parameters);
			foreach (ParameterSymbol parameter in lambdaSymbol.Parameters)
			{
				parameterMap.Add(parameter.Name, parameter);
			}
		}
	}

	private void RecordDefinitions(ImmutableArray<ParameterSymbol> definitions)
	{
		SmallDictionary<string, ParameterSymbol> smallDictionary = _definitionMap ?? (_definitionMap = new SmallDictionary<string, ParameterSymbol>());
		foreach (ParameterSymbol item in definitions)
		{
			if (!smallDictionary.ContainsKey(item.Name))
			{
				smallDictionary.Add(item.Name, item);
			}
		}
	}

	protected override TypeSymbol GetCurrentReturnType(out RefKind refKind)
	{
		refKind = lambdaSymbol.RefKind;
		return lambdaSymbol.ReturnType;
	}

	internal override TypeSymbol GetIteratorElementType(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		if (node != null)
		{
			diagnostics.Add(ErrorCode.ERR_YieldInAnonMeth, node.YieldKeyword.GetLocation());
		}
		return CreateErrorType();
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((options & LookupOptions.NamespaceAliasesOnly) != 0)
		{
			return;
		}
		foreach (ParameterSymbol item in parameterMap[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item, arity, options, null, diagnose, ref useSiteDiagnostics));
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderMembers())
		{
			return;
		}
		foreach (ParameterSymbol parameter in lambdaSymbol.Parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	private static bool ReportConflictWithParameter(ParameterSymbol parameter, Symbol newSymbol, string name, Location newLocation, DiagnosticBag diagnostics)
	{
		if (parameter.Locations[0] == newLocation)
		{
			return false;
		}
		_ = parameter.Locations[0];
		_ = parameter.Kind;
		switch (newSymbol?.Kind ?? SymbolKind.Parameter)
		{
		case SymbolKind.ErrorType:
			return true;
		case SymbolKind.Local:
		case SymbolKind.Parameter:
			diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
			return true;
		case SymbolKind.RangeVariable:
			diagnostics.Add(ErrorCode.ERR_QueryRangeVariableOverrides, newLocation, name);
			return true;
		default:
			return false;
		}
	}

	internal override bool EnsureSingleDefinition(Symbol symbol, string name, Location location, DiagnosticBag diagnostics)
	{
		SmallDictionary<string, ParameterSymbol> definitionMap = _definitionMap;
		if (definitionMap != null && definitionMap.TryGetValue(name, out var value))
		{
			return ReportConflictWithParameter(value, symbol, name, location, diagnostics);
		}
		return false;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
