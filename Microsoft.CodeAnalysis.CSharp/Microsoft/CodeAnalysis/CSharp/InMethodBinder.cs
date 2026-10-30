using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InMethodBinder : LocalScopeBinder
{
	private class IteratorInfo
	{
		public static readonly IteratorInfo Empty = new IteratorInfo(null, default);

		public readonly TypeSymbol ElementType;

		public readonly ImmutableArray<Diagnostic> ElementTypeDiagnostics;

		public IteratorInfo(TypeSymbol elementType, ImmutableArray<Diagnostic> elementTypeDiagnostics)
		{
			ElementType = elementType;
			ElementTypeDiagnostics = elementTypeDiagnostics;
		}
	}

	private MultiDictionary<string, ParameterSymbol> _lazyParameterMap;

	private readonly MethodSymbol _methodSymbol;

	private SmallDictionary<string, Symbol> _lazyDefinitionMap;

	private IteratorInfo _iteratorInfo;

	internal override uint LocalScopeDepth => 1u;

	internal override Symbol ContainingMemberOrLambda => _methodSymbol;

	internal override bool IsInMethodBody => true;

	internal override bool IsDirectlyInIterator => _iteratorInfo != null;

	internal override bool IsIndirectlyInIterator => IsDirectlyInIterator;

	internal override GeneratedLabelSymbol BreakLabel => null;

	internal override GeneratedLabelSymbol ContinueLabel => null;

	public InMethodBinder(MethodSymbol owner, Binder enclosing)
		: base(enclosing, enclosing.Flags & ~BinderFlags.AllClearedAtExecutableCodeBoundary)
	{
		_methodSymbol = owner;
	}

	private static void RecordDefinition<T>(SmallDictionary<string, Symbol> declarationMap, ImmutableArray<T> definitions) where T : Symbol
	{
		foreach (T item in definitions)
		{
			if (!declarationMap.ContainsKey(item.Name))
			{
				declarationMap.Add(item.Name, item);
			}
		}
	}

	protected override SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}

	internal void MakeIterator()
	{
		if (_iteratorInfo == null)
		{
			_iteratorInfo = IteratorInfo.Empty;
		}
	}

	internal override TypeSymbol GetIteratorElementType(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		RefKind refKind = _methodSymbol.RefKind;
		TypeSymbol returnType = _methodSymbol.ReturnType;
		if (!IsDirectlyInIterator)
		{
			return GetIteratorElementTypeFromReturnType(refKind, returnType, node, diagnostics) ?? CreateErrorType();
		}
		if (_iteratorInfo == IteratorInfo.Empty)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			TypeSymbol typeSymbol = GetIteratorElementTypeFromReturnType(refKind, returnType, node, instance);
			if ((object)typeSymbol == null)
			{
				if (refKind != RefKind.None)
				{
					Binder.Error(instance, ErrorCode.ERR_BadIteratorReturnRef, _methodSymbol.Locations[0], _methodSymbol);
				}
				else
				{
					Binder.Error(instance, ErrorCode.ERR_BadIteratorReturn, _methodSymbol.Locations[0], _methodSymbol, returnType);
				}
				typeSymbol = CreateErrorType();
			}
			IteratorInfo value = new IteratorInfo(typeSymbol, instance.ToReadOnlyAndFree());
			Interlocked.CompareExchange(ref _iteratorInfo, value, IteratorInfo.Empty);
		}
		if (node == null)
		{
			diagnostics.AddRange(_iteratorInfo.ElementTypeDiagnostics);
		}
		return _iteratorInfo.ElementType;
	}

	private TypeSymbol GetIteratorElementTypeFromReturnType(RefKind refKind, TypeSymbol returnType, CSharpSyntaxNode errorLocationNode, DiagnosticBag diagnostics)
	{
		if (refKind == RefKind.None && returnType.Kind == SymbolKind.NamedType)
		{
			switch (returnType.OriginalDefinition.SpecialType)
			{
			case SpecialType.System_Collections_IEnumerable:
			case SpecialType.System_Collections_IEnumerator:
				return GetSpecialType(SpecialType.System_Object, diagnostics, errorLocationNode);
			case SpecialType.System_Collections_Generic_IEnumerable_T:
			case SpecialType.System_Collections_Generic_IEnumerator_T:
				return ((NamedTypeSymbol)returnType).TypeArgumentsNoUseSiteDiagnostics[0];
			}
		}
		return null;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (_methodSymbol.ParameterCount == 0 || (options & LookupOptions.NamespaceAliasesOnly) != 0)
		{
			return;
		}
		MultiDictionary<string, ParameterSymbol> multiDictionary = _lazyParameterMap;
		if (multiDictionary == null)
		{
			ImmutableArray<ParameterSymbol> parameters = _methodSymbol.Parameters;
			multiDictionary = new MultiDictionary<string, ParameterSymbol>(parameters.Length, EqualityComparer<string>.Default);
			foreach (ParameterSymbol item in parameters)
			{
				multiDictionary.Add(item.Name, item);
			}
			_lazyParameterMap = multiDictionary;
		}
		foreach (ParameterSymbol item2 in multiDictionary[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item2, arity, options, null, diagnose, ref useSiteDiagnostics));
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderMembers())
		{
			return;
		}
		foreach (ParameterSymbol parameter in _methodSymbol.Parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	internal static bool ReportConflictWithParameter(Symbol parameter, Symbol newSymbol, string name, Location newLocation, DiagnosticBag diagnostics)
	{
		_ = parameter.Locations[0];
		SymbolKind kind = parameter.Kind;
		SymbolKind symbolKind = newSymbol?.Kind ?? SymbolKind.Parameter;
		if (symbolKind == SymbolKind.ErrorType)
		{
			return true;
		}
		if (kind == SymbolKind.Parameter)
		{
			if (symbolKind == SymbolKind.Parameter || symbolKind == SymbolKind.Local || (symbolKind == SymbolKind.Method && ((MethodSymbol)newSymbol).MethodKind == MethodKind.LocalFunction))
			{
				diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
				return true;
			}
			if (symbolKind == SymbolKind.RangeVariable)
			{
				diagnostics.Add(ErrorCode.ERR_QueryRangeVariableOverrides, newLocation, name);
				return true;
			}
		}
		if (kind == SymbolKind.TypeParameter)
		{
			if (symbolKind == SymbolKind.Parameter || symbolKind == SymbolKind.Local || (symbolKind == SymbolKind.Method && ((MethodSymbol)newSymbol).MethodKind == MethodKind.LocalFunction))
			{
				diagnostics.Add(ErrorCode.ERR_LocalSameNameAsTypeParam, newLocation, name);
				return true;
			}
			switch (symbolKind)
			{
			case SymbolKind.TypeParameter:
				return false;
			case SymbolKind.RangeVariable:
				diagnostics.Add(ErrorCode.ERR_QueryRangeVariableSameAsTypeParam, newLocation, name);
				return true;
			}
		}
		return true;
	}

	internal override bool EnsureSingleDefinition(Symbol symbol, string name, Location location, DiagnosticBag diagnostics)
	{
		ImmutableArray<ParameterSymbol> parameters = _methodSymbol.Parameters;
		ImmutableArray<TypeParameterSymbol> typeParameters = _methodSymbol.TypeParameters;
		if (parameters.IsEmpty && typeParameters.IsEmpty)
		{
			return false;
		}
		SmallDictionary<string, Symbol> smallDictionary = _lazyDefinitionMap;
		if (smallDictionary == null)
		{
			smallDictionary = new SmallDictionary<string, Symbol>();
			RecordDefinition(smallDictionary, parameters);
			RecordDefinition(smallDictionary, typeParameters);
			_lazyDefinitionMap = smallDictionary;
		}
		if (smallDictionary.TryGetValue(name, out var value))
		{
			return ReportConflictWithParameter(value, symbol, name, location, diagnostics);
		}
		return false;
	}
}
