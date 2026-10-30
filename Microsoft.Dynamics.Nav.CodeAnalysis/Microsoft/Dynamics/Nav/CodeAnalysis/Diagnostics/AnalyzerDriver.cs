using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalyzerDriver : AnalyzerDriverBase
{
	[StructLayout(LayoutKind.Auto)]
	private struct CodeBlockAnalyzerActions
	{
		public DiagnosticAnalyzer Analyzer;

		public ImmutableArray<CodeBlockStartAnalyzerAction> CodeBlockStartActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions;

		public ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OpererationBlockEndActions;
	}

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private static readonly ObjectPool<DeclarationAnalysisData> s_declarationAnalysisDataPool = new ObjectPool<DeclarationAnalysisData>(() => new DeclarationAnalysisData());

	private readonly Func<SyntaxNode, SyntaxKind> _getKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> lazyCodeBlockActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> lazyCodeBlockEndActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockStartAnalyzerAction>> _lazyCodeBlockStartActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>> _lazyNodeActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> _lazyOperationActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> _lazyOperationBlockActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> _lazyOperationBlockEndActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockStartAnalyzerAction>> _lazyOperationBlockStartActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>> NodeActionsByAnalyzerAndKind
	{
		get
		{
			if (_lazyNodeActionsByKind == null)
			{
				ImmutableArray<SyntaxNodeAnalyzerAction> syntaxNodeActions = AnalyzerActions.GetSyntaxNodeActions();
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>> value;
				if (syntaxNodeActions.Any())
				{
					IEnumerable<IGrouping<DiagnosticAnalyzer, SyntaxNodeAnalyzerAction>> enumerable = from a in syntaxNodeActions
						group a by a.Analyzer;
					ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>>();
					foreach (IGrouping<DiagnosticAnalyzer, SyntaxNodeAnalyzerAction> item in enumerable)
					{
						builder.Add(value: (!item.Any()) ? ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>.Empty : AnalyzerExecutor.GetNodeActionsByKind(item), key: item.Key);
					}
					value = builder.ToImmutable();
				}
				else
				{
					value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>>>.Empty;
				}
				Interlocked.CompareExchange(ref _lazyNodeActionsByKind, value, null);
			}
			return _lazyNodeActionsByKind;
		}
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> OperationActionsByAnalyzerAndKind
	{
		get
		{
			if (_lazyOperationActionsByKind == null)
			{
				ImmutableArray<OperationAnalyzerAction> operationActions = AnalyzerActions.OperationActions;
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> value;
				if (operationActions.Any())
				{
					IEnumerable<IGrouping<DiagnosticAnalyzer, OperationAnalyzerAction>> enumerable = from a in operationActions
						group a by a.Analyzer;
					ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>();
					foreach (IGrouping<DiagnosticAnalyzer, OperationAnalyzerAction> item in enumerable)
					{
						builder.Add(value: (!item.Any()) ? ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>.Empty : AnalyzerExecutor.GetOperationActionsByKind(item), key: item.Key);
					}
					value = builder.ToImmutable();
				}
				else
				{
					value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>.Empty;
				}
				Interlocked.CompareExchange(ref _lazyOperationActionsByKind, value, null);
			}
			return _lazyOperationActionsByKind;
		}
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockStartAnalyzerAction>> CodeBlockStartActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyCodeBlockStartActionsByAnalyzer, AnalyzerActions.GetCodeBlockStartActions());

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> CodeBlockEndActionsByAnalyzer => GetBlockActionsByAnalyzer(ref lazyCodeBlockEndActionsByAnalyzer, AnalyzerActions.CodeBlockEndActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> CodeBlockActionsByAnalyzer => GetBlockActionsByAnalyzer(ref lazyCodeBlockActionsByAnalyzer, AnalyzerActions.CodeBlockActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockStartAnalyzerAction>> OperationBlockStartActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockStartActionsByAnalyzer, AnalyzerActions.OperationBlockStartActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> OperationBlockEndActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockEndActionsByAnalyzer, AnalyzerActions.OperationBlockEndActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> OperationBlockActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockActionsByAnalyzer, AnalyzerActions.OperationBlockActions);

	internal AnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, Func<SyntaxNode, SyntaxKind> getKind, AnalyzerManager analyzerManager, Func<SyntaxTrivia, bool> isComment)
		: base(analyzers, analyzerManager, isComment)
	{
		_getKind = getKind;
	}

	protected override bool TryExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		bool flag = ShouldExecuteSyntaxNodeActions(analysisScope);
		bool flag2 = ShouldExecuteCodeBlockActions(analysisScope, symbol);
		bool flag3 = ShouldExecuteOperationActions(analysisScope);
		bool flag4 = ShouldExecuteOperationBlockActions(analysisScope, symbol);
		bool result = true;
		if (flag | flag3 | flag2 | flag4)
		{
			ImmutableArray<SyntaxReference> declaringSyntaxReferences = symbolEvent.DeclaringSyntaxReferences;
			for (int i = 0; i < declaringSyntaxReferences.Length; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				SyntaxReference syntaxReference = declaringSyntaxReferences[i];
				if ((analysisScope.FilterTreeOpt == null || analysisScope.FilterTreeOpt == syntaxReference.SyntaxTree) && !TryExecuteDeclaringReferenceActions(syntaxReference, i, symbolEvent, analysisScope, analysisStateOpt, flag, flag3, flag2, flag4, cancellationToken))
				{
					result = false;
				}
			}
		}
		else if (analysisStateOpt != null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			analysisStateOpt.MarkDeclarationsComplete(symbol, analysisScope.Analyzers);
			ImmutableArray<SyntaxReference> declaringSyntaxReferences2 = symbolEvent.DeclaringSyntaxReferences;
			for (int j = 0; j < declaringSyntaxReferences2.Length; j++)
			{
				SyntaxReference declaration = declaringSyntaxReferences2[j];
				ClearCachedAnalysisDataIfAnalyzed(declaration, symbol, j, analysisStateOpt);
			}
		}
		return result;
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> GetBlockActionsByAnalyzer<ActionType>(ref ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> lazyCodeBlockActionsByAnalyzer, ImmutableArray<ActionType> codeBlockActions) where ActionType : AnalyzerAction
	{
		if (lazyCodeBlockActionsByAnalyzer == null)
		{
			ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> value;
			if (!codeBlockActions.IsEmpty)
			{
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<ActionType>>();
				foreach (IGrouping<DiagnosticAnalyzer, ActionType> item in from action in codeBlockActions
					group action by action.Analyzer)
				{
					builder.Add(item.Key, item.ToImmutableArrayOrEmpty());
				}
				value = builder.ToImmutable();
			}
			else
			{
				value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>>.Empty;
			}
			Interlocked.CompareExchange(ref lazyCodeBlockActionsByAnalyzer, value, null);
		}
		return lazyCodeBlockActionsByAnalyzer;
	}

	private bool ShouldExecuteSyntaxNodeActions(AnalysisScope analysisScope)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (NodeActionsByAnalyzerAndKind.ContainsKey(analyzer))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldExecuteOperationActions(AnalysisScope analysisScope)
	{
		return analysisScope.Analyzers.Any((DiagnosticAnalyzer analyzer) => OperationActionsByAnalyzerAndKind.ContainsKey(analyzer));
	}

	private static bool ShouldExecuteBlockActions<T0, T1>(ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<T0>> blockStartActions, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<T1>> blockActions, AnalysisScope analysisScope, ISymbol symbol)
	{
		if (AnalyzerExecutor.CanHaveExecutableCodeBlock(symbol))
		{
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (blockStartActions.ContainsKey(analyzer) || blockActions.ContainsKey(analyzer))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ShouldExecuteCodeBlockActions(AnalysisScope analysisScope, ISymbol symbol)
	{
		return ShouldExecuteBlockActions(CodeBlockStartActionsByAnalyzer, CodeBlockActionsByAnalyzer, analysisScope, symbol);
	}

	private bool ShouldExecuteOperationBlockActions(AnalysisScope analysisScope, ISymbol symbol)
	{
		return ShouldExecuteBlockActions(OperationBlockStartActionsByAnalyzer, OperationBlockActionsByAnalyzer, analysisScope, symbol);
	}

	private void ClearCachedAnalysisDataIfAnalyzed(SyntaxReference declaration, ISymbol symbol, int declarationIndex, AnalysisState analysisState)
	{
		DebugAssertHelper.Assert(analysisState != null);
		if (analysisState.IsDeclarationComplete(symbol, declarationIndex))
		{
			compilationData.ClearDeclarationAnalysisData(declaration);
		}
	}

	private DeclarationAnalysisData ComputeDeclarationAnalysisData(ISymbol symbol, SyntaxReference declaration, SemanticModel semanticModel, bool shouldExecuteSyntaxNodeActions, AnalysisScope analysisScope, Func<DeclarationAnalysisData> allocateData, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		DeclarationAnalysisData declarationAnalysisData = allocateData();
		List<DeclarationInfo> declarationsInNode = declarationAnalysisData.DeclarationsInNode;
		SyntaxNode syntax = declaration.GetSyntax(cancellationToken);
		SyntaxNode topmostNodeForDiagnosticAnalysis = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, syntax);
		ComputeDeclarationsInNode(semanticModel, syntax, topmostNodeForDiagnosticAnalysis, declarationsInNode, cancellationToken);
		bool flag = analysisScope.FilterSpanOpt.HasValue && !analysisScope.ContainsSpan(topmostNodeForDiagnosticAnalysis.FullSpan);
		IEnumerable<SyntaxNode> enumerable2;
		if (!shouldExecuteSyntaxNodeActions)
		{
			IEnumerable<SyntaxNode> enumerable = ImmutableArray<SyntaxNode>.Empty;
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = GetSyntaxNodesToAnalyze(topmostNodeForDiagnosticAnalysis, symbol, declarationsInNode, analysisScope, flag, semanticModel, AnalyzerExecutor);
		}
		IEnumerable<SyntaxNode> collection = enumerable2;
		declarationAnalysisData.DeclaringReferenceSyntax = syntax;
		declarationAnalysisData.TopmostNodeForAnalysis = topmostNodeForDiagnosticAnalysis;
		declarationAnalysisData.DescendantNodesToAnalyze.AddRange(collection);
		declarationAnalysisData.IsPartialAnalysis = flag;
		return declarationAnalysisData;
	}

	private static void ComputeDeclarationsInNode(SemanticModel semanticModel, SyntaxNode declaringReferenceSyntax, SyntaxNode topmostNodeForAnalysis, List<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		int? levelsToCompute = 2;
		bool getSymbol = topmostNodeForAnalysis != declaringReferenceSyntax;
		semanticModel.ComputeDeclarationsInNode(topmostNodeForAnalysis, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private bool TryExecuteDeclaringReferenceActions(SyntaxReference decl, int declarationIndex, SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool shouldExecuteSyntaxNodeActions, bool shouldExecuteOperationActions, bool shouldExecuteCodeBlockActions, bool shouldExecuteOperationBlockActions, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(shouldExecuteSyntaxNodeActions | shouldExecuteOperationActions | shouldExecuteCodeBlockActions | shouldExecuteOperationBlockActions);
		ISymbol symbol = symbolEvent.Symbol;
		SemanticModel semanticModel = ((analysisStateOpt != null) ? compilationData.GetOrCreateCachedSemanticModel(decl.SyntaxTree, symbolEvent.Compilation) : symbolEvent.SemanticModel(decl));
		bool flag = analysisScope.Analyzers.Length < Analyzers.Length && (!analysisScope.FilterSpanOpt.HasValue || analysisScope.FilterSpanOpt.Value.Length >= decl.SyntaxTree.GetRoot(cancellationToken).Span.Length);
		DeclarationAnalysisData orComputeDeclarationAnalysisData = compilationData.GetOrComputeDeclarationAnalysisData(decl, (Func<DeclarationAnalysisData> allocateData) => ComputeDeclarationAnalysisData(symbol, decl, semanticModel, shouldExecuteSyntaxNodeActions, analysisScope, allocateData, cancellationToken), flag);
		if (!analysisScope.ShouldAnalyze(orComputeDeclarationAnalysisData.TopmostNodeForAnalysis))
		{
			return true;
		}
		bool flag2 = true;
		if (shouldExecuteSyntaxNodeActions)
		{
			List<SyntaxNode> descendantNodesToAnalyze = orComputeDeclarationAnalysisData.DescendantNodesToAnalyze;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (NodeActionsByAnalyzerAndKind.TryGetValue(analyzer, out var value) && !value.IsEmpty && !AnalyzerExecutor.TryExecuteSyntaxNodeActions(descendantNodesToAnalyze, value, analyzer, semanticModel, _getKind, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis.FullSpan, declarationIndex, symbol, analysisScope, analysisStateOpt))
				{
					flag2 = false;
				}
			}
		}
		if (shouldExecuteCodeBlockActions | shouldExecuteOperationActions | shouldExecuteOperationBlockActions)
		{
			ImmutableArray<SyntaxNode> immutableArray = ImmutableArray<SyntaxNode>.Empty;
			IEnumerable<CodeBlockAnalyzerActions> enumerable = null;
			foreach (DeclarationInfo item in orComputeDeclarationAnalysisData.DeclarationsInNode)
			{
				if (item.DeclaredNode != orComputeDeclarationAnalysisData.TopmostNodeForAnalysis && item.DeclaredNode != orComputeDeclarationAnalysisData.DeclaringReferenceSyntax)
				{
					continue;
				}
				immutableArray = item.ExecutableCodeBlocks;
				if (!immutableArray.Any())
				{
					continue;
				}
				if (shouldExecuteCodeBlockActions | shouldExecuteOperationBlockActions)
				{
					enumerable = GetCodeBlockActions(analysisScope);
				}
				if (!(shouldExecuteOperationActions | shouldExecuteOperationBlockActions))
				{
					break;
				}
				ImmutableArray<IOperation> operationBlocksToAnalyze = GetOperationBlocksToAnalyze(immutableArray, semanticModel, cancellationToken);
				ImmutableArray<IOperation> immutableArray2 = ImmutableArray<IOperation>.Empty;
				try
				{
					immutableArray2 = GetOperationsToAnalyze(operationBlocksToAnalyze);
				}
				catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex) || FatalError.ReportWithoutCrashUnlessCanceled(ex))
				{
					Diagnostic arg = AnalyzerExecutor.CreateDriverExceptionDiagnostic(ex);
					DiagnosticAnalyzer arg2 = Analyzers[0];
					AnalyzerExecutor.OnAnalyzerException(ex, arg2, arg);
				}
				if (immutableArray2.IsEmpty)
				{
					break;
				}
				if (shouldExecuteOperationActions)
				{
					foreach (DiagnosticAnalyzer analyzer2 in analysisScope.Analyzers)
					{
						if (OperationActionsByAnalyzerAndKind.TryGetValue(analyzer2, out var value2) && !value2.IsEmpty && !AnalyzerExecutor.TryExecuteOperationActions(immutableArray2, value2, analyzer2, semanticModel, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis.FullSpan, declarationIndex, symbol, analysisScope, analysisStateOpt))
						{
							flag2 = false;
						}
					}
				}
				if (!shouldExecuteOperationBlockActions)
				{
					break;
				}
				foreach (CodeBlockAnalyzerActions item2 in enumerable)
				{
					ImmutableArray<OperationBlockStartAnalyzerAction> operationBlockStartActions = item2.OperationBlockStartActions;
					if (operationBlockStartActions.IsEmpty)
					{
						ImmutableArray<OperationBlockAnalyzerAction> operationBlockActions = item2.OperationBlockActions;
						if (operationBlockActions.IsEmpty)
						{
							operationBlockActions = item2.OpererationBlockEndActions;
							if (operationBlockActions.IsEmpty)
							{
								continue;
							}
						}
					}
					if (!AnalyzerExecutor.TryExecuteOperationBlockActions(item2.OperationBlockStartActions, item2.OperationBlockActions, item2.OpererationBlockEndActions, item2.Analyzer, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis, symbol, operationBlocksToAnalyze, immutableArray2, semanticModel, declarationIndex, analysisScope, analysisStateOpt))
					{
						flag2 = false;
					}
				}
				break;
			}
			if (immutableArray.Any() & shouldExecuteCodeBlockActions)
			{
				foreach (CodeBlockAnalyzerActions item3 in enumerable)
				{
					ImmutableArray<CodeBlockStartAnalyzerAction> codeBlockStartActions = item3.CodeBlockStartActions;
					if (codeBlockStartActions.IsEmpty)
					{
						ImmutableArray<CodeBlockAnalyzerAction> codeBlockActions = item3.CodeBlockActions;
						if (codeBlockActions.IsEmpty)
						{
							codeBlockActions = item3.CodeBlockEndActions;
							if (codeBlockActions.IsEmpty)
							{
								continue;
							}
						}
					}
					if (!AnalyzerExecutor.TryExecuteCodeBlockActions(item3.CodeBlockStartActions, item3.CodeBlockActions, item3.CodeBlockEndActions, item3.Analyzer, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis, symbol, immutableArray, semanticModel, _getKind, declarationIndex, analysisScope, analysisStateOpt))
					{
						flag2 = false;
					}
				}
			}
		}
		if (flag2 && analysisStateOpt != null && !orComputeDeclarationAnalysisData.IsPartialAnalysis)
		{
			foreach (DiagnosticAnalyzer analyzer3 in analysisScope.Analyzers)
			{
				analysisStateOpt.MarkDeclarationComplete(symbol, declarationIndex, analyzer3);
			}
			if (flag)
			{
				ClearCachedAnalysisDataIfAnalyzed(decl, symbol, declarationIndex, analysisStateOpt);
			}
		}
		return flag2;
	}

	private IEnumerable<CodeBlockAnalyzerActions> GetCodeBlockActions(AnalysisScope analysisScope)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (!CodeBlockStartActionsByAnalyzer.TryGetValue(analyzer, out var value))
			{
				value = ImmutableArray<CodeBlockStartAnalyzerAction>.Empty;
			}
			if (!CodeBlockActionsByAnalyzer.TryGetValue(analyzer, out var value2))
			{
				value2 = ImmutableArray<CodeBlockAnalyzerAction>.Empty;
			}
			if (!CodeBlockEndActionsByAnalyzer.TryGetValue(analyzer, out var value3))
			{
				value3 = ImmutableArray<CodeBlockAnalyzerAction>.Empty;
			}
			if (!OperationBlockStartActionsByAnalyzer.TryGetValue(analyzer, out var value4))
			{
				value4 = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;
			}
			if (!OperationBlockActionsByAnalyzer.TryGetValue(analyzer, out var value5))
			{
				value5 = ImmutableArray<OperationBlockAnalyzerAction>.Empty;
			}
			if (!OperationBlockEndActionsByAnalyzer.TryGetValue(analyzer, out var value6))
			{
				value6 = ImmutableArray<OperationBlockAnalyzerAction>.Empty;
			}
			if (!value.IsEmpty || !value2.IsEmpty || !value3.IsEmpty || !value4.IsEmpty || !value5.IsEmpty || !value6.IsEmpty)
			{
				yield return new CodeBlockAnalyzerActions
				{
					Analyzer = analyzer,
					CodeBlockStartActions = value,
					CodeBlockActions = value2,
					CodeBlockEndActions = value3,
					OperationBlockStartActions = value4,
					OperationBlockActions = value5,
					OpererationBlockEndActions = value6
				};
			}
		}
	}

	private static IEnumerable<SyntaxNode> GetSyntaxNodesToAnalyze(SyntaxNode declaredNode, ISymbol declaredSymbol, IEnumerable<DeclarationInfo> declarationsInNode, AnalysisScope analysisScope, bool isPartialDeclAnalysis, SemanticModel semanticModel, AnalyzerExecutor analyzerExecutor)
	{
		HashSet<SyntaxNode> hashSet = null;
		bool flag = true;
		foreach (DeclarationInfo item2 in declarationsInNode)
		{
			analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
			if (item2.DeclaredNode != declaredNode)
			{
				if (IsEquivalentSymbol(declaredSymbol, item2.DeclaredSymbol))
				{
					if (flag)
					{
						break;
					}
					return ImmutableArray<SyntaxNode>.Empty;
				}
				if (SyntaxNodeShouldNotBeSkipped(item2, declaredNode))
				{
					continue;
				}
				SyntaxNode item = item2.DeclaredNode;
				ISymbol symbol = item2.DeclaredSymbol ?? semanticModel.GetDeclaredSymbol(item2.DeclaredNode, analyzerExecutor.CancellationToken);
				if (symbol != null)
				{
					item = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, item2.DeclaredNode);
				}
				hashSet = hashSet ?? new HashSet<SyntaxNode>();
				hashSet.Add(item);
			}
			flag = false;
		}
		IEnumerable<SyntaxNode> enumerable = ((hashSet == null) ? declaredNode.DescendantNodesAndSelf() : GetSyntaxNodesToAnalyze(declaredNode, hashSet));
		if (isPartialDeclAnalysis)
		{
			enumerable = enumerable.Where((SyntaxNode node) => analysisScope.ShouldAnalyze(node));
		}
		return enumerable;
	}

	private static bool SyntaxNodeShouldNotBeSkipped(DeclarationInfo declInNode, SyntaxNode declaredNode)
	{
		SyntaxKind kind = declInNode.DeclaredNode.Kind;
		if (kind == SyntaxKind.VarSection)
		{
			if (!declaredNode.Kind.IsApplicationObjectSyntax() && declaredNode.Kind != SyntaxKind.RequestPage && declaredNode.Kind != SyntaxKind.TriggerDeclaration)
			{
				return declaredNode.Kind == SyntaxKind.MethodDeclaration;
			}
			return true;
		}
		return false;
	}

	private static bool IsEquivalentSymbol(ISymbol declaredSymbol, ISymbol otherSymbol)
	{
		if (declaredSymbol.Equals(otherSymbol))
		{
			return true;
		}
		return false;
	}

	private static ImmutableArray<IOperation> GetOperationBlocksToAnalyze(ImmutableArray<SyntaxNode> executableBlocks, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		foreach (SyntaxNode item in executableBlocks)
		{
			IOperation operation = semanticModel.GetOperation(item, cancellationToken);
			if (operation != null)
			{
				instance.AddRange(operation);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<IOperation> GetOperationsToAnalyze(ImmutableArray<IOperation> operationBlocks)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		foreach (IOperation item in operationBlocks)
		{
			instance.AddRange(item.DescendantsAndSelf());
		}
		return instance.ToImmutableAndFree();
	}

	private static IEnumerable<SyntaxNode> GetSyntaxNodesToAnalyze(SyntaxNode declaredNode, HashSet<SyntaxNode> descendantDeclsToSkip)
	{
		DebugAssertHelper.Assert(declaredNode != null);
		DebugAssertHelper.Assert(descendantDeclsToSkip != null);
		foreach (SyntaxNode item in declaredNode.DescendantNodesAndSelf((SyntaxNode n) => !descendantDeclsToSkip.Contains(n), descendIntoTrivia: true))
		{
			if (!descendantDeclsToSkip.Contains(item))
			{
				yield return item;
			}
		}
	}
}
