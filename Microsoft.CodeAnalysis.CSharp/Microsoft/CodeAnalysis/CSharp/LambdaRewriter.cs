using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LambdaRewriter : MethodToClassRewriter
{
	internal sealed class Analysis : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		[DebuggerDisplay("{ToString(), nq}")]
		public sealed class Scope
		{
			public readonly Scope Parent;

			public readonly ArrayBuilder<Scope> NestedScopes = ArrayBuilder<Scope>.GetInstance();

			public readonly ArrayBuilder<Closure> Closures = ArrayBuilder<Closure>.GetInstance();

			public readonly SetWithInsertionOrder<Symbol> DeclaredVariables = new SetWithInsertionOrder<Symbol>();

			public readonly BoundNode BoundNode;

			public readonly Closure ContainingClosureOpt;

			public readonly ArrayBuilder<ClosureEnvironment> DeclaredEnvironments = ArrayBuilder<ClosureEnvironment>.GetInstance();

			public Scope(Scope parent, BoundNode boundNode, Closure containingClosure)
			{
				Parent = parent;
				BoundNode = boundNode;
				ContainingClosureOpt = containingClosure;
			}

			public void Free()
			{
				foreach (Scope nestedScope in NestedScopes)
				{
					nestedScope.Free();
				}
				NestedScopes.Free();
				foreach (Closure closure in Closures)
				{
					closure.Free();
				}
				Closures.Free();
				DeclaredEnvironments.Free();
			}

			public override string ToString()
			{
				return BoundNode.Syntax.GetText().ToString();
			}
		}

		public sealed class Closure
		{
			public readonly MethodSymbol OriginalMethodSymbol;

			public readonly SyntaxReference BlockSyntax;

			public readonly PooledHashSet<Symbol> CapturedVariables = PooledHashSet<Symbol>.GetInstance();

			public readonly ArrayBuilder<ClosureEnvironment> CapturedEnvironments = ArrayBuilder<ClosureEnvironment>.GetInstance();

			public ClosureEnvironment ContainingEnvironmentOpt;

			private bool _capturesThis;

			public SynthesizedClosureMethod SynthesizedLoweredMethod;

			public bool CapturesThis
			{
				get
				{
					return _capturesThis;
				}
				set
				{
					_capturesThis = value;
				}
			}

			public Closure(MethodSymbol symbol, SyntaxReference blockSyntax)
			{
				OriginalMethodSymbol = symbol;
				BlockSyntax = blockSyntax;
			}

			public void Free()
			{
				CapturedVariables.Free();
				CapturedEnvironments.Free();
			}
		}

		public sealed class ClosureEnvironment
		{
			public readonly SetWithInsertionOrder<Symbol> CapturedVariables;

			public bool CapturesParent;

			public readonly bool IsStruct;

			internal SynthesizedClosureEnvironment SynthesizedEnvironment;

			public ClosureEnvironment(IEnumerable<Symbol> capturedVariables, bool isStruct)
			{
				CapturedVariables = new SetWithInsertionOrder<Symbol>();
				foreach (Symbol capturedVariable in capturedVariables)
				{
					CapturedVariables.Add(capturedVariable);
				}
				IsStruct = isStruct;
			}
		}

		private class ScopeTreeBuilder : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
		{
			private Scope _currentScope;

			private Closure _currentClosure;

			private bool _inExpressionTree;

			private readonly SmallDictionary<Symbol, Scope> _localToScope = new SmallDictionary<Symbol, Scope>();

			private readonly MethodSymbol _topLevelMethod;

			private readonly HashSet<MethodSymbol> _methodsConvertedToDelegates;

			private readonly DiagnosticBag _diagnostics;

			private ScopeTreeBuilder(Scope rootScope, MethodSymbol topLevelMethod, HashSet<MethodSymbol> methodsConvertedToDelegates, DiagnosticBag diagnostics)
			{
				_currentScope = rootScope;
				_topLevelMethod = topLevelMethod;
				_methodsConvertedToDelegates = methodsConvertedToDelegates;
				_diagnostics = diagnostics;
			}

			public static Scope Build(BoundNode node, MethodSymbol topLevelMethod, HashSet<MethodSymbol> methodsConvertedToDelegates, DiagnosticBag diagnostics)
			{
				Scope scope = new Scope(null, node, null);
				new ScopeTreeBuilder(scope, topLevelMethod, methodsConvertedToDelegates, diagnostics).Build();
				return scope;
			}

			private void Build()
			{
				DeclareLocals(_currentScope, _topLevelMethod.Parameters);
				if (_topLevelMethod.TryGetThisParameter(out var thisParameter))
				{
					DeclareLocals(_currentScope, ImmutableArray.Create((Symbol)thisParameter));
				}
				Visit(_currentScope.BoundNode);
			}

			public override BoundNode VisitMethodGroup(BoundMethodGroup node)
			{
				throw ExceptionUtilities.Unreachable;
			}

			public override BoundNode VisitBlock(BoundBlock node)
			{
				Scope currentScope = _currentScope;
				_currentScope = CreateOrReuseScope(node, node.Locals);
				BoundNode result = base.VisitBlock(node);
				_currentScope = currentScope;
				return result;
			}

			public override BoundNode VisitCatchBlock(BoundCatchBlock node)
			{
				Scope currentScope = _currentScope;
				_currentScope = CreateOrReuseScope(node, node.Locals);
				BoundNode result = base.VisitCatchBlock(node);
				_currentScope = currentScope;
				return result;
			}

			public override BoundNode VisitSequence(BoundSequence node)
			{
				Scope currentScope = _currentScope;
				_currentScope = CreateOrReuseScope(node, node.Locals);
				BoundNode result = base.VisitSequence(node);
				_currentScope = currentScope;
				return result;
			}

			public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
			{
				Scope currentScope = _currentScope;
				_currentScope = CreateOrReuseScope(node, node.InnerLocals);
				BoundNode result = base.VisitSwitchStatement(node);
				_currentScope = currentScope;
				return result;
			}

			public override BoundNode VisitLambda(BoundLambda node)
			{
				bool inExpressionTree = _inExpressionTree;
				_inExpressionTree |= node.Type.IsExpressionTree();
				_methodsConvertedToDelegates.Add(node.Symbol.OriginalDefinition);
				BoundNode result = VisitClosure(node.Symbol, node.Body);
				_inExpressionTree = inExpressionTree;
				return result;
			}

			public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
			{
				return VisitClosure(node.Symbol.OriginalDefinition, node.Body);
			}

			public override BoundNode VisitCall(BoundCall node)
			{
				if (node.Method.MethodKind == MethodKind.LocalFunction)
				{
					AddIfCaptured(node.Method.OriginalDefinition, node.Syntax);
				}
				return base.VisitCall(node);
			}

			public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
			{
				MethodSymbol methodOpt = node.MethodOpt;
				if ((object)methodOpt != null && methodOpt.MethodKind == MethodKind.LocalFunction)
				{
					MethodSymbol originalDefinition = node.MethodOpt.OriginalDefinition;
					AddIfCaptured(originalDefinition, node.Syntax);
					_methodsConvertedToDelegates.Add(originalDefinition);
				}
				return base.VisitDelegateCreationExpression(node);
			}

			public override BoundNode VisitParameter(BoundParameter node)
			{
				AddIfCaptured(node.ParameterSymbol, node.Syntax);
				return base.VisitParameter(node);
			}

			public override BoundNode VisitLocal(BoundLocal node)
			{
				AddIfCaptured(node.LocalSymbol, node.Syntax);
				return base.VisitLocal(node);
			}

			public override BoundNode VisitBaseReference(BoundBaseReference node)
			{
				AddIfCaptured(_topLevelMethod.ThisParameter, node.Syntax);
				return base.VisitBaseReference(node);
			}

			public override BoundNode VisitThisReference(BoundThisReference node)
			{
				ParameterSymbol thisParameter = _topLevelMethod.ThisParameter;
				if (thisParameter != null)
				{
					AddIfCaptured(thisParameter, node.Syntax);
				}
				return base.VisitThisReference(node);
			}

			private BoundNode VisitClosure(MethodSymbol closureSymbol, BoundBlock body)
			{
				Closure closure = new Closure(closureSymbol, body.Syntax.GetReference());
				_currentScope.Closures.Add(closure);
				Closure currentClosure = _currentClosure;
				_currentClosure = closure;
				Scope currentScope = _currentScope;
				_currentScope = CreateNestedScope(body, _currentScope, _currentClosure);
				DeclareLocals(_currentScope, closureSymbol.Parameters, _inExpressionTree);
				BoundNode result = (_inExpressionTree ? base.VisitBlock(body) : VisitBlock(body));
				_currentScope = currentScope;
				_currentClosure = currentClosure;
				return result;
			}

			private void AddIfCaptured(Symbol symbol, SyntaxNode syntax)
			{
				if (_currentClosure == null || symbol is LocalSymbol { IsConst: not false } || (symbol is MethodSymbol methodSymbol && _currentClosure.OriginalMethodSymbol == methodSymbol) || !(symbol.ContainingSymbol != _currentClosure.OriginalMethodSymbol))
				{
					return;
				}
				AddDiagnosticIfRestrictedType(symbol, syntax);
				Scope scope = _currentScope;
				Closure closure = _currentClosure;
				while (closure != null && symbol.ContainingSymbol != closure.OriginalMethodSymbol)
				{
					closure.CapturedVariables.Add(symbol);
					while (scope.ContainingClosureOpt == closure)
					{
						scope = scope.Parent;
					}
					closure = scope.ContainingClosureOpt;
				}
				if (symbol.Kind != SymbolKind.Method && _localToScope.TryGetValue(symbol, out var value))
				{
					value.DeclaredVariables.Add(symbol);
				}
			}

			private void AddDiagnosticIfRestrictedType(Symbol capturedVariable, SyntaxNode syntax)
			{
				TypeSymbol type;
				switch (capturedVariable.Kind)
				{
				default:
					return;
				case SymbolKind.Local:
					type = ((LocalSymbol)capturedVariable).Type;
					break;
				case SymbolKind.Parameter:
					type = ((ParameterSymbol)capturedVariable).Type;
					break;
				}
				if (type.IsRestrictedType())
				{
					_diagnostics.Add(ErrorCode.ERR_SpecialByRefInLambda, syntax.Location, type);
				}
			}

			private Scope CreateOrReuseScope<TSymbol>(BoundNode node, ImmutableArray<TSymbol> locals) where TSymbol : Symbol
			{
				Scope scope = ((!locals.IsEmpty && _currentScope.BoundNode != node) ? CreateNestedScope(node, _currentScope, _currentClosure) : _currentScope);
				DeclareLocals(scope, locals);
				return scope;
			}

			private static Scope CreateNestedScope(BoundNode node, Scope parentScope, Closure currentClosure)
			{
				Scope scope = new Scope(parentScope, node, currentClosure);
				parentScope.NestedScopes.Add(scope);
				return scope;
			}

			private void DeclareLocals<TSymbol>(Scope scope, ImmutableArray<TSymbol> locals, bool declareAsFree = false) where TSymbol : Symbol
			{
				foreach (TSymbol item in locals)
				{
					if (!declareAsFree)
					{
						_localToScope.Add(item, scope);
					}
				}
			}
		}

		public readonly PooledHashSet<MethodSymbol> MethodsConvertedToDelegates;

		public readonly Scope ScopeTree;

		private readonly MethodSymbol _topLevelMethod;

		private readonly int _topLevelMethodOrdinal;

		private readonly MethodSymbol _substitutedSourceMethod;

		private readonly VariableSlotAllocator _slotAllocatorOpt;

		private readonly TypeCompilationState _compilationState;

		public bool CanTakeRefParameters(MethodSymbol closure)
		{
			if (!closure.IsAsync && !closure.IsIterator)
			{
				return !MethodsConvertedToDelegates.Contains(closure);
			}
			return false;
		}

		private Analysis(Scope scopeTree, PooledHashSet<MethodSymbol> methodsConvertedToDelegates, MethodSymbol topLevelMethod, int topLevelMethodOrdinal, MethodSymbol substitutedSourceMethod, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState)
		{
			ScopeTree = scopeTree;
			MethodsConvertedToDelegates = methodsConvertedToDelegates;
			_topLevelMethod = topLevelMethod;
			_topLevelMethodOrdinal = topLevelMethodOrdinal;
			_substitutedSourceMethod = substitutedSourceMethod;
			_slotAllocatorOpt = slotAllocatorOpt;
			_compilationState = compilationState;
		}

		public static Analysis Analyze(BoundNode node, MethodSymbol method, int topLevelMethodOrdinal, MethodSymbol substitutedSourceMethod, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, ArrayBuilder<ClosureDebugInfo> closureDebugInfo, DiagnosticBag diagnostics)
		{
			PooledHashSet<MethodSymbol> instance = PooledHashSet<MethodSymbol>.GetInstance();
			Analysis analysis = new Analysis(ScopeTreeBuilder.Build(node, method, instance, diagnostics), instance, method, topLevelMethodOrdinal, substitutedSourceMethod, slotAllocatorOpt, compilationState);
			analysis.MakeAndAssignEnvironments();
			analysis.ComputeLambdaScopesAndFrameCaptures(method.ThisParameter);
			analysis.InlineThisOnlyEnvironments();
			return analysis;
		}

		private static BoundNode FindNodeToAnalyze(BoundNode node)
		{
			while (true)
			{
				switch (node.Kind)
				{
				case BoundKind.SequencePoint:
					node = ((BoundSequencePoint)node).StatementOpt;
					break;
				case BoundKind.SequencePointWithSpan:
					node = ((BoundSequencePointWithSpan)node).StatementOpt;
					break;
				case BoundKind.FieldEqualsValue:
				case BoundKind.Block:
				case BoundKind.StatementList:
					return node;
				case BoundKind.GlobalStatementInitializer:
					return ((BoundGlobalStatementInitializer)node).Statement;
				default:
					throw ExceptionUtilities.UnexpectedValue(node.Kind);
				}
			}
		}

		private void ComputeLambdaScopesAndFrameCaptures(ParameterSymbol thisParam)
		{
			VisitClosures(ScopeTree, (Scope scope, Closure closure) =>
			{
				if (closure.CapturedEnvironments.Count > 0)
				{
					PooledHashSet<ClosureEnvironment> instance = PooledHashSet<ClosureEnvironment>.GetInstance();
					instance.AddAll(closure.CapturedEnvironments);
					Scope scope2;
					for (scope2 = scope; scope2 != null; scope2 = scope2.Parent)
					{
						if (instance.RemoveAll(scope2.DeclaredEnvironments))
						{
							ClosureEnvironment closureEnvironment = scope2.DeclaredEnvironments[0];
							if (!closureEnvironment.IsStruct)
							{
								closure.ContainingEnvironmentOpt = closureEnvironment;
								break;
							}
						}
					}
					ClosureEnvironment closureEnvironment2 = scope2?.DeclaredEnvironments[0];
					while (scope2 != null && instance.Count != 0)
					{
						IEnumerable<ClosureEnvironment> source = scope2.DeclaredEnvironments.Where((ClosureEnvironment e) => !e.IsStruct);
						if (!source.IsEmpty())
						{
							ClosureEnvironment closureEnvironment3 = source.First();
							closureEnvironment2.CapturesParent = true;
							closureEnvironment2 = closureEnvironment3;
						}
						instance.RemoveAll(scope2.DeclaredEnvironments);
						scope2 = scope2.Parent;
					}
					if (instance.Count > 0)
					{
						throw ExceptionUtilities.Unreachable;
					}
					instance.Free();
				}
			});
		}

		private void InlineThisOnlyEnvironments()
		{
			if (!_topLevelMethod.TryGetThisParameter(out var thisParameter) || thisParameter == null)
			{
				return;
			}
			ArrayBuilder<ClosureEnvironment> topLevelEnvs = ScopeTree.DeclaredEnvironments;
			if (topLevelEnvs.Count == 0)
			{
				return;
			}
			ClosureEnvironment env = topLevelEnvs[0];
			if (env.CapturedVariables.Count > 1 || !env.CapturedVariables.Contains(thisParameter))
			{
				return;
			}
			if (env.IsStruct)
			{
				if (!CheckClosures(ScopeTree, (Scope scope, Closure closure) => closure.CapturedEnvironments.Contains(env) && closure.ContainingEnvironmentOpt != null))
				{
					RemoveEnv();
				}
				return;
			}
			RemoveEnv();
			VisitClosures(ScopeTree, (Scope scope, Closure closure) =>
			{
				if (closure.ContainingEnvironmentOpt == env)
				{
					closure.ContainingEnvironmentOpt = null;
				}
			});
			void RemoveEnv()
			{
				topLevelEnvs.RemoveAt(topLevelEnvs.IndexOf(env));
				VisitClosures(ScopeTree, (Scope scope, Closure closure) =>
				{
					int num = closure.CapturedEnvironments.IndexOf(env);
					if (num >= 0)
					{
						closure.CapturedEnvironments.RemoveAt(num);
					}
				});
			}
		}

		private void MakeAndAssignEnvironments()
		{
			VisitScopeTree(ScopeTree, (Scope scope) =>
			{
				SetWithInsertionOrder<Symbol> declaredVariables = scope.DeclaredVariables;
				if (declaredVariables.Count == 0)
				{
					return;
				}
				bool isStruct = true;
				SetWithInsertionOrder<Closure> closures = new SetWithInsertionOrder<Closure>();
				bool addedItem;
				do
				{
					addedItem = false;
					VisitClosures(scope, (Scope closureScope, Closure closure) =>
					{
						if (!closures.Contains(closure) && (closure.CapturedVariables.Overlaps(scope.DeclaredVariables) || closure.CapturedVariables.Overlaps(closures.Select((Closure c) => c.OriginalMethodSymbol))))
						{
							closures.Add(closure);
							addedItem = true;
							isStruct &= CanTakeRefParameters(closure.OriginalMethodSymbol);
						}
					});
				}
				while (addedItem);
				ClosureEnvironment closureEnvironment = new ClosureEnvironment(declaredVariables, isStruct);
				scope.DeclaredEnvironments.Add(closureEnvironment);
				_topLevelMethod.TryGetThisParameter(out var thisParameter);
				foreach (Closure item in closures)
				{
					item.CapturedEnvironments.Add(closureEnvironment);
					if (thisParameter != null && closureEnvironment.CapturedVariables.Contains(thisParameter))
					{
						item.CapturesThis = true;
					}
				}
			});
		}

		internal DebugId GetTopLevelMethodId()
		{
			return _slotAllocatorOpt?.MethodId ?? new DebugId(_topLevelMethodOrdinal, _compilationState.ModuleBuilderOpt.CurrentGenerationOrdinal);
		}

		internal DebugId GetClosureId(SyntaxNode syntax, ArrayBuilder<ClosureDebugInfo> closureDebugInfo)
		{
			DebugId debugId = ((_slotAllocatorOpt == null || !_slotAllocatorOpt.TryGetPreviousClosure(syntax, out var closureId)) ? new DebugId(closureDebugInfo.Count, _compilationState.ModuleBuilderOpt.CurrentGenerationOrdinal) : closureId);
			int syntaxOffset = _topLevelMethod.CalculateLocalSyntaxOffset(syntax.SpanStart, syntax.SyntaxTree);
			closureDebugInfo.Add(new ClosureDebugInfo(syntaxOffset, debugId));
			return debugId;
		}

		public static Scope GetVariableDeclarationScope(Scope startingScope, Symbol variable)
		{
			if (variable is ParameterSymbol { IsThis: not false })
			{
				return null;
			}
			for (Scope scope = startingScope; scope != null; scope = scope.Parent)
			{
				switch (variable.Kind)
				{
				case SymbolKind.Local:
				case SymbolKind.Parameter:
					if (scope.DeclaredVariables.Contains(variable))
					{
						return scope;
					}
					break;
				case SymbolKind.Method:
					foreach (Closure closure in scope.Closures)
					{
						if (closure.OriginalMethodSymbol == variable)
						{
							return scope;
						}
					}
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(variable.Kind);
				}
			}
			return null;
		}

		public static Scope GetScopeParent(Scope treeRoot, BoundNode scopeNode)
		{
			return GetScopeWithMatchingBoundNode(treeRoot, scopeNode).Parent;
		}

		public static Scope GetScopeWithMatchingBoundNode(Scope treeRoot, BoundNode node)
		{
			return Helper(treeRoot) ?? throw ExceptionUtilities.Unreachable;
			Scope Helper(Scope currentScope)
			{
				if (currentScope.BoundNode == node)
				{
					return currentScope;
				}
				foreach (Scope nestedScope in currentScope.NestedScopes)
				{
					Scope scope = Helper(nestedScope);
					if (scope != null)
					{
						return scope;
					}
				}
				return null;
			}
		}

		public static (Closure, Scope) GetVisibleClosure(Scope startingScope, MethodSymbol closureSymbol)
		{
			for (Scope scope = startingScope; scope != null; scope = scope.Parent)
			{
				foreach (Closure closure in scope.Closures)
				{
					if (closure.OriginalMethodSymbol == closureSymbol)
					{
						return (closure, scope);
					}
				}
			}
			throw ExceptionUtilities.Unreachable;
		}

		public static Closure GetClosureInTree(Scope treeRoot, MethodSymbol closureSymbol)
		{
			return Helper(treeRoot) ?? throw ExceptionUtilities.Unreachable;
			Closure Helper(Scope scope)
			{
				foreach (Closure closure2 in scope.Closures)
				{
					if (closure2.OriginalMethodSymbol == closureSymbol)
					{
						return closure2;
					}
				}
				foreach (Scope nestedScope in scope.NestedScopes)
				{
					Closure closure = Helper(nestedScope);
					if (closure != null)
					{
						return closure;
					}
				}
				return null;
			}
		}

		public void Free()
		{
			MethodsConvertedToDelegates.Free();
			ScopeTree.Free();
		}

		public static void VisitClosures(Scope scope, Action<Scope, Closure> action)
		{
			foreach (Closure closure in scope.Closures)
			{
				action(scope, closure);
			}
			foreach (Scope nestedScope in scope.NestedScopes)
			{
				VisitClosures(nestedScope, action);
			}
		}

		public static bool CheckClosures(Scope scope, Func<Scope, Closure, bool> func)
		{
			foreach (Closure closure in scope.Closures)
			{
				if (func(scope, closure))
				{
					return true;
				}
			}
			foreach (Scope nestedScope in scope.NestedScopes)
			{
				if (CheckClosures(nestedScope, func))
				{
					return true;
				}
			}
			return false;
		}

		public static void VisitScopeTree(Scope treeRoot, Action<Scope> action)
		{
			action(treeRoot);
			foreach (Scope nestedScope in treeRoot.NestedScopes)
			{
				VisitScopeTree(nestedScope, action);
			}
		}
	}

	private readonly Analysis _analysis;

	private readonly MethodSymbol _topLevelMethod;

	private readonly MethodSymbol _substitutedSourceMethod;

	private readonly int _topLevelMethodOrdinal;

	private SynthesizedClosureEnvironment _lazyStaticLambdaFrame;

	private readonly Dictionary<ParameterSymbol, ParameterSymbol> _parameterMap = new Dictionary<ParameterSymbol, ParameterSymbol>();

	private readonly Dictionary<BoundNode, Analysis.ClosureEnvironment> _frames = new Dictionary<BoundNode, Analysis.ClosureEnvironment>();

	private readonly Dictionary<NamedTypeSymbol, Symbol> _framePointers = new Dictionary<NamedTypeSymbol, Symbol>();

	private readonly HashSet<LocalSymbol> _assignLocals;

	private MethodSymbol _currentMethod;

	private ParameterSymbol _currentFrameThis;

	private readonly ArrayBuilder<LambdaDebugInfo> _lambdaDebugInfoBuilder;

	private int _synthesizedFieldNameIdDispenser;

	private Symbol _innermostFramePointer;

	private TypeMap _currentLambdaBodyTypeMap;

	private ImmutableArray<TypeParameterSymbol> _currentTypeParameters;

	private BoundExpression _thisProxyInitDeferred;

	private bool _seenBaseCall;

	private bool _inExpressionLambda;

	private ArrayBuilder<LocalSymbol> _addedLocals;

	private ArrayBuilder<BoundStatement> _addedStatements;

	private ArrayBuilder<TypeCompilationState.MethodWithBody> _synthesizedMethods;

	private readonly ImmutableHashSet<Symbol> _allCapturedVariables;

	protected override TypeMap TypeMap => _currentLambdaBodyTypeMap;

	protected override MethodSymbol CurrentMethod => _currentMethod;

	protected override NamedTypeSymbol ContainingType => _topLevelMethod.ContainingType;

	private LambdaRewriter(Analysis analysis, NamedTypeSymbol thisType, ParameterSymbol thisParameterOpt, MethodSymbol method, int methodOrdinal, MethodSymbol substitutedSourceMethod, ArrayBuilder<LambdaDebugInfo> lambdaDebugInfoBuilder, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics, HashSet<LocalSymbol> assignLocals)
		: base(slotAllocatorOpt, compilationState, diagnostics)
	{
		_topLevelMethod = method;
		_substitutedSourceMethod = substitutedSourceMethod;
		_topLevelMethodOrdinal = methodOrdinal;
		_lambdaDebugInfoBuilder = lambdaDebugInfoBuilder;
		_currentMethod = method;
		_analysis = analysis;
		_assignLocals = assignLocals;
		_currentTypeParameters = method.TypeParameters;
		_currentLambdaBodyTypeMap = TypeMap.Empty;
		_innermostFramePointer = (_currentFrameThis = thisParameterOpt);
		_framePointers[thisType] = thisParameterOpt;
		_seenBaseCall = method.MethodKind != MethodKind.Constructor;
		_synthesizedFieldNameIdDispenser = 1;
		ImmutableHashSet<Symbol>.Builder allCapturedVars = ImmutableHashSet.CreateBuilder<Symbol>();
		Analysis.VisitClosures(analysis.ScopeTree, (Analysis.Scope scope, Analysis.Closure closure) =>
		{
			allCapturedVars.UnionWith(closure.CapturedVariables);
		});
		_allCapturedVariables = allCapturedVars.ToImmutable();
	}

	protected override bool NeedsProxy(Symbol localOrParameter)
	{
		return _allCapturedVariables.Contains(localOrParameter);
	}

	public static BoundStatement Rewrite(BoundStatement loweredBody, NamedTypeSymbol thisType, ParameterSymbol thisParameter, MethodSymbol method, int methodOrdinal, MethodSymbol substitutedSourceMethod, ArrayBuilder<LambdaDebugInfo> lambdaDebugInfoBuilder, ArrayBuilder<ClosureDebugInfo> closureDebugInfoBuilder, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics, HashSet<LocalSymbol> assignLocals)
	{
		Analysis analysis = Analysis.Analyze(loweredBody, method, methodOrdinal, substitutedSourceMethod, slotAllocatorOpt, compilationState, closureDebugInfoBuilder, diagnostics);
		LambdaRewriter lambdaRewriter = new LambdaRewriter(analysis, thisType, thisParameter, method, methodOrdinal, substitutedSourceMethod, lambdaDebugInfoBuilder, slotAllocatorOpt, compilationState, diagnostics, assignLocals);
		lambdaRewriter.SynthesizeClosureEnvironments(closureDebugInfoBuilder);
		lambdaRewriter.SynthesizeLoweredFunctionMethods();
		BoundStatement result = lambdaRewriter.AddStatementsIfNeeded((BoundStatement)lambdaRewriter.Visit(loweredBody));
		if (lambdaRewriter._synthesizedMethods != null)
		{
			if (compilationState.SynthesizedMethods == null)
			{
				compilationState.SynthesizedMethods = lambdaRewriter._synthesizedMethods;
			}
			else
			{
				compilationState.SynthesizedMethods.AddRange(lambdaRewriter._synthesizedMethods);
				lambdaRewriter._synthesizedMethods.Free();
			}
		}
		analysis.Free();
		return result;
	}

	private BoundStatement AddStatementsIfNeeded(BoundStatement body)
	{
		if (_addedLocals != null)
		{
			_addedStatements.Add(body);
			body = new BoundBlock(body.Syntax, _addedLocals.ToImmutableAndFree(), _addedStatements.ToImmutableAndFree())
			{
				WasCompilerGenerated = true
			};
			_addedLocals = null;
			_addedStatements = null;
		}
		return body;
	}

	private void SynthesizeClosureEnvironments(ArrayBuilder<ClosureDebugInfo> closureDebugInfo)
	{
		Analysis.VisitScopeTree(_analysis.ScopeTree, (Analysis.Scope scope) =>
		{
			if (scope.DeclaredEnvironments.Count > 0)
			{
				Analysis.ClosureEnvironment closureEnvironment = scope.DeclaredEnvironments[0];
				SynthesizedClosureEnvironment synthesizedClosureEnvironment = (closureEnvironment.SynthesizedEnvironment = MakeFrame(scope, closureEnvironment));
				CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(ContainingType, synthesizedClosureEnvironment);
				if (synthesizedClosureEnvironment.Constructor != null)
				{
					AddSynthesizedMethod(synthesizedClosureEnvironment.Constructor, FlowAnalysisPass.AppendImplicitReturn(MethodCompiler.BindMethodBody(synthesizedClosureEnvironment.Constructor, CompilationState, null), synthesizedClosureEnvironment.Constructor));
				}
				_frames.Add(scope.BoundNode, closureEnvironment);
			}
		});
		SynthesizedClosureEnvironment MakeFrame(Analysis.Scope scope, Analysis.ClosureEnvironment env)
		{
			SyntaxNode syntax = scope.BoundNode.Syntax;
			DebugId topLevelMethodId = _analysis.GetTopLevelMethodId();
			DebugId closureId = _analysis.GetClosureId(syntax, closureDebugInfo);
			MethodSymbol methodSymbol = scope.ContainingClosureOpt?.OriginalMethodSymbol ?? _topLevelMethod;
			if ((object)_substitutedSourceMethod != null && methodSymbol == _topLevelMethod)
			{
				methodSymbol = _substitutedSourceMethod;
			}
			SynthesizedClosureEnvironment synthesizedClosureEnvironment = new SynthesizedClosureEnvironment(_topLevelMethod, methodSymbol, env.IsStruct, syntax, topLevelMethodId, closureId);
			foreach (Symbol capturedVariable in env.CapturedVariables)
			{
				LambdaCapturedVariable lambdaCapturedVariable = LambdaCapturedVariable.Create(synthesizedClosureEnvironment, capturedVariable, ref _synthesizedFieldNameIdDispenser);
				proxies.Add(capturedVariable, new CapturedToFrameSymbolReplacement(lambdaCapturedVariable, isReusable: false));
				synthesizedClosureEnvironment.AddHoistedField(lambdaCapturedVariable);
				CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(synthesizedClosureEnvironment, lambdaCapturedVariable);
			}
			return synthesizedClosureEnvironment;
		}
	}

	private void SynthesizeLoweredFunctionMethods()
	{
		Analysis.VisitClosures(_analysis.ScopeTree, (Analysis.Scope scope, Analysis.Closure closure) =>
		{
			MethodSymbol originalMethodSymbol = closure.OriginalMethodSymbol;
			SyntaxNode syntax = originalMethodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
			ClosureKind closureKind;
			NamedTypeSymbol containingType;
			int closureOrdinal;
			if (closure.ContainingEnvironmentOpt != null)
			{
				SynthesizedClosureEnvironment synthesizedClosureEnvironment = closure.ContainingEnvironmentOpt?.SynthesizedEnvironment;
				closureKind = ClosureKind.General;
				containingType = synthesizedClosureEnvironment;
				closureOrdinal = synthesizedClosureEnvironment.ClosureOrdinal;
			}
			else if (closure.CapturesThis)
			{
				containingType = _topLevelMethod.ContainingType;
				closureKind = ClosureKind.ThisOnly;
				closureOrdinal = -2;
			}
			else if (closure.CapturedEnvironments.Count == 0 && _analysis.MethodsConvertedToDelegates.Contains(originalMethodSymbol))
			{
				containingType = GetStaticFrame(Diagnostics, syntax);
				closureKind = ClosureKind.Singleton;
				closureOrdinal = -1;
			}
			else
			{
				containingType = _topLevelMethod.ContainingType;
				closureKind = ClosureKind.Static;
				closureOrdinal = -1;
			}
			DebugId topLevelMethodId = _analysis.GetTopLevelMethodId();
			DebugId lambdaId = GetLambdaId(syntax, closureKind, closureOrdinal);
			SynthesizedClosureMethod synthesizedLoweredMethod = new SynthesizedClosureMethod(containingType, GetStructClosures(closure), closureKind, _topLevelMethod, topLevelMethodId, originalMethodSymbol, closure.BlockSyntax, lambdaId);
			closure.SynthesizedLoweredMethod = synthesizedLoweredMethod;
		});
		static ImmutableArray<SynthesizedClosureEnvironment> GetStructClosures(Analysis.Closure closure)
		{
			ArrayBuilder<SynthesizedClosureEnvironment> instance = ArrayBuilder<SynthesizedClosureEnvironment>.GetInstance();
			foreach (Analysis.ClosureEnvironment capturedEnvironment in closure.CapturedEnvironments)
			{
				if (capturedEnvironment.IsStruct)
				{
					instance.Add(capturedEnvironment.SynthesizedEnvironment);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	private SynthesizedClosureEnvironment GetStaticFrame(DiagnosticBag diagnostics, SyntaxNode syntax)
	{
		if (_lazyStaticLambdaFrame == null)
		{
			bool flag = !_topLevelMethod.IsGenericMethod;
			if (flag)
			{
				_lazyStaticLambdaFrame = CompilationState.StaticLambdaFrame;
			}
			if (_lazyStaticLambdaFrame == null)
			{
				DebugId methodId = ((!flag) ? _analysis.GetTopLevelMethodId() : new DebugId(-1, CompilationState.ModuleBuilderOpt.CurrentGenerationOrdinal));
				DebugId closureId = default;
				MethodSymbol containingMethod = (flag ? null : (_substitutedSourceMethod ?? _topLevelMethod));
				_lazyStaticLambdaFrame = new SynthesizedClosureEnvironment(_topLevelMethod, containingMethod, isStruct: false, null, methodId, closureId);
				if (flag)
				{
					CompilationState.StaticLambdaFrame = _lazyStaticLambdaFrame;
				}
				SynthesizedClosureEnvironment lazyStaticLambdaFrame = _lazyStaticLambdaFrame;
				CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(ContainingType, lazyStaticLambdaFrame);
				AddSynthesizedMethod(lazyStaticLambdaFrame.Constructor, FlowAnalysisPass.AppendImplicitReturn(MethodCompiler.BindMethodBody(lazyStaticLambdaFrame.Constructor, CompilationState, null), lazyStaticLambdaFrame.Constructor));
				SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(lazyStaticLambdaFrame.StaticConstructor, syntax, CompilationState, diagnostics);
				BoundBlock body = syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Field(null, lazyStaticLambdaFrame.SingletonCache), syntheticBoundNodeFactory.New(lazyStaticLambdaFrame.Constructor)), new BoundReturnStatement(syntax, RefKind.None, null));
				AddSynthesizedMethod(lazyStaticLambdaFrame.StaticConstructor, body);
			}
		}
		return _lazyStaticLambdaFrame;
	}

	private BoundExpression FrameOfType(SyntaxNode syntax, NamedTypeSymbol frameType)
	{
		return FramePointer(syntax, frameType.OriginalDefinition);
	}

	protected override BoundExpression FramePointer(SyntaxNode syntax, NamedTypeSymbol frameClass)
	{
		if ((object)_currentFrameThis != null && _currentFrameThis.Type == frameClass)
		{
			return new BoundThisReference(syntax, frameClass);
		}
		SynthesizedClosureMethod synthesizedClosureMethod = _currentMethod as SynthesizedClosureMethod;
		if (synthesizedClosureMethod != null)
		{
			for (int i = synthesizedClosureMethod.ParameterCount - synthesizedClosureMethod.ExtraSynthesizedParameterCount; i < synthesizedClosureMethod.ParameterCount; i++)
			{
				ParameterSymbol parameterSymbol = synthesizedClosureMethod.Parameters[i];
				if (parameterSymbol.Type.OriginalDefinition == frameClass)
				{
					return new BoundParameter(syntax, parameterSymbol);
				}
			}
		}
		Symbol symbol = _framePointers[frameClass];
		if (proxies.TryGetValue(symbol, out var value))
		{
			return value.Replacement(syntax, (NamedTypeSymbol frameType) => FramePointer(syntax, frameType));
		}
		LocalSymbol localSymbol = (LocalSymbol)symbol;
		return new BoundLocal(syntax, localSymbol, null, localSymbol.Type);
	}

	private static void InsertAndFreePrologue(ArrayBuilder<BoundStatement> result, ArrayBuilder<BoundExpression> prologue)
	{
		foreach (BoundExpression item in prologue)
		{
			result.Add(new BoundExpressionStatement(item.Syntax, item));
		}
		prologue.Free();
	}

	private BoundNode IntroduceFrame(BoundNode node, Analysis.ClosureEnvironment env, Func<ArrayBuilder<BoundExpression>, ArrayBuilder<LocalSymbol>, BoundNode> F)
	{
		SynthesizedClosureEnvironment synthesizedEnvironment = env.SynthesizedEnvironment;
		ImmutableArray<TypeWithModifiers> typeArguments = ImmutableArray.Create(StaticCast<TypeSymbol>.From(_currentTypeParameters).SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers), 0, synthesizedEnvironment.Arity);
		NamedTypeSymbol namedTypeSymbol = synthesizedEnvironment.ConstructIfGeneric(typeArguments);
		LocalSymbol localSymbol = new SynthesizedLocal(_topLevelMethod, namedTypeSymbol, SynthesizedLocalKind.LambdaDisplayClass, synthesizedEnvironment.ScopeSyntaxOpt);
		SyntaxNode syntax = node.Syntax;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		if ((object)synthesizedEnvironment.Constructor != null)
		{
			MethodSymbol constructor = synthesizedEnvironment.Constructor.AsMember(namedTypeSymbol);
			instance.Add(new BoundAssignmentOperator(syntax, new BoundLocal(syntax, localSymbol, null, namedTypeSymbol), new BoundObjectCreationExpression(syntax, constructor, null), namedTypeSymbol));
		}
		CapturedSymbolReplacement value = null;
		if ((object)_innermostFramePointer != null)
		{
			proxies.TryGetValue(_innermostFramePointer, out value);
			if (env.CapturesParent)
			{
				LambdaCapturedVariable lambdaCapturedVariable = LambdaCapturedVariable.Create(synthesizedEnvironment, _innermostFramePointer, ref _synthesizedFieldNameIdDispenser);
				FieldSymbol fieldSymbol = lambdaCapturedVariable.AsMember(namedTypeSymbol);
				BoundExpression boundExpression = new BoundFieldAccess(syntax, new BoundLocal(syntax, localSymbol, null, namedTypeSymbol), fieldSymbol, null);
				BoundExpression right = FrameOfType(syntax, fieldSymbol.Type as NamedTypeSymbol);
				BoundExpression item = new BoundAssignmentOperator(syntax, boundExpression, right, boundExpression.Type);
				instance.Add(item);
				if (CompilationState.Emitting)
				{
					synthesizedEnvironment.AddHoistedField(lambdaCapturedVariable);
					CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(synthesizedEnvironment, lambdaCapturedVariable);
				}
				proxies[_innermostFramePointer] = new CapturedToFrameSymbolReplacement(lambdaCapturedVariable, isReusable: false);
			}
		}
		foreach (Symbol capturedVariable in env.CapturedVariables)
		{
			InitVariableProxy(syntax, capturedVariable, localSymbol, instance);
		}
		Symbol innermostFramePointer = _innermostFramePointer;
		if (!localSymbol.Type.IsValueType)
		{
			_innermostFramePointer = localSymbol;
		}
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		instance2.Add(localSymbol);
		_framePointers.Add(synthesizedEnvironment, localSymbol);
		BoundNode result = F(instance, instance2);
		_innermostFramePointer = innermostFramePointer;
		if ((object)_innermostFramePointer != null)
		{
			if (value != null)
			{
				proxies[_innermostFramePointer] = value;
				return result;
			}
			proxies.Remove(_innermostFramePointer);
		}
		return result;
	}

	private void InitVariableProxy(SyntaxNode syntax, Symbol symbol, LocalSymbol framePointer, ArrayBuilder<BoundExpression> prologue)
	{
		if (!proxies.TryGetValue(symbol, out var value))
		{
			return;
		}
		BoundExpression boundExpression;
		switch (symbol.Kind)
		{
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)symbol;
			if (!_parameterMap.TryGetValue(parameterSymbol, out var value3))
			{
				value3 = parameterSymbol;
			}
			boundExpression = new BoundParameter(syntax, value3);
			break;
		}
		case SymbolKind.Local:
		{
			LocalSymbol localSymbol = (LocalSymbol)symbol;
			if (_assignLocals == null || !_assignLocals.Contains(localSymbol))
			{
				return;
			}
			if (!localMap.TryGetValue(localSymbol, out var value2))
			{
				value2 = localSymbol;
			}
			boundExpression = new BoundLocal(syntax, value2, null, value2.Type);
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
		BoundExpression left = value.Replacement(syntax, (NamedTypeSymbol frameType1) => new BoundLocal(syntax, framePointer, null, framePointer.Type));
		BoundAssignmentOperator boundAssignmentOperator = new BoundAssignmentOperator(syntax, left, boundExpression, boundExpression.Type);
		if (_currentMethod.MethodKind == MethodKind.Constructor && symbol == _currentMethod.ThisParameter && !_seenBaseCall)
		{
			_thisProxyInitDeferred = boundAssignmentOperator;
		}
		else
		{
			prologue.Add(boundAssignmentOperator);
		}
	}

	protected override BoundNode VisitUnhoistedParameter(BoundParameter node)
	{
		if (_parameterMap.TryGetValue(node.ParameterSymbol, out var value))
		{
			return new BoundParameter(node.Syntax, value, value.Type, node.HasErrors);
		}
		return base.VisitUnhoistedParameter(node);
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		if (!(_currentMethod == _topLevelMethod) && !(_topLevelMethod.ThisParameter == null))
		{
			return FramePointer(node.Syntax, (NamedTypeSymbol)node.Type);
		}
		return node;
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		if (_currentMethod.IsStatic || !(_currentMethod.ContainingType == _topLevelMethod.ContainingType))
		{
			return FramePointer(node.Syntax, _topLevelMethod.ContainingType);
		}
		return node;
	}

	private void RemapLocalFunction(SyntaxNode syntax, MethodSymbol localFunc, out BoundExpression receiver, out MethodSymbol method, ref ImmutableArray<BoundExpression> arguments, ref ImmutableArray<RefKind> argRefKinds)
	{
		SynthesizedClosureMethod synthesizedLoweredMethod = Analysis.GetClosureInTree(_analysis.ScopeTree, localFunc.OriginalDefinition).SynthesizedLoweredMethod;
		int extraSynthesizedParameterCount = synthesizedLoweredMethod.ExtraSynthesizedParameterCount;
		if (extraSynthesizedParameterCount != 0)
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(synthesizedLoweredMethod.ParameterCount);
			instance.AddRange(arguments);
			for (int i = synthesizedLoweredMethod.ParameterCount - extraSynthesizedParameterCount; i < synthesizedLoweredMethod.ParameterCount; i++)
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)synthesizedLoweredMethod.Parameters[i].Type.OriginalDefinition;
				if (namedTypeSymbol.Arity > 0)
				{
					ImmutableArray<TypeParameterSymbol> constructedFromTypeParameters = ((SynthesizedClosureEnvironment)namedTypeSymbol).ConstructedFromTypeParameters;
					ImmutableArray<TypeParameterSymbol> immutableArray = TypeMap.SubstituteTypeParameters(constructedFromTypeParameters);
					namedTypeSymbol = namedTypeSymbol.Construct(immutableArray);
				}
				BoundExpression item = FrameOfType(syntax, namedTypeSymbol);
				instance.Add(item);
			}
			ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance(instance.Count);
			if (!argRefKinds.IsDefault)
			{
				instance2.AddRange(argRefKinds);
			}
			else
			{
				instance2.AddMany(RefKind.None, arguments.Length);
			}
			instance2.AddMany(RefKind.Ref, extraSynthesizedParameterCount);
			arguments = instance.ToImmutableAndFree();
			argRefKinds = instance2.ToImmutableAndFree();
		}
		method = synthesizedLoweredMethod;
		RemapLambdaOrLocalFunction(syntax, localFunc, SubstituteTypeArguments(localFunc.TypeArguments), synthesizedLoweredMethod.ClosureKind, ref method, out receiver, out var _);
	}

	private ImmutableArray<TypeSymbol> SubstituteTypeArguments(ImmutableArray<TypeSymbol> typeArguments)
	{
		if (typeArguments.IsEmpty)
		{
			return typeArguments;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		foreach (TypeSymbol item in typeArguments)
		{
			TypeSymbol typeSymbol = item;
			TypeSymbol typeSymbol2;
			do
			{
				typeSymbol2 = typeSymbol;
				typeSymbol = TypeMap.SubstituteType(item).Type;
			}
			while (typeSymbol2 != typeSymbol);
			instance.Add(typeSymbol);
		}
		return instance.ToImmutableAndFree();
	}

	private void RemapLambdaOrLocalFunction(SyntaxNode syntax, MethodSymbol originalMethod, ImmutableArray<TypeSymbol> typeArgumentsOpt, ClosureKind closureKind, ref MethodSymbol synthesizedMethod, out BoundExpression receiver, out NamedTypeSymbol constructedFrame)
	{
		NamedTypeSymbol containingType = synthesizedMethod.ContainingType;
		SynthesizedClosureEnvironment synthesizedClosureEnvironment = containingType as SynthesizedClosureEnvironment;
		int num = (synthesizedClosureEnvironment?.Arity ?? 0) + synthesizedMethod.Arity;
		ImmutableArray<TypeSymbol> immutableArray = ImmutableArray.Create(StaticCast<TypeSymbol>.From(_currentTypeParameters), 0, num - originalMethod.Arity);
		if (!typeArgumentsOpt.IsDefault)
		{
			immutableArray = immutableArray.Concat(typeArgumentsOpt);
		}
		if (synthesizedClosureEnvironment != null && synthesizedClosureEnvironment.Arity != 0)
		{
			ImmutableArray<TypeSymbol> typeArguments = ImmutableArray.Create(immutableArray, 0, synthesizedClosureEnvironment.Arity);
			immutableArray = ImmutableArray.Create(immutableArray, synthesizedClosureEnvironment.Arity, immutableArray.Length - synthesizedClosureEnvironment.Arity);
			constructedFrame = synthesizedClosureEnvironment.Construct(typeArguments);
		}
		else
		{
			constructedFrame = containingType;
		}
		switch (closureKind)
		{
		case ClosureKind.Singleton:
		{
			FieldSymbol fieldSymbol = synthesizedClosureEnvironment.SingletonCache.AsMember(constructedFrame);
			receiver = new BoundFieldAccess(syntax, null, fieldSymbol, null);
			break;
		}
		case ClosureKind.Static:
			receiver = null;
			break;
		default:
			receiver = FrameOfType(syntax, constructedFrame);
			break;
		}
		synthesizedMethod = synthesizedMethod.AsMember(constructedFrame);
		if (synthesizedMethod.IsGenericMethod)
		{
			synthesizedMethod = synthesizedMethod.Construct(StaticCast<TypeSymbol>.From(immutableArray));
		}
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		if (node.Method.MethodKind == MethodKind.LocalFunction)
		{
			ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
			ImmutableArray<RefKind> argRefKinds = node.ArgumentRefKindsOpt;
			TypeSymbol type = VisitType(node.Type);
			RemapLocalFunction(node.Syntax, node.Method, out var receiver, out var method, ref arguments, ref argRefKinds);
			return node.Update(receiver, method, arguments, node.ArgumentNamesOpt, argRefKinds, node.IsDelegateCall, node.Expanded, node.InvokedAsExtensionMethod, node.ArgsToParamsOpt, node.ResultKind, node.BinderOpt, type);
		}
		BoundNode boundNode = base.VisitCall(node);
		if (boundNode.Kind != BoundKind.Call)
		{
			return boundNode;
		}
		BoundCall boundCall = (BoundCall)boundNode;
		if (!_seenBaseCall && _currentMethod == _topLevelMethod && node.IsConstructorInitializer())
		{
			_seenBaseCall = true;
			if (_thisProxyInitDeferred != null)
			{
				return new BoundSequence(node.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundExpression)boundCall), _thisProxyInitDeferred, boundCall.Type);
			}
		}
		return boundCall;
	}

	private BoundSequence RewriteSequence(BoundSequence node, ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals)
	{
		RewriteLocals(node.Locals, newLocals);
		foreach (BoundExpression sideEffect in node.SideEffects)
		{
			BoundExpression boundExpression = (BoundExpression)Visit(sideEffect);
			if (boundExpression != null)
			{
				prologue.Add(boundExpression);
			}
		}
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(newLocals.ToImmutableAndFree(), prologue.ToImmutableAndFree(), value, type);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		if (_frames.TryGetValue(node, out var value))
		{
			return IntroduceFrame(node, value, (ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals) => RewriteBlock(node, prologue, newLocals));
		}
		return RewriteBlock(node, ArrayBuilder<BoundExpression>.GetInstance(), ArrayBuilder<LocalSymbol>.GetInstance());
	}

	private BoundBlock RewriteBlock(BoundBlock node, ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals)
	{
		RewriteLocals(node.Locals, newLocals);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (prologue.Count > 0)
		{
			instance.Add(new BoundSequencePoint(null, null)
			{
				WasCompilerGenerated = true
			});
		}
		InsertAndFreePrologue(instance, prologue);
		foreach (BoundStatement statement in node.Statements)
		{
			BoundStatement boundStatement = (BoundStatement)Visit(statement);
			if (boundStatement != null)
			{
				instance.Add(boundStatement);
			}
		}
		return node.Update(newLocals.ToImmutableAndFree(), node.LocalFunctions, instance.ToImmutableAndFree());
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		RewriteLocals(node.Locals, instance);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		if (instance.Count == 0)
		{
			instance.Free();
			return new BoundStatementList(node.Syntax, statements);
		}
		return node.Update(instance.ToImmutableAndFree(), statements);
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		if (_frames.TryGetValue(node, out var value))
		{
			return IntroduceFrame(node, value, (ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals) => RewriteCatch(node, prologue, newLocals));
		}
		return RewriteCatch(node, ArrayBuilder<BoundExpression>.GetInstance(), ArrayBuilder<LocalSymbol>.GetInstance());
	}

	private BoundNode RewriteCatch(BoundCatchBlock node, ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals)
	{
		RewriteLocals(node.Locals, newLocals);
		ImmutableArray<LocalSymbol> locals = newLocals.ToImmutableAndFree();
		BoundExpression boundExpression = null;
		BoundExpression boundExpression2 = (BoundExpression)Visit(node.ExceptionFilterOpt);
		if (node.ExceptionSourceOpt != null)
		{
			boundExpression = (BoundExpression)Visit(node.ExceptionSourceOpt);
			if (prologue.Count > 0)
			{
				boundExpression = new BoundSequence(boundExpression.Syntax, ImmutableArray.Create<LocalSymbol>(), prologue.ToImmutable(), boundExpression, boundExpression.Type);
			}
		}
		else if (prologue.Count > 0)
		{
			boundExpression2 = new BoundSequence(boundExpression2.Syntax, ImmutableArray.Create<LocalSymbol>(), prologue.ToImmutable(), boundExpression2, boundExpression2.Type);
		}
		prologue.Free();
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		BoundBlock body = (BoundBlock)Visit(node.Body);
		return node.Update(locals, boundExpression, exceptionTypeOpt, boundExpression2, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		if (_frames.TryGetValue(node, out var value))
		{
			return IntroduceFrame(node, value, (ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals) => RewriteSequence(node, prologue, newLocals));
		}
		return RewriteSequence(node, ArrayBuilder<BoundExpression>.GetInstance(), ArrayBuilder<LocalSymbol>.GetInstance());
	}

	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		if (_frames.TryGetValue(node, out var value))
		{
			return IntroduceFrame(node, value, (ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals) =>
			{
				ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
				InsertAndFreePrologue(instance, prologue);
				foreach (BoundStatement statement in node.Statements)
				{
					instance.Add((BoundStatement)Visit(statement));
				}
				return new BoundBlock(node.Syntax, newLocals.ToImmutableAndFree(), instance.ToImmutableAndFree(), node.HasErrors);
			});
		}
		return base.VisitStatementList(node);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		if (_frames.TryGetValue(node, out var value))
		{
			return IntroduceFrame(node, value, (ArrayBuilder<BoundExpression> prologue, ArrayBuilder<LocalSymbol> newLocals) =>
			{
				ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
				InsertAndFreePrologue(instance, prologue);
				instance.Add((BoundStatement)base.VisitSwitchStatement(node));
				return new BoundBlock(node.Syntax, newLocals.ToImmutableAndFree(), instance.ToImmutableAndFree(), node.HasErrors);
			});
		}
		return base.VisitSwitchStatement(node);
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument.Kind == BoundKind.Lambda)
		{
			return RewriteLambdaConversion((BoundLambda)node.Argument);
		}
		MethodSymbol methodOpt = node.MethodOpt;
		if ((object)methodOpt != null && methodOpt.MethodKind == MethodKind.LocalFunction)
		{
			ImmutableArray<BoundExpression> arguments = default;
			ImmutableArray<RefKind> argRefKinds = default;
			RemapLocalFunction(node.Syntax, node.MethodOpt, out var receiver, out var method, ref arguments, ref argRefKinds);
			return new BoundDelegateCreationExpression(node.Syntax, receiver, method, node.IsExtensionMethod, VisitType(node.Type));
		}
		return base.VisitDelegateCreationExpression(node);
	}

	public override BoundNode VisitConversion(BoundConversion conversion)
	{
		if (conversion.ConversionKind == ConversionKind.AnonymousFunction)
		{
			BoundExpression boundExpression = (BoundExpression)RewriteLambdaConversion((BoundLambda)conversion.Operand);
			if (_inExpressionLambda && conversion.ExplicitCastInCode)
			{
				boundExpression = new BoundConversion(conversion.Syntax, boundExpression, conversion.Conversion, isBaseConversion: false, @checked: false, explicitCastInCode: true, conversion.ConstantValueOpt, conversion.Type);
			}
			return boundExpression;
		}
		return base.VisitConversion(conversion);
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		RewriteLambdaOrLocalFunction(node, out var _, out var _, out var _, out var _, out var _, out var _);
		return new BoundNoOpStatement(node.Syntax, NoOpStatementFlavor.Default);
	}

	private DebugId GetLambdaId(SyntaxNode syntax, ClosureKind closureKind, int closureOrdinal)
	{
		AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax = syntax as AnonymousFunctionExpressionSyntax;
		LocalFunctionStatementSyntax localFunctionStatementSyntax = syntax as LocalFunctionStatementSyntax;
		SyntaxNode syntaxNode;
		bool isLambdaBody;
		if (anonymousFunctionExpressionSyntax != null)
		{
			syntaxNode = anonymousFunctionExpressionSyntax.Body;
			isLambdaBody = true;
		}
		else if (localFunctionStatementSyntax != null)
		{
			syntaxNode = (SyntaxNode)(((object)localFunctionStatementSyntax.Body) ?? ((object)localFunctionStatementSyntax.ExpressionBody?.Expression));
			isLambdaBody = true;
		}
		else if (LambdaUtilities.IsQueryPairLambda(syntax))
		{
			syntaxNode = syntax;
			isLambdaBody = false;
		}
		else
		{
			syntaxNode = syntax;
			isLambdaBody = true;
		}
		DebugId debugId = ((slotAllocatorOpt == null || !slotAllocatorOpt.TryGetPreviousLambda(syntaxNode, isLambdaBody, out var lambdaId)) ? new DebugId(_lambdaDebugInfoBuilder.Count, CompilationState.ModuleBuilderOpt.CurrentGenerationOrdinal) : lambdaId);
		int syntaxOffset = _topLevelMethod.CalculateLocalSyntaxOffset(syntaxNode.SpanStart, syntaxNode.SyntaxTree);
		_lambdaDebugInfoBuilder.Add(new LambdaDebugInfo(syntaxOffset, debugId, closureOrdinal));
		return debugId;
	}

	private SynthesizedClosureMethod RewriteLambdaOrLocalFunction(IBoundLambdaOrFunction node, out ClosureKind closureKind, out NamedTypeSymbol translatedLambdaContainer, out SynthesizedClosureEnvironment containerAsFrame, out BoundNode lambdaScope, out DebugId topLevelMethodId, out DebugId lambdaId)
	{
		Analysis.Closure closure = Analysis.GetClosureInTree(_analysis.ScopeTree, node.Symbol);
		SynthesizedClosureMethod synthesizedLoweredMethod = closure.SynthesizedLoweredMethod;
		closureKind = synthesizedLoweredMethod.ClosureKind;
		translatedLambdaContainer = synthesizedLoweredMethod.ContainingType;
		containerAsFrame = translatedLambdaContainer as SynthesizedClosureEnvironment;
		topLevelMethodId = _analysis.GetTopLevelMethodId();
		lambdaId = synthesizedLoweredMethod.LambdaId;
		if (closure.ContainingEnvironmentOpt != null)
		{
			BoundNode tmpScope = null;
			Analysis.VisitScopeTree(_analysis.ScopeTree, (Analysis.Scope scope) =>
			{
				if (scope.DeclaredEnvironments.Contains(closure.ContainingEnvironmentOpt))
				{
					tmpScope = scope.BoundNode;
				}
			});
			lambdaScope = tmpScope;
		}
		else
		{
			lambdaScope = null;
		}
		CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(translatedLambdaContainer, synthesizedLoweredMethod);
		foreach (ParameterSymbol parameter in node.Symbol.Parameters)
		{
			_parameterMap.Add(parameter, synthesizedLoweredMethod.Parameters[parameter.Ordinal]);
		}
		MethodSymbol currentMethod = _currentMethod;
		ParameterSymbol currentFrameThis = _currentFrameThis;
		ImmutableArray<TypeParameterSymbol> currentTypeParameters = _currentTypeParameters;
		Symbol innermostFramePointer = _innermostFramePointer;
		TypeMap currentLambdaBodyTypeMap = _currentLambdaBodyTypeMap;
		ArrayBuilder<BoundStatement> addedStatements = _addedStatements;
		ArrayBuilder<LocalSymbol> addedLocals = _addedLocals;
		_addedStatements = null;
		_addedLocals = null;
		_currentMethod = synthesizedLoweredMethod;
		if (closureKind == ClosureKind.Static || closureKind == ClosureKind.Singleton)
		{
			_innermostFramePointer = (_currentFrameThis = null);
		}
		else
		{
			_currentFrameThis = synthesizedLoweredMethod.ThisParameter;
			_framePointers.TryGetValue(translatedLambdaContainer, out _innermostFramePointer);
		}
		_currentTypeParameters = containerAsFrame?.TypeParameters.Concat(synthesizedLoweredMethod.TypeParameters) ?? synthesizedLoweredMethod.TypeParameters;
		_currentLambdaBodyTypeMap = synthesizedLoweredMethod.TypeMap;
		BoundStatement body = AddStatementsIfNeeded((BoundStatement)VisitBlock(node.Body));
		AddSynthesizedMethod(synthesizedLoweredMethod, body);
		_currentMethod = currentMethod;
		_currentFrameThis = currentFrameThis;
		_currentTypeParameters = currentTypeParameters;
		_innermostFramePointer = innermostFramePointer;
		_currentLambdaBodyTypeMap = currentLambdaBodyTypeMap;
		_addedLocals = addedLocals;
		_addedStatements = addedStatements;
		return synthesizedLoweredMethod;
	}

	private void AddSynthesizedMethod(MethodSymbol method, BoundStatement body)
	{
		if (_synthesizedMethods == null)
		{
			_synthesizedMethods = ArrayBuilder<TypeCompilationState.MethodWithBody>.GetInstance();
		}
		_synthesizedMethods.Add(new TypeCompilationState.MethodWithBody(method, body, CompilationState.CurrentImportChain));
	}

	private BoundNode RewriteLambdaConversion(BoundLambda node)
	{
		bool inExpressionLambda = _inExpressionLambda;
		_inExpressionLambda = _inExpressionLambda || node.Type.IsExpressionTree();
		if (_inExpressionLambda)
		{
			TypeSymbol type = VisitType(node.Type);
			BoundBlock body = (BoundBlock)Visit(node.Body);
			node = node.Update(node.Symbol, body, node.Diagnostics, node.Binder, type);
			BoundNode result = (inExpressionLambda ? node : ExpressionLambdaRewriter.RewriteLambda(node, CompilationState, TypeMap, RecursionDepth, Diagnostics));
			_inExpressionLambda = inExpressionLambda;
			return result;
		}
		MethodSymbol synthesizedMethod = RewriteLambdaOrLocalFunction(node, out var closureKind, out var translatedLambdaContainer, out var containerAsFrame, out var lambdaScope, out var topLevelMethodId, out var lambdaId);
		RemapLambdaOrLocalFunction(node.Syntax, node.Symbol, default, closureKind, ref synthesizedMethod, out var receiver, out var constructedFrame);
		TypeSymbol type2 = VisitType(node.Type);
		BoundExpression boundExpression = new BoundDelegateCreationExpression(node.Syntax, receiver, synthesizedMethod, isExtensionMethod: false, type2);
		bool flag = closureKind == ClosureKind.Singleton && _currentMethod.MethodKind != MethodKind.StaticConstructor && !synthesizedMethod.IsGenericMethod;
		bool flag2 = lambdaScope != null && lambdaScope != Analysis.GetScopeParent(_analysis.ScopeTree, node.Body).BoundNode && InLoopOrLambda(node.Syntax, lambdaScope.Syntax);
		if (flag | flag2)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(_currentMethod, node.Syntax, CompilationState, Diagnostics);
			try
			{
				BoundExpression left;
				if (flag || (flag2 && (object)containerAsFrame != null))
				{
					TypeSymbol type3 = containerAsFrame.TypeMap.SubstituteType(node.Type).Type;
					string name = GeneratedNames.MakeLambdaCacheFieldName((closureKind == ClosureKind.General) ? (-1) : topLevelMethodId.Ordinal, topLevelMethodId.Generation, lambdaId.Ordinal, lambdaId.Generation);
					SynthesizedLambdaCacheFieldSymbol synthesizedLambdaCacheFieldSymbol = new SynthesizedLambdaCacheFieldSymbol(translatedLambdaContainer, type3, name, _topLevelMethod, isReadOnly: false, closureKind == ClosureKind.Singleton);
					CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(translatedLambdaContainer, synthesizedLambdaCacheFieldSymbol);
					left = syntheticBoundNodeFactory.Field(receiver, synthesizedLambdaCacheFieldSymbol.AsMember(constructedFrame));
				}
				else
				{
					LocalSymbol localSymbol = syntheticBoundNodeFactory.SynthesizedLocal(type2, null, isPinned: false, RefKind.None, SynthesizedLocalKind.CachedAnonymousMethodDelegate);
					if (_addedLocals == null)
					{
						_addedLocals = ArrayBuilder<LocalSymbol>.GetInstance();
					}
					_addedLocals.Add(localSymbol);
					if (_addedStatements == null)
					{
						_addedStatements = ArrayBuilder<BoundStatement>.GetInstance();
					}
					left = syntheticBoundNodeFactory.Local(localSymbol);
					_addedStatements.Add(syntheticBoundNodeFactory.Assignment(left, syntheticBoundNodeFactory.Null(type2)));
				}
				boundExpression = syntheticBoundNodeFactory.Coalesce(left, syntheticBoundNodeFactory.AssignmentExpression(left, boundExpression));
			}
			catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
			{
				Diagnostics.Add(missingPredefinedMember.Diagnostic);
				return new BoundBadExpression(syntheticBoundNodeFactory.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
			}
		}
		return boundExpression;
	}

	private static bool InLoopOrLambda(SyntaxNode lambdaSyntax, SyntaxNode scopeSyntax)
	{
		SyntaxNode parent = lambdaSyntax.Parent;
		while (parent != null && parent != scopeSyntax)
		{
			SyntaxKind syntaxKind = parent.Kind();
			if (syntaxKind - 8642 <= SyntaxKind.List || syntaxKind - 8809 <= (SyntaxKind)3 || syntaxKind == SyntaxKind.ForEachVariableStatement)
			{
				return true;
			}
			parent = parent.Parent;
		}
		return false;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
