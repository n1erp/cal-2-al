using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class LocalScopeBinder : Binder
{
	private ImmutableArray<LocalSymbol> _locals;

	private ImmutableArray<LocalFunctionSymbol> _localFunctions;

	private ImmutableArray<LabelSymbol> _labels;

	private readonly uint _localScopeDepth;

	private SmallDictionary<string, LocalSymbol> _lazyLocalsMap;

	private SmallDictionary<string, LocalFunctionSymbol> _lazyLocalFunctionsMap;

	private SmallDictionary<string, LabelSymbol> _lazyLabelsMap;

	internal sealed override ImmutableArray<LocalSymbol> Locals
	{
		get
		{
			if (_locals.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _locals, BuildLocals(), default);
			}
			return _locals;
		}
	}

	internal sealed override ImmutableArray<LocalFunctionSymbol> LocalFunctions
	{
		get
		{
			if (_localFunctions.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _localFunctions, BuildLocalFunctions(), default);
			}
			return _localFunctions;
		}
	}

	internal sealed override ImmutableArray<LabelSymbol> Labels
	{
		get
		{
			if (_labels.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _labels, BuildLabels(), default);
			}
			return _labels;
		}
	}

	private SmallDictionary<string, LocalSymbol> LocalsMap
	{
		get
		{
			if (_lazyLocalsMap == null && Locals.Length > 0)
			{
				_lazyLocalsMap = BuildMap(Locals);
			}
			return _lazyLocalsMap;
		}
	}

	private SmallDictionary<string, LocalFunctionSymbol> LocalFunctionsMap
	{
		get
		{
			if (_lazyLocalFunctionsMap == null && LocalFunctions.Length > 0)
			{
				_lazyLocalFunctionsMap = BuildMap(LocalFunctions);
			}
			return _lazyLocalFunctionsMap;
		}
	}

	private SmallDictionary<string, LabelSymbol> LabelsMap
	{
		get
		{
			if (_lazyLabelsMap == null && Labels.Length > 0)
			{
				_lazyLabelsMap = BuildMap(Labels);
			}
			return _lazyLabelsMap;
		}
	}

	internal override uint LocalScopeDepth => _localScopeDepth;

	internal LocalScopeBinder(Binder next)
		: this(next, next.Flags)
	{
	}

	internal LocalScopeBinder(Binder next, BinderFlags flags)
		: base(next, flags)
	{
		uint localScopeDepth = next.LocalScopeDepth;
		if (localScopeDepth != 1)
		{
			_localScopeDepth = localScopeDepth + 1;
			return;
		}
		for (Binder binder = next; binder != null; binder = binder.Next)
		{
			if (binder is InMethodBinder || binder is WithLambdaParametersBinder)
			{
				_localScopeDepth = 1u;
				break;
			}
			if (binder is LocalScopeBinder)
			{
				_localScopeDepth = 2u;
				break;
			}
		}
	}

	protected virtual ImmutableArray<LocalSymbol> BuildLocals()
	{
		return ImmutableArray<LocalSymbol>.Empty;
	}

	protected virtual ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		return ImmutableArray<LocalFunctionSymbol>.Empty;
	}

	protected virtual ImmutableArray<LabelSymbol> BuildLabels()
	{
		return ImmutableArray<LabelSymbol>.Empty;
	}

	private static SmallDictionary<string, TSymbol> BuildMap<TSymbol>(ImmutableArray<TSymbol> array) where TSymbol : Symbol
	{
		SmallDictionary<string, TSymbol> smallDictionary = new SmallDictionary<string, TSymbol>();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			TSymbol val = array[num];
			smallDictionary[val.Name] = val;
		}
		return smallDictionary;
	}

	protected ImmutableArray<LocalSymbol> BuildLocals(SyntaxList<StatementSyntax> statements, Binder enclosingBinder)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		foreach (StatementSyntax item2 in statements)
		{
			StatementSyntax statementSyntax = item2;
			while (statementSyntax.Kind() == SyntaxKind.LabeledStatement)
			{
				statementSyntax = ((LabeledStatementSyntax)statementSyntax).Statement;
			}
			switch (statementSyntax.Kind())
			{
			case SyntaxKind.LocalDeclarationStatement:
			{
				Binder binder2 = enclosingBinder.GetBinder(statementSyntax) ?? enclosingBinder;
				LocalDeclarationStatementSyntax localDeclarationStatementSyntax = (LocalDeclarationStatementSyntax)statementSyntax;
				LocalDeclarationKind kind = ((!localDeclarationStatementSyntax.IsConst) ? LocalDeclarationKind.RegularVariable : LocalDeclarationKind.Constant);
				foreach (VariableDeclaratorSyntax variable in localDeclarationStatementSyntax.Declaration.Variables)
				{
					SourceLocalSymbol item = MakeLocal(localDeclarationStatementSyntax.Declaration, variable, kind, binder2);
					instance.Add(item);
					ExpressionVariableFinder.FindExpressionVariables(this, instance, variable, binder2);
				}
				break;
			}
			case SyntaxKind.ExpressionStatement:
			case SyntaxKind.ReturnStatement:
			case SyntaxKind.YieldReturnStatement:
			case SyntaxKind.ThrowStatement:
			case SyntaxKind.IfStatement:
				ExpressionVariableFinder.FindExpressionVariables(this, instance, statementSyntax, enclosingBinder.GetBinder(statementSyntax) ?? enclosingBinder);
				break;
			case SyntaxKind.SwitchStatement:
			{
				SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)statementSyntax;
				ExpressionVariableFinder.FindExpressionVariables(this, instance, statementSyntax, enclosingBinder.GetBinder(switchStatementSyntax.Expression) ?? enclosingBinder);
				break;
			}
			case SyntaxKind.LockStatement:
			{
				Binder binder = enclosingBinder.GetBinder(statementSyntax);
				ExpressionVariableFinder.FindExpressionVariables(this, instance, statementSyntax, binder);
				break;
			}
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions(SyntaxList<StatementSyntax> statements)
	{
		ArrayBuilder<LocalFunctionSymbol> arrayBuilder = null;
		foreach (StatementSyntax item2 in statements)
		{
			StatementSyntax statementSyntax = item2;
			while (statementSyntax.Kind() == SyntaxKind.LabeledStatement)
			{
				statementSyntax = ((LabeledStatementSyntax)statementSyntax).Statement;
			}
			if (statementSyntax.Kind() == SyntaxKind.LocalFunctionStatement)
			{
				LocalFunctionStatementSyntax declaration = (LocalFunctionStatementSyntax)statementSyntax;
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<LocalFunctionSymbol>.GetInstance();
				}
				LocalFunctionSymbol item = MakeLocalFunction(declaration);
				arrayBuilder.Add(item);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<LocalFunctionSymbol>.Empty;
	}

	protected SourceLocalSymbol MakeLocal(VariableDeclarationSyntax declaration, VariableDeclaratorSyntax declarator, LocalDeclarationKind kind, Binder initializerBinderOpt = null)
	{
		return SourceLocalSymbol.MakeLocal(ContainingMemberOrLambda, this, allowRefKind: true, declaration.Type, declarator.Identifier, kind, declarator.Initializer, initializerBinderOpt);
	}

	protected LocalFunctionSymbol MakeLocalFunction(LocalFunctionStatementSyntax declaration)
	{
		return new LocalFunctionSymbol(this, ContainingMemberOrLambda, declaration);
	}

	protected void BuildLabels(SyntaxList<StatementSyntax> statements, ref ArrayBuilder<LabelSymbol> labels)
	{
		MethodSymbol containingMethod = (MethodSymbol)ContainingMemberOrLambda;
		foreach (StatementSyntax item in statements)
		{
			BuildLabels(containingMethod, item, ref labels);
		}
	}

	internal static void BuildLabels(MethodSymbol containingMethod, StatementSyntax statement, ref ArrayBuilder<LabelSymbol> labels)
	{
		while (statement.Kind() == SyntaxKind.LabeledStatement)
		{
			LabeledStatementSyntax labeledStatementSyntax = (LabeledStatementSyntax)statement;
			if (labels == null)
			{
				labels = ArrayBuilder<LabelSymbol>.GetInstance();
			}
			SourceLabelSymbol item = new SourceLabelSymbol(containingMethod, labeledStatementSyntax.Identifier);
			labels.Add(item);
			statement = labeledStatementSyntax.Statement;
		}
	}

	protected override SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		LocalSymbol value = null;
		if (LocalsMap != null && LocalsMap.TryGetValue(nameToken.ValueText, out value))
		{
			if (value.IdentifierToken == nameToken)
			{
				return (SourceLocalSymbol)value;
			}
			foreach (LocalSymbol local in Locals)
			{
				if (local.IdentifierToken == nameToken)
				{
					return (SourceLocalSymbol)local;
				}
			}
		}
		return base.LookupLocal(nameToken);
	}

	protected override LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		LocalFunctionSymbol value = null;
		if (LocalFunctionsMap != null && LocalFunctionsMap.TryGetValue(nameToken.ValueText, out value))
		{
			if (value.NameToken == nameToken)
			{
				return value;
			}
			foreach (LocalFunctionSymbol localFunction in LocalFunctions)
			{
				if (localFunction.NameToken == nameToken)
				{
					return localFunction;
				}
			}
		}
		return base.LookupLocalFunction(nameToken);
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((options & LookupOptions.LabelsOnly) != 0)
		{
			SmallDictionary<string, LabelSymbol> labelsMap = LabelsMap;
			if (labelsMap != null && labelsMap.TryGetValue(name, out var value))
			{
				result.MergeEqual(LookupResult.Good(value));
			}
			return;
		}
		SmallDictionary<string, LocalSymbol> localsMap = LocalsMap;
		if (localsMap != null && (options & LookupOptions.NamespaceAliasesOnly) == 0 && localsMap.TryGetValue(name, out var value2))
		{
			result.MergeEqual(originalBinder.CheckViability(value2, arity, options, null, diagnose, ref useSiteDiagnostics, basesBeingResolved));
		}
		SmallDictionary<string, LocalFunctionSymbol> localFunctionsMap = LocalFunctionsMap;
		if (localFunctionsMap != null && options.CanConsiderLocals() && localFunctionsMap.TryGetValue(name, out var value3))
		{
			result.MergeEqual(originalBinder.CheckViability(value3, arity, options, null, diagnose, ref useSiteDiagnostics, basesBeingResolved));
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if ((options & LookupOptions.LabelsOnly) != 0 && LabelsMap != null)
		{
			foreach (KeyValuePair<string, LabelSymbol> item in LabelsMap)
			{
				result.AddSymbol(item.Value, item.Key, 0);
			}
		}
		if (!options.CanConsiderLocals())
		{
			return;
		}
		if (LocalsMap != null)
		{
			foreach (KeyValuePair<string, LocalSymbol> item2 in LocalsMap)
			{
				if (originalBinder.CanAddLookupSymbolInfo(item2.Value, options, result, null))
				{
					result.AddSymbol(item2.Value, item2.Key, 0);
				}
			}
		}
		if (LocalFunctionsMap == null)
		{
			return;
		}
		foreach (KeyValuePair<string, LocalFunctionSymbol> item3 in LocalFunctionsMap)
		{
			if (originalBinder.CanAddLookupSymbolInfo(item3.Value, options, result, null))
			{
				result.AddSymbol(item3.Value, item3.Key, 0);
			}
		}
	}

	private bool ReportConflictWithLocal(Symbol local, Symbol newSymbol, string name, Location newLocation, DiagnosticBag diagnostics)
	{
		SymbolKind symbolKind = newSymbol?.Kind ?? SymbolKind.Parameter;
		if (symbolKind == SymbolKind.ErrorType)
		{
			return true;
		}
		if ((0u | ((symbolKind == SymbolKind.Local && Locals.Contains((LocalSymbol)newSymbol)) ? 1u : 0u) | ((symbolKind == SymbolKind.Method && LocalFunctions.Contains((LocalFunctionSymbol)newSymbol)) ? 1u : 0u)) != 0 && newLocation.SourceSpan.Start >= local.Locations[0].SourceSpan.Start)
		{
			diagnostics.Add(ErrorCode.ERR_LocalDuplicate, newLocation, name);
			return true;
		}
		switch (symbolKind)
		{
		case SymbolKind.Local:
		case SymbolKind.Method:
		case SymbolKind.Parameter:
		case SymbolKind.TypeParameter:
			diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
			return true;
		case SymbolKind.RangeVariable:
			diagnostics.Add(ErrorCode.ERR_QueryRangeVariableOverrides, newLocation, name);
			return true;
		default:
			return false;
		}
	}

	internal virtual bool EnsureSingleDefinition(Symbol symbol, string name, Location location, DiagnosticBag diagnostics)
	{
		LocalSymbol value = null;
		LocalFunctionSymbol value2 = null;
		SmallDictionary<string, LocalSymbol> localsMap = LocalsMap;
		SmallDictionary<string, LocalFunctionSymbol> localFunctionsMap = LocalFunctionsMap;
		if ((localsMap != null && localsMap.TryGetValue(name, out value)) || (localFunctionsMap != null && localFunctionsMap.TryGetValue(name, out value2)))
		{
			Symbol symbol2 = (Symbol)(((object)value) ?? ((object)value2));
			if (symbol == symbol2)
			{
				return false;
			}
			return ReportConflictWithLocal(symbol2, symbol, name, location, diagnostics);
		}
		return false;
	}
}
