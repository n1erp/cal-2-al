using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalysisState
{
	internal class AnalyzerStateData
	{
		public static readonly AnalyzerStateData FullyProcessedInstance = CreateFullyProcessedInstance();

		public StateKind StateKind { get; private set; }

		public HashSet<AnalyzerAction> ProcessedActions { get; }

		public AnalyzerStateData()
		{
			StateKind = StateKind.InProcess;
			ProcessedActions = new HashSet<AnalyzerAction>();
		}

		public virtual void SetStateKind(StateKind stateKind)
		{
			StateKind = stateKind;
		}

		public void ResetToReadyState()
		{
			SetStateKind(StateKind.ReadyToProcess);
		}

		public virtual void Free()
		{
			StateKind = StateKind.ReadyToProcess;
			ProcessedActions.Clear();
		}

		private static AnalyzerStateData CreateFullyProcessedInstance()
		{
			AnalyzerStateData analyzerStateData = new AnalyzerStateData();
			analyzerStateData.SetStateKind(StateKind.FullyProcessed);
			return analyzerStateData;
		}
	}

	private class PerAnalyzerState
	{
		private readonly ObjectPool<AnalyzerStateData> analyzerStateDataPool;

		private readonly ObjectPool<DeclarationAnalyzerStateData> declarationAnalyzerStateDataPool;

		private readonly ObjectPool<Dictionary<int, DeclarationAnalyzerStateData>> currentlyAnalyzingDeclarationsMapPool;

		private readonly object gate = new object();

		private readonly Dictionary<ISymbol, Dictionary<int, DeclarationAnalyzerStateData>> pendingDeclarations = new Dictionary<ISymbol, Dictionary<int, DeclarationAnalyzerStateData>>();

		private readonly Dictionary<CompilationEvent, AnalyzerStateData> pendingEvents = new Dictionary<CompilationEvent, AnalyzerStateData>();

		private readonly Dictionary<ISymbol, AnalyzerStateData> pendingSymbols = new Dictionary<ISymbol, AnalyzerStateData>();

		private Dictionary<SyntaxTree, AnalyzerStateData> lazySyntaxTreesWithAnalysisData;

		private int pendingSyntaxAnalysisTreesCount;

		public PerAnalyzerState(ObjectPool<AnalyzerStateData> analyzerStateDataPool, ObjectPool<DeclarationAnalyzerStateData> declarationAnalyzerStateDataPool, ObjectPool<Dictionary<int, DeclarationAnalyzerStateData>> currentlyAnalyzingDeclarationsMapPool)
		{
			this.analyzerStateDataPool = analyzerStateDataPool;
			this.declarationAnalyzerStateDataPool = declarationAnalyzerStateDataPool;
			this.currentlyAnalyzingDeclarationsMapPool = currentlyAnalyzingDeclarationsMapPool;
		}

		public void AddPendingEvents(HashSet<CompilationEvent> uniqueEvents)
		{
			lock (gate)
			{
				foreach (CompilationEvent key in pendingEvents.Keys)
				{
					uniqueEvents.Add(key);
				}
			}
		}

		public bool HasPendingSyntaxAnalysis(SyntaxTree treeOpt)
		{
			lock (gate)
			{
				if (pendingSyntaxAnalysisTreesCount == 0)
				{
					return false;
				}
				DebugAssertHelper.Assert(lazySyntaxTreesWithAnalysisData != null);
				if (treeOpt == null)
				{
					return true;
				}
				if (!lazySyntaxTreesWithAnalysisData.TryGetValue(treeOpt, out var value))
				{
					return true;
				}
				return value.StateKind == StateKind.FullyProcessed;
			}
		}

		public bool HasPendingSymbolAnalysis(ISymbol symbol)
		{
			lock (gate)
			{
				return pendingSymbols.ContainsKey(symbol);
			}
		}

		public bool TryStartProcessingEvent(CompilationEvent compilationEvent, out AnalyzerStateData state)
		{
			return TryStartProcessingEntity(compilationEvent, pendingEvents, analyzerStateDataPool, out state);
		}

		public void MarkEventComplete(CompilationEvent compilationEvent)
		{
			MarkEntityProcessed(compilationEvent, pendingEvents, analyzerStateDataPool);
		}

		public bool TryStartAnalyzingSymbol(ISymbol symbol, out AnalyzerStateData state)
		{
			return TryStartProcessingEntity(symbol, pendingSymbols, analyzerStateDataPool, out state);
		}

		public void MarkSymbolComplete(ISymbol symbol)
		{
			MarkEntityProcessed(symbol, pendingSymbols, analyzerStateDataPool);
		}

		public bool TryStartAnalyzingDeclaration(ISymbol symbol, int declarationIndex, out DeclarationAnalyzerStateData state)
		{
			lock (gate)
			{
				return TryStartAnalyzingDeclaration_NoLock(symbol, declarationIndex, out state);
			}
		}

		public bool IsDeclarationComplete(ISymbol symbol, int declarationIndex)
		{
			lock (gate)
			{
				return IsDeclarationComplete_NoLock(symbol, declarationIndex);
			}
		}

		public void MarkDeclarationComplete(ISymbol symbol, int declarationIndex)
		{
			lock (gate)
			{
				MarkDeclarationProcessed_NoLock(symbol, declarationIndex);
			}
		}

		public void MarkDeclarationsComplete(ISymbol symbol)
		{
			lock (gate)
			{
				MarkDeclarationsProcessed_NoLock(symbol);
			}
		}

		public bool TryStartSyntaxAnalysis(SyntaxTree tree, out AnalyzerStateData state)
		{
			lock (gate)
			{
				DebugAssertHelper.Assert(lazySyntaxTreesWithAnalysisData != null);
				return TryStartSyntaxAnalysis_NoLock(tree, out state);
			}
		}

		public void MarkSyntaxAnalysisComplete(SyntaxTree tree)
		{
			lock (gate)
			{
				MarkSyntaxTreeProcessed_NoLock(tree);
			}
		}

		public void OnCompilationEventGenerated(CompilationEvent compilationEvent, AnalyzerActionCounts actionCounts)
		{
			lock (gate)
			{
				if (compilationEvent is SymbolDeclaredCompilationEvent symbolDeclaredCompilationEvent)
				{
					bool flag = false;
					ISymbol symbol = symbolDeclaredCompilationEvent.Symbol;
					if (!AnalysisScope.ShouldSkipSymbolAnalysis(symbolDeclaredCompilationEvent) && actionCounts.SymbolActionsCount > 0)
					{
						flag = true;
						pendingSymbols[symbol] = null;
					}
					if (!AnalysisScope.ShouldSkipDeclarationAnalysis(symbol) && actionCounts.HasAnyExecutableCodeActions)
					{
						flag = true;
						pendingDeclarations[symbol] = currentlyAnalyzingDeclarationsMapPool.Allocate();
					}
					if (!flag)
					{
						return;
					}
				}
				else if (compilationEvent is CompilationStartedEvent)
				{
					if (actionCounts.SyntaxTreeActionsCount > 0)
					{
						lazySyntaxTreesWithAnalysisData = new Dictionary<SyntaxTree, AnalyzerStateData>();
						pendingSyntaxAnalysisTreesCount = compilationEvent.Compilation.SyntaxTrees.Count();
					}
					if (actionCounts.CompilationActionsCount == 0)
					{
						return;
					}
				}
				pendingEvents[compilationEvent] = null;
			}
		}

		public bool IsEventAnalyzed(CompilationEvent compilationEvent)
		{
			return IsEntityFullyProcessed(compilationEvent, pendingEvents);
		}

		public void OnSymbolDeclaredEventProcessed(SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			lock (gate)
			{
				OnSymbolDeclaredEventProcessed_NoLock(symbolDeclaredEvent);
			}
		}

		private bool TryStartProcessingEntity<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities, ObjectPool<TAnalyzerStateData> pool, out TAnalyzerStateData newState) where TAnalyzerStateData : AnalyzerStateData, new()
		{
			lock (gate)
			{
				return TryStartProcessingEntity_NoLock(analysisEntity, pendingEntities, pool, out newState);
			}
		}

		private static bool TryStartProcessingEntity_NoLock<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities, ObjectPool<TAnalyzerStateData> pool, out TAnalyzerStateData state) where TAnalyzerStateData : AnalyzerStateData
		{
			if (pendingEntities.TryGetValue(analysisEntity, out state) && (state == null || state.StateKind == StateKind.ReadyToProcess))
			{
				if (state == null)
				{
					state = pool.Allocate();
				}
				state.SetStateKind(StateKind.InProcess);
				DebugAssertHelper.Assert(state.StateKind == StateKind.InProcess);
				pendingEntities[analysisEntity] = state;
				return true;
			}
			state = null;
			return false;
		}

		private void MarkEntityProcessed<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities, ObjectPool<TAnalyzerStateData> pool) where TAnalyzerStateData : AnalyzerStateData
		{
			lock (gate)
			{
				MarkEntityProcessed_NoLock(analysisEntity, pendingEntities, pool);
			}
		}

		private static void MarkEntityProcessed_NoLock<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities, ObjectPool<TAnalyzerStateData> pool) where TAnalyzerStateData : AnalyzerStateData
		{
			if (pendingEntities.TryGetValue(analysisEntity, out var value))
			{
				pendingEntities.Remove(analysisEntity);
				FreeState_NoLock(value, pool);
			}
		}

		private bool TryStartSyntaxAnalysis_NoLock(SyntaxTree tree, out AnalyzerStateData state)
		{
			if (pendingSyntaxAnalysisTreesCount == 0)
			{
				state = null;
				return false;
			}
			if (lazySyntaxTreesWithAnalysisData.TryGetValue(tree, out state))
			{
				if (state.StateKind != StateKind.ReadyToProcess)
				{
					state = null;
					return false;
				}
			}
			else
			{
				state = analyzerStateDataPool.Allocate();
			}
			state.SetStateKind(StateKind.InProcess);
			DebugAssertHelper.Assert(state.StateKind == StateKind.InProcess);
			lazySyntaxTreesWithAnalysisData[tree] = state;
			return true;
		}

		private void MarkSyntaxTreeProcessed_NoLock(SyntaxTree tree)
		{
			if (pendingSyntaxAnalysisTreesCount == 0)
			{
				return;
			}
			DebugAssertHelper.Assert(lazySyntaxTreesWithAnalysisData != null);
			bool flag = false;
			if (lazySyntaxTreesWithAnalysisData.TryGetValue(tree, out var value))
			{
				if (value.StateKind != StateKind.FullyProcessed)
				{
					FreeState_NoLock(value, analyzerStateDataPool);
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				pendingSyntaxAnalysisTreesCount--;
			}
			lazySyntaxTreesWithAnalysisData[tree] = AnalyzerStateData.FullyProcessedInstance;
		}

		private bool TryStartAnalyzingDeclaration_NoLock(ISymbol symbol, int declarationIndex, out DeclarationAnalyzerStateData state)
		{
			if (!pendingDeclarations.TryGetValue(symbol, out var value))
			{
				state = null;
				return false;
			}
			if (value.TryGetValue(declarationIndex, out state))
			{
				if (state.StateKind != StateKind.ReadyToProcess)
				{
					state = null;
					return false;
				}
			}
			else
			{
				state = declarationAnalyzerStateDataPool.Allocate();
			}
			state.SetStateKind(StateKind.InProcess);
			DebugAssertHelper.Assert(state.StateKind == StateKind.InProcess);
			value[declarationIndex] = state;
			return true;
		}

		private void MarkDeclarationProcessed_NoLock(ISymbol symbol, int declarationIndex)
		{
			if (pendingDeclarations.TryGetValue(symbol, out var value))
			{
				if (value.TryGetValue(declarationIndex, out var value2))
				{
					FreeDeclarationAnalyzerState_NoLock(value2);
				}
				value[declarationIndex] = DeclarationAnalyzerStateData.FullyProcessedInstance;
			}
		}

		private void MarkDeclarationsProcessed_NoLock(ISymbol symbol)
		{
			if (pendingDeclarations.TryGetValue(symbol, out var value))
			{
				FreeDeclarationDataMap_NoLock(value);
				pendingDeclarations.Remove(symbol);
			}
		}

		private void FreeDeclarationDataMap_NoLock(Dictionary<int, DeclarationAnalyzerStateData> declarationDataMap)
		{
			declarationDataMap.Clear();
			currentlyAnalyzingDeclarationsMapPool.Free(declarationDataMap);
		}

		private void FreeDeclarationAnalyzerState_NoLock(DeclarationAnalyzerStateData state)
		{
			if (state != DeclarationAnalyzerStateData.FullyProcessedInstance)
			{
				FreeState_NoLock(state, declarationAnalyzerStateDataPool);
			}
		}

		private static void FreeState_NoLock<TAnalyzerStateData>(TAnalyzerStateData state, ObjectPool<TAnalyzerStateData> pool) where TAnalyzerStateData : AnalyzerStateData
		{
			if (state != null && state != AnalyzerStateData.FullyProcessedInstance)
			{
				state.Free();
				pool.Free(state);
			}
		}

		private bool IsEntityFullyProcessed<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities) where TAnalyzerStateData : AnalyzerStateData
		{
			lock (gate)
			{
				return IsEntityFullyProcessed_NoLock(analysisEntity, pendingEntities);
			}
		}

		private static bool IsEntityFullyProcessed_NoLock<TAnalysisEntity, TAnalyzerStateData>(TAnalysisEntity analysisEntity, Dictionary<TAnalysisEntity, TAnalyzerStateData> pendingEntities) where TAnalyzerStateData : AnalyzerStateData
		{
			if (pendingEntities.TryGetValue(analysisEntity, out var value))
			{
				TAnalyzerStateData val = value;
				if (val == null)
				{
					return false;
				}
				return val.StateKind == StateKind.FullyProcessed;
			}
			return true;
		}

		private bool IsDeclarationComplete_NoLock(ISymbol symbol, int declarationIndex)
		{
			if (!pendingDeclarations.TryGetValue(symbol, out var value))
			{
				return true;
			}
			if (!value.TryGetValue(declarationIndex, out var value2))
			{
				return false;
			}
			return value2.StateKind == StateKind.FullyProcessed;
		}

		private bool AreDeclarationsProcessed_NoLock(ISymbol symbol, int declarationsCount)
		{
			if (!pendingDeclarations.TryGetValue(symbol, out var value))
			{
				return true;
			}
			if (value.Count == declarationsCount)
			{
				return value.Values.All((DeclarationAnalyzerStateData state) => state.StateKind == StateKind.FullyProcessed);
			}
			return false;
		}

		private void OnSymbolDeclaredEventProcessed_NoLock(SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			if (IsEntityFullyProcessed_NoLock(symbolDeclaredEvent.Symbol, pendingSymbols) && AreDeclarationsProcessed_NoLock(symbolDeclaredEvent.Symbol, symbolDeclaredEvent.DeclaringSyntaxReferences.Length))
			{
				MarkEntityProcessed_NoLock(symbolDeclaredEvent, pendingEvents, analyzerStateDataPool);
				MarkDeclarationsProcessed_NoLock(symbolDeclaredEvent.Symbol);
			}
		}
	}

	internal sealed class DeclarationAnalyzerStateData : SyntaxNodeAnalyzerStateData
	{
		public new static readonly DeclarationAnalyzerStateData FullyProcessedInstance = CreateFullyProcessedInstance();

		public CodeBlockAnalyzerStateData CodeBlockAnalysisState { get; }

		public OperationBlockAnalyzerStateData OperationBlockAnalysisState { get; }

		public DeclarationAnalyzerStateData()
		{
			CodeBlockAnalysisState = new CodeBlockAnalyzerStateData();
			OperationBlockAnalysisState = new OperationBlockAnalyzerStateData();
		}

		public override void SetStateKind(StateKind stateKind)
		{
			CodeBlockAnalysisState.SetStateKind(stateKind);
			OperationBlockAnalysisState.SetStateKind(stateKind);
			base.SetStateKind(stateKind);
		}

		public override void Free()
		{
			base.Free();
			CodeBlockAnalysisState.Free();
			OperationBlockAnalysisState.Free();
		}

		private static DeclarationAnalyzerStateData CreateFullyProcessedInstance()
		{
			DeclarationAnalyzerStateData declarationAnalyzerStateData = new DeclarationAnalyzerStateData();
			declarationAnalyzerStateData.SetStateKind(StateKind.FullyProcessed);
			return declarationAnalyzerStateData;
		}
	}

	internal class SyntaxNodeAnalyzerStateData : AnalyzerStateData
	{
		public HashSet<SyntaxNode> ProcessedNodes { get; }

		public SyntaxNode CurrentNode { get; set; }

		public SyntaxNodeAnalyzerStateData()
		{
			CurrentNode = null;
			ProcessedNodes = new HashSet<SyntaxNode>();
		}

		public void ClearNodeAnalysisState()
		{
			CurrentNode = null;
			ProcessedActions.Clear();
		}

		public override void Free()
		{
			base.Free();
			CurrentNode = null;
			ProcessedNodes.Clear();
		}
	}

	internal class OperationAnalyzerStateData : AnalyzerStateData
	{
		public HashSet<IOperation> ProcessedOperations { get; }

		public IOperation CurrentOperation { get; set; }

		public OperationAnalyzerStateData()
		{
			CurrentOperation = null;
			ProcessedOperations = new HashSet<IOperation>();
		}

		public void ClearNodeAnalysisState()
		{
			CurrentOperation = null;
			ProcessedActions.Clear();
		}

		public override void Free()
		{
			base.Free();
			CurrentOperation = null;
			ProcessedOperations.Clear();
		}
	}

	internal abstract class BlockAnalyzerStateData<TBlockAction, TNodeStateData> : AnalyzerStateData where TBlockAction : AnalyzerAction where TNodeStateData : AnalyzerStateData, new()
	{
		public TNodeStateData ExecutableNodesAnalysisState { get; }

		public ImmutableHashSet<TBlockAction> CurrentBlockEndActions { get; set; }

		public ImmutableHashSet<AnalyzerAction> CurrentBlockNodeActions { get; set; }

		public BlockAnalyzerStateData()
		{
			ExecutableNodesAnalysisState = new TNodeStateData();
			CurrentBlockEndActions = null;
			CurrentBlockNodeActions = null;
		}

		public override void SetStateKind(StateKind stateKind)
		{
			ExecutableNodesAnalysisState.SetStateKind(stateKind);
			base.SetStateKind(stateKind);
		}

		public override void Free()
		{
			base.Free();
			ExecutableNodesAnalysisState.Free();
			CurrentBlockEndActions = null;
			CurrentBlockNodeActions = null;
		}
	}

	internal sealed class CodeBlockAnalyzerStateData : BlockAnalyzerStateData<CodeBlockAnalyzerAction, SyntaxNodeAnalyzerStateData>
	{
	}

	internal sealed class OperationBlockAnalyzerStateData : BlockAnalyzerStateData<OperationBlockAnalyzerAction, OperationAnalyzerStateData>
	{
	}

	private readonly object gate;

	private readonly ImmutableDictionary<DiagnosticAnalyzer, int> analyzerStateMap;

	private readonly ImmutableArray<PerAnalyzerState> analyzerStates;

	private readonly Dictionary<SyntaxTree, HashSet<CompilationEvent>> pendingSourceEvents;

	private readonly HashSet<CompilationEvent> pendingNonSourceEvents;

	private ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts> lazyAnalyzerActionCountsMap;

	private readonly HashSet<SyntaxTree> treesWithGeneratedSourceEvents;

	private readonly HashSet<ISymbol> partialSymbolsWithGeneratedSourceEvents;

	private readonly AnalyzerDriverBase.CompilationData compilationData;

	private bool compilationStartGenerated;

	private bool compilationEndGenerated;

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private readonly ConditionalWeakTable<SyntaxTree, SemanticModel> semanticModelsMap;

	private readonly ObjectPool<HashSet<CompilationEvent>> compilationEventsPool;

	private readonly HashSet<CompilationEvent> pooledEventsWithAnyActionsSet;

	public AnalysisState(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerDriverBase.CompilationData compilationData)
	{
		gate = new object();
		analyzerStateMap = CreateAnalyzerStateMap(analyzers, out analyzerStates);
		this.compilationData = compilationData;
		pendingSourceEvents = new Dictionary<SyntaxTree, HashSet<CompilationEvent>>();
		pendingNonSourceEvents = new HashSet<CompilationEvent>();
		lazyAnalyzerActionCountsMap = null;
		semanticModelsMap = new ConditionalWeakTable<SyntaxTree, SemanticModel>();
		treesWithGeneratedSourceEvents = new HashSet<SyntaxTree>();
		partialSymbolsWithGeneratedSourceEvents = new HashSet<ISymbol>();
		compilationStartGenerated = false;
		compilationEndGenerated = false;
		compilationEventsPool = new ObjectPool<HashSet<CompilationEvent>>(() => new HashSet<CompilationEvent>());
		pooledEventsWithAnyActionsSet = new HashSet<CompilationEvent>();
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, int> CreateAnalyzerStateMap(ImmutableArray<DiagnosticAnalyzer> analyzers, out ImmutableArray<PerAnalyzerState> analyzerStates)
	{
		ObjectPool<AnalyzerStateData> analyzerStateDataPool = new ObjectPool<AnalyzerStateData>(() => new AnalyzerStateData());
		ObjectPool<DeclarationAnalyzerStateData> declarationAnalyzerStateDataPool = new ObjectPool<DeclarationAnalyzerStateData>(() => new DeclarationAnalyzerStateData());
		ObjectPool<Dictionary<int, DeclarationAnalyzerStateData>> currentlyAnalyzingDeclarationsMapPool = new ObjectPool<Dictionary<int, DeclarationAnalyzerStateData>>(() => new Dictionary<int, DeclarationAnalyzerStateData>());
		ImmutableArray<PerAnalyzerState>.Builder builder = ImmutableArray.CreateBuilder<PerAnalyzerState>();
		ImmutableDictionary<DiagnosticAnalyzer, int>.Builder builder2 = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, int>();
		int num = 0;
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			builder.Add(new PerAnalyzerState(analyzerStateDataPool, declarationAnalyzerStateDataPool, currentlyAnalyzingDeclarationsMapPool));
			builder2[item] = num;
			num++;
		}
		analyzerStates = builder.ToImmutable();
		return builder2.ToImmutable();
	}

	private PerAnalyzerState GetAnalyzerState(DiagnosticAnalyzer analyzer)
	{
		int index = analyzerStateMap[analyzer];
		return analyzerStates[index];
	}

	public async Task GenerateSimulatedCompilationEventsAsync(AnalysisScope analysisScope, Compilation compilation, Func<SyntaxTree, Compilation, CancellationToken, SemanticModel> getCachedSemanticModel, AnalyzerDriver driver, CancellationToken cancellationToken)
	{
		await EnsureAnalyzerActionCountsInitializedAsync(driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		GenerateSimulatedCompilatioNonSourceEvent(compilation, started: true);
		foreach (SyntaxTree syntaxTree in analysisScope.SyntaxTrees)
		{
			GenerateSimulatedCompilationSourceEvents(syntaxTree, compilation, getCachedSemanticModel, cancellationToken);
		}
		if (analysisScope.FilterTreeOpt == null)
		{
			GenerateSimulatedCompilatioNonSourceEvent(compilation, started: false);
		}
	}

	private void GenerateSimulatedCompilationSourceEvents(SyntaxTree tree, Compilation compilation, Func<SyntaxTree, Compilation, CancellationToken, SemanticModel> getCachedSemanticModel, CancellationToken cancellationToken)
	{
		lock (gate)
		{
			if (treesWithGeneratedSourceEvents.Contains(tree))
			{
				return;
			}
		}
		ImmutableArray<CompilationEvent> compilationEvents = CreateCompilationEventsForTree(GetDeclaredSymbolsInTree(tree, compilation, getCachedSemanticModel, cancellationToken), tree, compilation);
		lock (gate)
		{
			if (!treesWithGeneratedSourceEvents.Contains(tree))
			{
				OnCompilationEventsGenerated_NoLock(compilationEvents, tree);
				DebugAssertHelper.Assert(treesWithGeneratedSourceEvents.Add(tree));
			}
		}
	}

	private static IEnumerable<ISymbol> GetDeclaredSymbolsInTree(SyntaxTree tree, Compilation compilation, Func<SyntaxTree, Compilation, CancellationToken, SemanticModel> getCachedSemanticModel, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = getCachedSemanticModel(tree, compilation, cancellationToken);
		TextSpan fullSpan = tree.GetRoot(cancellationToken).FullSpan;
		List<DeclarationInfo> list = new List<DeclarationInfo>();
		semanticModel.ComputeDeclarationsInSpan(fullSpan, getSymbol: true, list, cancellationToken);
		return list.Select((DeclarationInfo declInfo) => declInfo.DeclaredSymbol).Distinct().WhereNotNull();
	}

	private static ImmutableArray<CompilationEvent> CreateCompilationEventsForTree(IEnumerable<ISymbol> declaredSymbols, SyntaxTree tree, Compilation compilation)
	{
		ImmutableArray<CompilationEvent>.Builder builder = ImmutableArray.CreateBuilder<CompilationEvent>();
		foreach (ISymbol declaredSymbol in declaredSymbols)
		{
			builder.Add(new SymbolDeclaredCompilationEvent(compilation, declaredSymbol));
		}
		builder.Add(new CompilationUnitCompletedEvent(compilation, tree));
		return builder.ToImmutable();
	}

	private void GenerateSimulatedCompilatioNonSourceEvent(Compilation compilation, bool started)
	{
		lock (gate)
		{
			if (!(started ? compilationStartGenerated : compilationEndGenerated))
			{
				ImmutableArray<CompilationEvent> compilationEvents = ImmutableArray.Create(started ? ((CompilationEvent)new CompilationStartedEvent(compilation)) : ((CompilationEvent)new CompilationCompletedEvent(compilation)));
				OnCompilationEventsGenerated_NoLock(compilationEvents, null);
				if (started)
				{
					compilationStartGenerated = true;
				}
				else
				{
					compilationEndGenerated = true;
				}
			}
		}
	}

	public async Task OnCompilationEventsGeneratedAsync(ImmutableArray<CompilationEvent> compilationEvents, AnalyzerDriver driver, CancellationToken cancellationToken)
	{
		await EnsureAnalyzerActionCountsInitializedAsync(driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		lock (gate)
		{
			OnCompilationEventsGenerated_NoLock(compilationEvents, null);
		}
	}

	private void OnCompilationEventsGenerated_NoLock(ImmutableArray<CompilationEvent> compilationEvents, SyntaxTree filterTreeOpt)
	{
		DebugAssertHelper.Assert(lazyAnalyzerActionCountsMap != null);
		AddToEventsMap_NoLock(compilationEvents, filterTreeOpt);
		ArrayBuilder<ISymbol> arrayBuilder = null;
		DebugAssertHelper.Assert(pooledEventsWithAnyActionsSet.Count == 0);
		foreach (KeyValuePair<DiagnosticAnalyzer, int> item in analyzerStateMap)
		{
			DiagnosticAnalyzer key = item.Key;
			PerAnalyzerState perAnalyzerState = analyzerStates[item.Value];
			AnalyzerActionCounts actionCounts = lazyAnalyzerActionCountsMap[key];
			foreach (CompilationEvent item2 in compilationEvents)
			{
				if (!HasActionsForEvent(item2, actionCounts))
				{
					continue;
				}
				pooledEventsWithAnyActionsSet.Add(item2);
				if (item2 is SymbolDeclaredCompilationEvent { DeclaringSyntaxReferences: { Length: >1 } } symbolDeclaredCompilationEvent)
				{
					if (partialSymbolsWithGeneratedSourceEvents.Contains(symbolDeclaredCompilationEvent.Symbol))
					{
						continue;
					}
					arrayBuilder = arrayBuilder ?? ArrayBuilder<ISymbol>.GetInstance();
					arrayBuilder.Add(symbolDeclaredCompilationEvent.Symbol);
				}
				perAnalyzerState.OnCompilationEventGenerated(item2, actionCounts);
			}
		}
		foreach (CompilationEvent item3 in compilationEvents)
		{
			if (!pooledEventsWithAnyActionsSet.Remove(item3))
			{
				UpdateEventsMap_NoLock(item3, add: false);
			}
		}
		if (arrayBuilder != null)
		{
			partialSymbolsWithGeneratedSourceEvents.AddAll(arrayBuilder);
			arrayBuilder.Free();
		}
	}

	private void AddToEventsMap_NoLock(ImmutableArray<CompilationEvent> compilationEvents, SyntaxTree filterTreeOpt)
	{
		if (filterTreeOpt != null)
		{
			AddPendingSourceEvents_NoLock(compilationEvents, filterTreeOpt);
			return;
		}
		foreach (CompilationEvent item in compilationEvents)
		{
			UpdateEventsMap_NoLock(item, add: true);
		}
	}

	private void UpdateEventsMap_NoLock(CompilationEvent compilationEvent, bool add)
	{
		if (compilationEvent is SymbolDeclaredCompilationEvent symbolDeclaredCompilationEvent)
		{
			foreach (Location location in symbolDeclaredCompilationEvent.Symbol.Locations)
			{
				if (location.SourceTree != null)
				{
					if (add)
					{
						AddPendingSourceEvent_NoLock(location.SourceTree, compilationEvent);
					}
					else
					{
						RemovePendingSourceEvent_NoLock(location.SourceTree, compilationEvent);
					}
				}
			}
		}
		else if (compilationEvent is CompilationUnitCompletedEvent compilationUnitCompletedEvent)
		{
			SyntaxTree syntaxTree = compilationUnitCompletedEvent.SemanticModel.SyntaxTree;
			if (add)
			{
				AddPendingSourceEvent_NoLock(syntaxTree, compilationEvent);
			}
			else
			{
				RemovePendingSourceEvent_NoLock(syntaxTree, compilationEvent);
			}
		}
		else
		{
			if (!(compilationEvent is CompilationStartedEvent) && !(compilationEvent is CompilationCompletedEvent))
			{
				throw new InvalidOperationException("Unexpected compilation event of type " + compilationEvent.GetType().Name);
			}
			if (add)
			{
				pendingNonSourceEvents.Add(compilationEvent);
			}
			else
			{
				pendingNonSourceEvents.Remove(compilationEvent);
			}
		}
	}

	private void AddPendingSourceEvents_NoLock(ImmutableArray<CompilationEvent> compilationEvents, SyntaxTree tree)
	{
		if (!pendingSourceEvents.TryGetValue(tree, out var value))
		{
			value = new HashSet<CompilationEvent>(compilationEvents);
			pendingSourceEvents[tree] = value;
			compilationData.RemoveCachedSemanticModel(tree);
		}
		else
		{
			value.AddAll(compilationEvents);
		}
	}

	private void AddPendingSourceEvent_NoLock(SyntaxTree tree, CompilationEvent compilationEvent)
	{
		if (!pendingSourceEvents.TryGetValue(tree, out var value))
		{
			value = new HashSet<CompilationEvent>();
			pendingSourceEvents[tree] = value;
			compilationData.RemoveCachedSemanticModel(tree);
		}
		value.Add(compilationEvent);
	}

	private void RemovePendingSourceEvent_NoLock(SyntaxTree tree, CompilationEvent compilationEvent)
	{
		if (pendingSourceEvents.TryGetValue(tree, out var value) && value.Remove(compilationEvent) && value.Count == 0)
		{
			pendingSourceEvents.Remove(tree);
			compilationData.RemoveCachedSemanticModel(tree);
		}
	}

	private async Task EnsureAnalyzerActionCountsInitializedAsync(AnalyzerDriver driver, CancellationToken cancellationToken)
	{
		if (lazyAnalyzerActionCountsMap != null)
		{
			return;
		}
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, AnalyzerActionCounts>();
		foreach (DiagnosticAnalyzer analyzer in analyzerStateMap.Keys)
		{
			builder.Add(analyzer, await driver.GetAnalyzerActionCountsAsync(analyzer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		Interlocked.CompareExchange(ref lazyAnalyzerActionCountsMap, builder.ToImmutable(), null);
	}

	internal async Task<AnalyzerActionCounts> GetOrComputeAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, AnalyzerDriver driver, CancellationToken cancellationToken)
	{
		await EnsureAnalyzerActionCountsInitializedAsync(driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return lazyAnalyzerActionCountsMap[analyzer];
	}

	internal AnalyzerActionCounts GetAnalyzerActionCounts(DiagnosticAnalyzer analyzer)
	{
		DebugAssertHelper.Assert(lazyAnalyzerActionCountsMap != null);
		return lazyAnalyzerActionCountsMap[analyzer];
	}

	private static bool HasActionsForEvent(CompilationEvent compilationEvent, AnalyzerActionCounts actionCounts)
	{
		if (compilationEvent is CompilationStartedEvent)
		{
			if (actionCounts.CompilationActionsCount <= 0)
			{
				return actionCounts.SyntaxTreeActionsCount > 0;
			}
			return true;
		}
		if (compilationEvent is CompilationCompletedEvent)
		{
			return actionCounts.CompilationEndActionsCount > 0;
		}
		if (compilationEvent is SymbolDeclaredCompilationEvent)
		{
			if (actionCounts.SymbolActionsCount <= 0)
			{
				return actionCounts.HasAnyExecutableCodeActions;
			}
			return true;
		}
		return actionCounts.SemanticModelActionsCount > 0;
	}

	private void OnSymbolDeclaredEventProcessed(SymbolDeclaredCompilationEvent symbolDeclaredEvent, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			GetAnalyzerState(item).OnSymbolDeclaredEventProcessed(symbolDeclaredEvent);
		}
	}

	public void OnCompilationEventProcessed(CompilationEvent compilationEvent, AnalysisScope analysisScope)
	{
		if (compilationEvent is SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			OnSymbolDeclaredEventProcessed(symbolDeclaredEvent, analysisScope.Analyzers);
		}
		foreach (PerAnalyzerState analyzerState in analyzerStates)
		{
			if (!analyzerState.IsEventAnalyzed(compilationEvent))
			{
				return;
			}
		}
		lock (gate)
		{
			UpdateEventsMap_NoLock(compilationEvent, add: false);
		}
	}

	public ImmutableArray<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree tree)
	{
		lock (gate)
		{
			return GetPendingEvents_NoLock(analyzers, tree);
		}
	}

	private HashSet<CompilationEvent> GetPendingEvents_NoLock(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		HashSet<CompilationEvent> hashSet = compilationEventsPool.Allocate();
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			GetAnalyzerState(item).AddPendingEvents(hashSet);
		}
		return hashSet;
	}

	private ImmutableArray<CompilationEvent> GetPendingEvents_NoLock(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree tree)
	{
		if (pendingSourceEvents.TryGetValue(tree, out var value) && value != null && value.Count > 0)
		{
			HashSet<CompilationEvent> hashSet = null;
			try
			{
				hashSet = GetPendingEvents_NoLock(analyzers);
				if (hashSet.Count > 0)
				{
					hashSet.IntersectWith(value);
					return hashSet.ToImmutableArray();
				}
			}
			finally
			{
				Free(hashSet);
			}
		}
		return ImmutableArray<CompilationEvent>.Empty;
	}

	public ImmutableArray<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeSourceEvents, bool includeNonSourceEvents)
	{
		lock (gate)
		{
			return GetPendingEvents_NoLock(analyzers, includeSourceEvents, includeNonSourceEvents);
		}
	}

	private ImmutableArray<CompilationEvent> GetPendingEvents_NoLock(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeSourceEvents, bool includeNonSourceEvents)
	{
		HashSet<CompilationEvent> hashSet = null;
		HashSet<CompilationEvent> hashSet2 = null;
		try
		{
			hashSet = GetPendingEvents_NoLock(analyzers);
			if (hashSet.Count == 0)
			{
				return ImmutableArray<CompilationEvent>.Empty;
			}
			hashSet2 = compilationEventsPool.Allocate();
			if (includeSourceEvents)
			{
				foreach (HashSet<CompilationEvent> value in pendingSourceEvents.Values)
				{
					foreach (CompilationEvent item in value)
					{
						hashSet2.Add(item);
					}
				}
			}
			if (includeNonSourceEvents)
			{
				foreach (CompilationEvent pendingNonSourceEvent in pendingNonSourceEvents)
				{
					hashSet2.Add(pendingNonSourceEvent);
				}
			}
			hashSet2.IntersectWith(hashSet);
			return hashSet2.ToImmutableArray();
		}
		finally
		{
			Free(hashSet);
			Free(hashSet2);
		}
	}

	private void Free(HashSet<CompilationEvent> events)
	{
		if (events != null)
		{
			events.Clear();
			compilationEventsPool.Free(events);
		}
	}

	public bool HasPendingSyntaxAnalysis(AnalysisScope analysisScope)
	{
		if (analysisScope.IsTreeAnalysis && !analysisScope.IsSyntaxOnlyTreeAnalysis)
		{
			return false;
		}
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (GetAnalyzerState(analyzer).HasPendingSyntaxAnalysis(analysisScope.FilterTreeOpt))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPendingSymbolAnalysis(AnalysisScope analysisScope)
	{
		DebugAssertHelper.Assert(analysisScope.FilterTreeOpt != null);
		foreach (SymbolDeclaredCompilationEvent pendingSymbolDeclaredEvent in GetPendingSymbolDeclaredEvents(analysisScope.FilterTreeOpt))
		{
			if (!analysisScope.ShouldAnalyze(pendingSymbolDeclaredEvent.Symbol))
			{
				continue;
			}
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (GetAnalyzerState(analyzer).HasPendingSymbolAnalysis(pendingSymbolDeclaredEvent.Symbol))
				{
					return true;
				}
			}
		}
		return false;
	}

	private ImmutableArray<SymbolDeclaredCompilationEvent> GetPendingSymbolDeclaredEvents(SyntaxTree tree)
	{
		DebugAssertHelper.Assert(tree != null);
		lock (gate)
		{
			if (!pendingSourceEvents.TryGetValue(tree, out var value))
			{
				return ImmutableArray<SymbolDeclaredCompilationEvent>.Empty;
			}
			return value.OfType<SymbolDeclaredCompilationEvent>().ToImmutableArray();
		}
	}

	public bool TryStartProcessingEvent(CompilationEvent compilationEvent, DiagnosticAnalyzer analyzer, out AnalyzerStateData state)
	{
		return GetAnalyzerState(analyzer).TryStartProcessingEvent(compilationEvent, out state);
	}

	public void MarkEventComplete(CompilationEvent compilationEvent, DiagnosticAnalyzer analyzer)
	{
		GetAnalyzerState(analyzer).MarkEventComplete(compilationEvent);
	}

	public void MarkEventComplete(CompilationEvent compilationEvent, IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			GetAnalyzerState(analyzer).MarkEventComplete(compilationEvent);
		}
	}

	public bool TryStartAnalyzingSymbol(ISymbol symbol, DiagnosticAnalyzer analyzer, out AnalyzerStateData state)
	{
		return GetAnalyzerState(analyzer).TryStartAnalyzingSymbol(symbol, out state);
	}

	public void MarkSymbolComplete(ISymbol symbol, DiagnosticAnalyzer analyzer)
	{
		GetAnalyzerState(analyzer).MarkSymbolComplete(symbol);
	}

	public bool TryStartAnalyzingDeclaration(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, out DeclarationAnalyzerStateData state)
	{
		return GetAnalyzerState(analyzer).TryStartAnalyzingDeclaration(symbol, declarationIndex, out state);
	}

	public bool IsDeclarationComplete(ISymbol symbol, int declarationIndex)
	{
		foreach (PerAnalyzerState analyzerState in analyzerStates)
		{
			if (!analyzerState.IsDeclarationComplete(symbol, declarationIndex))
			{
				return false;
			}
		}
		return true;
	}

	public void MarkDeclarationComplete(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer)
	{
		GetAnalyzerState(analyzer).MarkDeclarationComplete(symbol, declarationIndex);
	}

	public void MarkDeclarationComplete(ISymbol symbol, int declarationIndex, IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			GetAnalyzerState(analyzer).MarkDeclarationComplete(symbol, declarationIndex);
		}
	}

	public void MarkDeclarationsComplete(ISymbol symbol, IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			GetAnalyzerState(analyzer).MarkDeclarationsComplete(symbol);
		}
	}

	public bool TryStartSyntaxAnalysis(SyntaxTree tree, DiagnosticAnalyzer analyzer, out AnalyzerStateData state)
	{
		return GetAnalyzerState(analyzer).TryStartSyntaxAnalysis(tree, out state);
	}

	public void MarkSyntaxAnalysisComplete(SyntaxTree tree, DiagnosticAnalyzer analyzer)
	{
		GetAnalyzerState(analyzer).MarkSyntaxAnalysisComplete(tree);
	}

	public void MarkSyntaxAnalysisComplete(SyntaxTree tree, IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			GetAnalyzerState(analyzer).MarkSyntaxAnalysisComplete(tree);
		}
	}

	public bool IsEventComplete(CompilationEvent compilationEvent, DiagnosticAnalyzer analyzer)
	{
		return GetAnalyzerState(analyzer).IsEventAnalyzed(compilationEvent);
	}
}
