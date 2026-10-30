using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

internal sealed class ControlFlowGraphBuilder : OperationVisitor<int?, IOperation>
{
	internal sealed class BasicBlockBuilder
	{
		internal struct Branch
		{
			public ControlFlowBranchSemantics Kind { get; set; }

			public BasicBlockBuilder Destination { get; set; }
		}

		public int Ordinal;

		public readonly BasicBlockKind Kind;

		private ArrayBuilder<IOperation> _statements;

		private BasicBlockBuilder _predecessor1;

		private BasicBlockBuilder _predecessor2;

		private PooledHashSet<BasicBlockBuilder> _predecessors;

		public IOperation BranchValue;

		public ControlFlowConditionKind ConditionKind;

		public Branch Conditional;

		public Branch FallThrough;

		public bool IsReachable;

		public ControlFlowRegion Region;

		public bool HasStatements
		{
			get
			{
				ArrayBuilder<IOperation> statements = _statements;
				if (statements == null)
				{
					return false;
				}
				return statements.Count > 0;
			}
		}

		public ArrayBuilder<IOperation> StatementsOpt => _statements;

		public bool HasPredecessors
		{
			get
			{
				if (_predecessors != null)
				{
					return _predecessors.Count > 0;
				}
				if (_predecessor1 == null)
				{
					return _predecessor2 != null;
				}
				return true;
			}
		}

		public bool HasCondition => ConditionKind != ControlFlowConditionKind.None;

		public BasicBlockBuilder(BasicBlockKind kind)
		{
			Kind = kind;
			Ordinal = -1;
			IsReachable = false;
		}

		public void AddStatement(IOperation operation)
		{
			if (_statements == null)
			{
				_statements = ArrayBuilder<IOperation>.GetInstance();
			}
			_statements.Add(operation);
		}

		public void MoveStatementsFrom(BasicBlockBuilder other)
		{
			if (other._statements != null)
			{
				if (_statements == null)
				{
					_statements = other._statements;
					other._statements = null;
				}
				else
				{
					_statements.AddRange(other._statements);
					other._statements.Clear();
				}
			}
		}

		public BasicBlock ToImmutable()
		{
			BasicBlock result = new BasicBlock(Kind, _statements?.ToImmutableAndFree() ?? ImmutableArray<IOperation>.Empty, BranchValue, ConditionKind, Ordinal, IsReachable, Region);
			_statements = null;
			return result;
		}

		public BasicBlockBuilder GetSingletonPredecessorOrDefault()
		{
			if (_predecessors != null)
			{
				return _predecessors.AsSingleton();
			}
			if (_predecessor2 == null)
			{
				return _predecessor1;
			}
			if (_predecessor1 == null)
			{
				return _predecessor2;
			}
			return null;
		}

		public void AddPredecessor(BasicBlockBuilder predecessor)
		{
			if (_predecessors != null)
			{
				_predecessors.Add(predecessor);
			}
			else if (_predecessor1 != predecessor && _predecessor2 != predecessor)
			{
				if (_predecessor1 == null)
				{
					_predecessor1 = predecessor;
					return;
				}
				if (_predecessor2 == null)
				{
					_predecessor2 = predecessor;
					return;
				}
				_predecessors = PooledHashSet<BasicBlockBuilder>.GetInstance();
				_predecessors.Add(_predecessor1);
				_predecessors.Add(_predecessor2);
				_predecessors.Add(predecessor);
				_predecessor1 = null;
				_predecessor2 = null;
			}
		}

		public void RemovePredecessor(BasicBlockBuilder predecessor)
		{
			if (_predecessors != null)
			{
				_predecessors.Remove(predecessor);
			}
			else if (_predecessor1 == predecessor)
			{
				_predecessor1 = null;
			}
			else if (_predecessor2 == predecessor)
			{
				_predecessor2 = null;
			}
		}

		public void GetPredecessors(ArrayBuilder<BasicBlockBuilder> builder)
		{
			if (_predecessors != null)
			{
				foreach (BasicBlockBuilder predecessor in _predecessors)
				{
					builder.Add(predecessor);
				}
				return;
			}
			if (_predecessor1 != null)
			{
				builder.Add(_predecessor1);
			}
			if (_predecessor2 != null)
			{
				builder.Add(_predecessor2);
			}
		}

		public ImmutableArray<ControlFlowBranch> ConvertPredecessorsToBranches(ArrayBuilder<BasicBlock> blocks)
		{
			if (!HasPredecessors)
			{
				_predecessors?.Free();
				_predecessors = null;
				return ImmutableArray<ControlFlowBranch>.Empty;
			}
			BasicBlock block = blocks[Ordinal];
			ArrayBuilder<ControlFlowBranch> branches = ArrayBuilder<ControlFlowBranch>.GetInstance(_predecessors?.Count ?? 2);
			if (_predecessors != null)
			{
				foreach (BasicBlockBuilder predecessor in _predecessors)
				{
					addBranches(predecessor);
				}
				_predecessors.Free();
				_predecessors = null;
			}
			else
			{
				if (_predecessor1 != null)
				{
					addBranches(_predecessor1);
					_predecessor1 = null;
				}
				if (_predecessor2 != null)
				{
					addBranches(_predecessor2);
					_predecessor2 = null;
				}
			}
			branches.Sort((ControlFlowBranch x, ControlFlowBranch y) =>
			{
				int num = x.Source.Ordinal - y.Source.Ordinal;
				if (num == 0 && x.IsConditionalSuccessor != y.IsConditionalSuccessor)
				{
					num = ((!x.IsConditionalSuccessor) ? 1 : (-1));
				}
				return num;
			});
			return branches.ToImmutableAndFree();
			void addBranches(BasicBlockBuilder predecessorBlockBuilder)
			{
				BasicBlock basicBlock = blocks[predecessorBlockBuilder.Ordinal];
				if (basicBlock.FallThroughSuccessor.Destination == block)
				{
					branches.Add(basicBlock.FallThroughSuccessor);
				}
				if (basicBlock.ConditionalSuccessor?.Destination == block)
				{
					branches.Add(basicBlock.ConditionalSuccessor);
				}
			}
		}

		public void Free()
		{
			Ordinal = -1;
			_statements?.Free();
			_statements = null;
			_predecessors?.Free();
			_predecessors = null;
			_predecessor1 = null;
			_predecessor2 = null;
		}
	}

	internal class CaptureIdDispenser
	{
		private int _captureId = -1;

		public int GetNextId()
		{
			return Interlocked.Increment(ref _captureId);
		}

		public int GetCurrentId()
		{
			return _captureId;
		}
	}

	internal struct Context
	{
		public readonly IOperation ImplicitInstance;

		public readonly INamedTypeSymbol AnonymousType;

		public readonly ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>> AnonymousTypePropertyValues;

		internal Context(IOperation implicitInstance, INamedTypeSymbol anonymousType, ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>> anonymousTypePropertyValues)
		{
			ImplicitInstance = implicitInstance;
			AnonymousType = anonymousType;
			AnonymousTypePropertyValues = anonymousTypePropertyValues;
		}
	}

	private class EvalStackFrame
	{
		private RegionBuilder _lazyRegionBuilder;

		public RegionBuilder RegionBuilderOpt
		{
			get
			{
				return _lazyRegionBuilder;
			}
			set
			{
				_lazyRegionBuilder = value;
			}
		}
	}

	private struct ImplicitInstanceInfo
	{
		public IOperation ImplicitInstance { get; }

		public INamedTypeSymbol AnonymousType { get; }

		public PooledDictionary<IPropertySymbol, IOperation> AnonymousTypePropertyValues { get; }

		public ImplicitInstanceInfo(IOperation currentImplicitInstance)
		{
			ImplicitInstance = currentImplicitInstance;
			AnonymousType = null;
			AnonymousTypePropertyValues = null;
		}

		public ImplicitInstanceInfo(INamedTypeSymbol currentInitializedAnonymousType)
		{
			ImplicitInstance = null;
			AnonymousType = currentInitializedAnonymousType;
			AnonymousTypePropertyValues = PooledDictionary<IPropertySymbol, IOperation>.GetInstance();
		}

		public ImplicitInstanceInfo(in Context context)
		{
			if (context.ImplicitInstance != null)
			{
				ImplicitInstance = context.ImplicitInstance;
				AnonymousType = null;
				AnonymousTypePropertyValues = null;
			}
			else if (context.AnonymousType != null)
			{
				ImplicitInstance = null;
				AnonymousType = context.AnonymousType;
				AnonymousTypePropertyValues = PooledDictionary<IPropertySymbol, IOperation>.GetInstance();
				foreach (KeyValuePair<IPropertySymbol, IOperation> anonymousTypePropertyValue in context.AnonymousTypePropertyValues)
				{
					AnonymousTypePropertyValues.Add(anonymousTypePropertyValue.Key, anonymousTypePropertyValue.Value);
				}
			}
			else
			{
				ImplicitInstance = null;
				AnonymousType = null;
				AnonymousTypePropertyValues = null;
			}
		}

		public void Free()
		{
			AnonymousTypePropertyValues?.Free();
		}
	}

	private class RegionBuilder
	{
		private sealed class AnonymousFunctionsMapBuilder : OperationVisitor<(ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region), IOperation>
		{
			public static readonly AnonymousFunctionsMapBuilder Instance = new AnonymousFunctionsMapBuilder();

			public override IOperation VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
			{
				argument.map.Add(operation, (argument.region, argument.map.Count));
				return base.VisitFlowAnonymousFunction(operation, argument);
			}

			internal override IOperation VisitNoneOperation(IOperation operation, (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
			{
				return DefaultVisit(operation, argument);
			}

			public override IOperation DefaultVisit(IOperation operation, (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
			{
				foreach (IOperation child in operation.Children)
				{
					Visit(child, argument);
				}
				return null;
			}
		}

		public ControlFlowRegionKind Kind;

		public readonly ITypeSymbol ExceptionType;

		public BasicBlockBuilder FirstBlock;

		public BasicBlockBuilder LastBlock;

		public ArrayBuilder<RegionBuilder> Regions;

		public ImmutableArray<ILocalSymbol> Locals;

		public ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)> LocalFunctions;

		public ArrayBuilder<CaptureId> CaptureIds;

		public readonly bool IsStackSpillRegion;

		public RegionBuilder Enclosing { get; private set; }

		public bool IsEmpty => FirstBlock == null;

		public bool HasRegions
		{
			get
			{
				ArrayBuilder<RegionBuilder> regions = Regions;
				if (regions == null)
				{
					return false;
				}
				return regions.Count > 0;
			}
		}

		public bool HasLocalFunctions
		{
			get
			{
				ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)> localFunctions = LocalFunctions;
				if (localFunctions == null)
				{
					return false;
				}
				return localFunctions.Count > 0;
			}
		}

		public bool HasCaptureIds
		{
			get
			{
				ArrayBuilder<CaptureId> captureIds = CaptureIds;
				if (captureIds == null)
				{
					return false;
				}
				return captureIds.Count > 0;
			}
		}

		public RegionBuilder(ControlFlowRegionKind kind, ITypeSymbol exceptionType = null, ImmutableArray<ILocalSymbol> locals = default(ImmutableArray<ILocalSymbol>), bool isStackSpillRegion = false)
		{
			Kind = kind;
			ExceptionType = exceptionType;
			Locals = locals.NullToEmpty();
			IsStackSpillRegion = isStackSpillRegion;
		}

		public void AddCaptureId(int captureId)
		{
			if (CaptureIds == null)
			{
				CaptureIds = ArrayBuilder<CaptureId>.GetInstance();
			}
			CaptureIds.Add(new CaptureId(captureId));
		}

		public void AddCaptureIds(ArrayBuilder<CaptureId> others)
		{
			if (others != null)
			{
				if (CaptureIds == null)
				{
					CaptureIds = ArrayBuilder<CaptureId>.GetInstance();
				}
				CaptureIds.AddRange(others);
			}
		}

		public void Add(IMethodSymbol symbol, ILocalFunctionOperation operation)
		{
			if (LocalFunctions == null)
			{
				LocalFunctions = ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>.GetInstance();
			}
			LocalFunctions.Add((symbol, operation));
		}

		public void AddRange(ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)> others)
		{
			if (others != null)
			{
				if (LocalFunctions == null)
				{
					LocalFunctions = ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>.GetInstance();
				}
				LocalFunctions.AddRange(others);
			}
		}

		public void Add(RegionBuilder region)
		{
			if (Regions == null)
			{
				Regions = ArrayBuilder<RegionBuilder>.GetInstance();
			}
			region.Enclosing = this;
			Regions.Add(region);
		}

		public void Remove(RegionBuilder region)
		{
			if (Regions.Count == 1)
			{
				Regions.Clear();
			}
			else
			{
				Regions.RemoveAt(Regions.IndexOf(region));
			}
			region.Enclosing = null;
		}

		public void ReplaceRegion(RegionBuilder toReplace, ArrayBuilder<RegionBuilder> replaceWith)
		{
			int num = ((Regions.Count != 1) ? Regions.IndexOf(toReplace) : 0);
			int count = replaceWith.Count;
			if (count == 1)
			{
				RegionBuilder regionBuilder = replaceWith[0];
				regionBuilder.Enclosing = this;
				Regions[num] = regionBuilder;
			}
			else
			{
				int count2 = Regions.Count;
				Regions.Count = count - 1 + count2;
				int num2 = count2 - 1;
				int num3 = Regions.Count - 1;
				while (num2 > num)
				{
					Regions[num3] = Regions[num2];
					num2--;
					num3--;
				}
				foreach (RegionBuilder item in replaceWith)
				{
					item.Enclosing = this;
					Regions[num++] = item;
				}
			}
			toReplace.Enclosing = null;
		}

		public void ExtendToInclude(BasicBlockBuilder block)
		{
			if (FirstBlock == null)
			{
				if (!HasRegions)
				{
					FirstBlock = block;
					LastBlock = block;
					return;
				}
				FirstBlock = Regions.First().FirstBlock;
			}
			LastBlock = block;
		}

		public void Free()
		{
			Enclosing = null;
			FirstBlock = null;
			LastBlock = null;
			Regions?.Free();
			Regions = null;
			LocalFunctions?.Free();
			LocalFunctions = null;
			CaptureIds?.Free();
			CaptureIds = null;
		}

		public ControlFlowRegion ToImmutableRegionAndFree(ArrayBuilder<BasicBlockBuilder> blocks, ArrayBuilder<IMethodSymbol> localFunctions, ImmutableDictionary<IMethodSymbol, (ControlFlowRegion region, ILocalFunctionOperation operation, int ordinal)>.Builder localFunctionsMap, ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder anonymousFunctionsMapOpt, ControlFlowRegion enclosing)
		{
			int count = localFunctions.Count;
			if (HasLocalFunctions)
			{
				foreach (var localFunction in LocalFunctions)
				{
					IMethodSymbol item = localFunction.Item1;
					localFunctions.Add(item);
				}
			}
			ImmutableArray<ControlFlowRegion> nestedRegions;
			if (HasRegions)
			{
				ArrayBuilder<ControlFlowRegion> instance = ArrayBuilder<ControlFlowRegion>.GetInstance(Regions.Count);
				foreach (RegionBuilder region in Regions)
				{
					instance.Add(region.ToImmutableRegionAndFree(blocks, localFunctions, localFunctionsMap, anonymousFunctionsMapOpt, null));
				}
				nestedRegions = instance.ToImmutableAndFree();
			}
			else
			{
				nestedRegions = ImmutableArray<ControlFlowRegion>.Empty;
			}
			CaptureIds?.Sort((CaptureId x, CaptureId y) => x.Value.CompareTo(y.Value));
			ControlFlowRegion result = new ControlFlowRegion(Kind, FirstBlock.Ordinal, LastBlock.Ordinal, nestedRegions, Locals, LocalFunctions?.SelectAsArray(((IMethodSymbol, ILocalFunctionOperation) tuple2) => tuple2.Item1) ?? default(ImmutableArray<IMethodSymbol>), CaptureIds?.ToImmutable() ?? default(ImmutableArray<CaptureId>), ExceptionType, enclosing);
			if (HasLocalFunctions)
			{
				foreach (var (key, item2) in LocalFunctions)
				{
					localFunctionsMap.Add(key, (result, item2, count++));
				}
			}
			int num = FirstBlock.Ordinal;
			foreach (ControlFlowRegion item3 in nestedRegions)
			{
				for (int num2 = num; num2 < item3.FirstBlockOrdinal; num2++)
				{
					setRegion(blocks[num2]);
				}
				num = item3.LastBlockOrdinal + 1;
			}
			for (int num3 = num; num3 <= LastBlock.Ordinal; num3++)
			{
				setRegion(blocks[num3]);
			}
			Free();
			return result;
			void setRegion(BasicBlockBuilder block)
			{
				block.Region = result;
				if (anonymousFunctionsMapOpt != null)
				{
					(ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion, int)>.Builder, ControlFlowRegion) argument = (anonymousFunctionsMapOpt, result);
					if (block.HasStatements)
					{
						foreach (IOperation item4 in block.StatementsOpt)
						{
							AnonymousFunctionsMapBuilder.Instance.Visit(item4, argument);
						}
					}
					AnonymousFunctionsMapBuilder.Instance.Visit(block.BranchValue, argument);
				}
			}
		}
	}

	private readonly Compilation _compilation;

	private readonly BasicBlockBuilder _entry = new BasicBlockBuilder(BasicBlockKind.Entry);

	private readonly BasicBlockBuilder _exit = new BasicBlockBuilder(BasicBlockKind.Exit);

	private ArrayBuilder<BasicBlockBuilder> _blocks;

	private PooledDictionary<BasicBlockBuilder, RegionBuilder> _regionMap;

	private BasicBlockBuilder _currentBasicBlock;

	private RegionBuilder _currentRegion;

	private PooledDictionary<ILabelSymbol, BasicBlockBuilder> _labeledBlocks;

	private bool _haveAnonymousFunction;

	private IOperation _currentStatement;

	private ArrayBuilder<(EvalStackFrame frameOpt, IOperation operationOpt)> _evalStack;

	private int _startSpillingAt;

	private IOperation _currentConditionalAccessInstance;

	private IOperation _currentSwitchOperationExpression;

	private IOperation _forToLoopBinaryOperatorLeftOperand;

	private IOperation _forToLoopBinaryOperatorRightOperand;

	private IOperation _currentAggregationGroup;

	private bool _forceImplicit;

	private readonly CaptureIdDispenser _captureIdDispenser;

	private ImplicitInstanceInfo _currentImplicitInstance;

	private BasicBlockBuilder CurrentBasicBlock
	{
		get
		{
			if (_currentBasicBlock == null)
			{
				AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
			}
			return _currentBasicBlock;
		}
	}

	private Context GetCurrentContext()
	{
		return new Context(_currentImplicitInstance.ImplicitInstance, _currentImplicitInstance.AnonymousType, _currentImplicitInstance.AnonymousTypePropertyValues?.ToImmutableArray() ?? ImmutableArray<KeyValuePair<IPropertySymbol, IOperation>>.Empty);
	}

	private void SetCurrentContext(in Context context)
	{
		_currentImplicitInstance = new ImplicitInstanceInfo(in context);
	}

	private ControlFlowGraphBuilder(Compilation compilation, CaptureIdDispenser captureIdDispenser)
	{
		_compilation = compilation;
		_captureIdDispenser = captureIdDispenser ?? new CaptureIdDispenser();
	}

	private bool IsImplicit(IOperation operation)
	{
		if (!_forceImplicit)
		{
			return operation.IsImplicit;
		}
		return true;
	}

	public static ControlFlowGraph Create(IOperation body, ControlFlowRegion enclosing = null, CaptureIdDispenser captureIdDispenser = null, in Context context = default(Context))
	{
		ControlFlowGraphBuilder controlFlowGraphBuilder = new ControlFlowGraphBuilder(((Operation)body).OwningSemanticModel.Compilation, captureIdDispenser);
		ArrayBuilder<BasicBlockBuilder> arrayBuilder = (controlFlowGraphBuilder._blocks = ArrayBuilder<BasicBlockBuilder>.GetInstance());
		controlFlowGraphBuilder._evalStack = ArrayBuilder<(EvalStackFrame, IOperation)>.GetInstance();
		controlFlowGraphBuilder._regionMap = PooledDictionary<BasicBlockBuilder, RegionBuilder>.GetInstance();
		RegionBuilder regionBuilder = new RegionBuilder(ControlFlowRegionKind.Root);
		controlFlowGraphBuilder.EnterRegion(regionBuilder);
		controlFlowGraphBuilder.AppendNewBlock(controlFlowGraphBuilder._entry, linkToPrevious: false);
		controlFlowGraphBuilder._currentBasicBlock = null;
		controlFlowGraphBuilder.SetCurrentContext(in context);
		controlFlowGraphBuilder.EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime));
		switch (body.Kind)
		{
		case OperationKind.LocalFunction:
			controlFlowGraphBuilder.VisitLocalFunctionAsRoot((ILocalFunctionOperation)body);
			break;
		case OperationKind.AnonymousFunction:
		{
			IAnonymousFunctionOperation anonymousFunctionOperation = (IAnonymousFunctionOperation)body;
			controlFlowGraphBuilder.VisitStatement(anonymousFunctionOperation.Body);
			break;
		}
		default:
			controlFlowGraphBuilder.VisitStatement(body);
			break;
		}
		controlFlowGraphBuilder.LeaveRegion();
		controlFlowGraphBuilder.AppendNewBlock(controlFlowGraphBuilder._exit);
		controlFlowGraphBuilder.LeaveRegion();
		controlFlowGraphBuilder._currentImplicitInstance.Free();
		CheckUnresolvedBranches(arrayBuilder, controlFlowGraphBuilder._labeledBlocks);
		Pack(arrayBuilder, regionBuilder, controlFlowGraphBuilder._regionMap);
		ArrayBuilder<IMethodSymbol> instance = ArrayBuilder<IMethodSymbol>.GetInstance();
		ImmutableDictionary<IMethodSymbol, (ControlFlowRegion, ILocalFunctionOperation, int)>.Builder builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, (ControlFlowRegion, ILocalFunctionOperation, int)>();
		ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion, int)>.Builder builder2 = null;
		if (controlFlowGraphBuilder._haveAnonymousFunction)
		{
			builder2 = ImmutableDictionary.CreateBuilder<IFlowAnonymousFunctionOperation, (ControlFlowRegion, int)>();
		}
		ControlFlowRegion root = regionBuilder.ToImmutableRegionAndFree(arrayBuilder, instance, builder, builder2, enclosing);
		MarkReachableBlocks(arrayBuilder);
		controlFlowGraphBuilder._evalStack.Free();
		controlFlowGraphBuilder._regionMap.Free();
		controlFlowGraphBuilder._labeledBlocks?.Free();
		return new ControlFlowGraph(body, controlFlowGraphBuilder._captureIdDispenser, ToImmutableBlocks(arrayBuilder), root, instance.ToImmutableAndFree(), builder.ToImmutable(), builder2?.ToImmutable() ?? ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion, int)>.Empty);
	}

	private static ImmutableArray<BasicBlock> ToImmutableBlocks(ArrayBuilder<BasicBlockBuilder> blockBuilders)
	{
		ArrayBuilder<BasicBlock> builder = ArrayBuilder<BasicBlock>.GetInstance(blockBuilders.Count);
		foreach (BasicBlockBuilder blockBuilder in blockBuilders)
		{
			builder.Add(blockBuilder.ToImmutable());
		}
		foreach (BasicBlockBuilder blockBuilder2 in blockBuilders)
		{
			ControlFlowBranch successor = getFallThroughSuccessor(blockBuilder2);
			ControlFlowBranch conditionalSuccessor = getConditionalSuccessor(blockBuilder2);
			builder[blockBuilder2.Ordinal].SetSuccessors(successor, conditionalSuccessor);
		}
		foreach (BasicBlockBuilder blockBuilder3 in blockBuilders)
		{
			builder[blockBuilder3.Ordinal].SetPredecessors(blockBuilder3.ConvertPredecessorsToBranches(builder));
		}
		return builder.ToImmutableAndFree();
		ControlFlowBranch getBranch(in BasicBlockBuilder.Branch branch, BasicBlockBuilder source, bool isConditionalSuccessor)
		{
			return new ControlFlowBranch(builder[source.Ordinal], (branch.Destination != null) ? builder[branch.Destination.Ordinal] : null, branch.Kind, isConditionalSuccessor);
		}
		ControlFlowBranch getConditionalSuccessor(BasicBlockBuilder blockBuilder)
		{
			if (!blockBuilder.HasCondition)
			{
				return null;
			}
			return getBranch(in blockBuilder.Conditional, blockBuilder, isConditionalSuccessor: true);
		}
		ControlFlowBranch getFallThroughSuccessor(BasicBlockBuilder blockBuilder)
		{
			if (blockBuilder.Kind == BasicBlockKind.Exit)
			{
				return null;
			}
			return getBranch(in blockBuilder.FallThrough, blockBuilder, isConditionalSuccessor: false);
		}
	}

	private static void MarkReachableBlocks(ArrayBuilder<BasicBlockBuilder> blocks)
	{
		PooledDictionary<ControlFlowRegion, bool> instance = PooledDictionary<ControlFlowRegion, bool>.GetInstance();
		PooledHashSet<ControlFlowRegion> instance2 = PooledHashSet<ControlFlowRegion>.GetInstance();
		MarkReachableBlocks(blocks, 0, blocks.Count - 1, null, instance, instance2, out var _);
		instance.Free();
		instance2.Free();
	}

	private static BitVector MarkReachableBlocks(ArrayBuilder<BasicBlockBuilder> blocks, int firstBlockOrdinal, int lastBlockOrdinal, ArrayBuilder<BasicBlockBuilder> outOfRangeBlocksToVisit, PooledDictionary<ControlFlowRegion, bool> continueDispatchAfterFinally, PooledHashSet<ControlFlowRegion> dispatchedExceptionsFromRegions, out bool fellThrough)
	{
		BitVector visited = BitVector.Empty;
		ArrayBuilder<BasicBlockBuilder> toVisit = ArrayBuilder<BasicBlockBuilder>.GetInstance();
		fellThrough = false;
		toVisit.Push(blocks[firstBlockOrdinal]);
		do
		{
			BasicBlockBuilder basicBlockBuilder = toVisit.Pop();
			if (basicBlockBuilder.Ordinal < firstBlockOrdinal || basicBlockBuilder.Ordinal > lastBlockOrdinal)
			{
				outOfRangeBlocksToVisit.Push(basicBlockBuilder);
			}
			else
			{
				if (visited[basicBlockBuilder.Ordinal])
				{
					continue;
				}
				visited[basicBlockBuilder.Ordinal] = true;
				basicBlockBuilder.IsReachable = true;
				bool flag = true;
				if (basicBlockBuilder.HasCondition)
				{
					if (basicBlockBuilder.BranchValue.ConstantValue.HasValue && basicBlockBuilder.BranchValue.ConstantValue.Value is bool flag2)
					{
						if (flag2 == (basicBlockBuilder.ConditionKind == ControlFlowConditionKind.WhenTrue))
						{
							followBranch(basicBlockBuilder, in basicBlockBuilder.Conditional);
							flag = false;
						}
					}
					else
					{
						followBranch(basicBlockBuilder, in basicBlockBuilder.Conditional);
					}
				}
				if (flag)
				{
					BasicBlockBuilder.Branch branch = basicBlockBuilder.FallThrough;
					followBranch(basicBlockBuilder, in branch);
					if (basicBlockBuilder.Ordinal == lastBlockOrdinal && branch.Kind != ControlFlowBranchSemantics.Throw && branch.Kind != ControlFlowBranchSemantics.Rethrow)
					{
						fellThrough = true;
					}
				}
				dispatchException(basicBlockBuilder.Region);
			}
		}
		while (toVisit.Count != 0);
		toVisit.Free();
		return visited;
		void dispatchException(ControlFlowRegion fromRegion)
		{
			while (dispatchedExceptionsFromRegions.Add(fromRegion))
			{
				ControlFlowRegion controlFlowRegion = ((fromRegion.Kind == ControlFlowRegionKind.Root) ? null : fromRegion.EnclosingRegion);
				if (fromRegion.Kind == ControlFlowRegionKind.Try)
				{
					switch (controlFlowRegion.Kind)
					{
					case ControlFlowRegionKind.TryAndFinally:
						if (!stepThroughSingleFinally(controlFlowRegion.NestedRegions[1]))
						{
							return;
						}
						break;
					case ControlFlowRegionKind.TryAndCatch:
						dispatchExceptionThroughCatches(controlFlowRegion, 1);
						break;
					default:
						throw ExceptionUtilities.UnexpectedValue(controlFlowRegion.Kind);
					}
				}
				else if (fromRegion.Kind == ControlFlowRegionKind.Filter)
				{
					ControlFlowRegion enclosingRegion = controlFlowRegion.EnclosingRegion;
					int num = enclosingRegion.NestedRegions.IndexOf(controlFlowRegion, 1);
					if (num <= 0)
					{
						throw ExceptionUtilities.Unreachable;
					}
					dispatchExceptionThroughCatches(enclosingRegion, num + 1);
					fromRegion = enclosingRegion;
					goto IL_00ac;
				}
				fromRegion = controlFlowRegion;
				goto IL_00ac;
				IL_00ac:
				if (fromRegion == null)
				{
					break;
				}
			}
		}
		void dispatchExceptionThroughCatches(ControlFlowRegion tryAndCatch, int startAt)
		{
			for (int i = startAt; i < tryAndCatch.NestedRegions.Length; i++)
			{
				ControlFlowRegion controlFlowRegion = tryAndCatch.NestedRegions[i];
				switch (controlFlowRegion.Kind)
				{
				case ControlFlowRegionKind.Catch:
					toVisit.Add(blocks[controlFlowRegion.FirstBlockOrdinal]);
					break;
				case ControlFlowRegionKind.FilterAndHandler:
				{
					BasicBlockBuilder item = blocks[controlFlowRegion.FirstBlockOrdinal];
					toVisit.Add(item);
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(controlFlowRegion.Kind);
				}
			}
		}
		void followBranch(BasicBlockBuilder current, in BasicBlockBuilder.Branch reference)
		{
			switch (reference.Kind)
			{
			case ControlFlowBranchSemantics.None:
			case ControlFlowBranchSemantics.StructuredExceptionHandling:
			case ControlFlowBranchSemantics.ProgramTermination:
			case ControlFlowBranchSemantics.Throw:
			case ControlFlowBranchSemantics.Rethrow:
			case ControlFlowBranchSemantics.Error:
				break;
			case ControlFlowBranchSemantics.Regular:
			case ControlFlowBranchSemantics.Return:
				if (stepThroughFinally(current.Region, reference.Destination))
				{
					toVisit.Add(reference.Destination);
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(reference.Kind);
			}
		}
		bool stepThroughFinally(ControlFlowRegion region, BasicBlockBuilder destination)
		{
			int ordinal = destination.Ordinal;
			while (!region.ContainsBlock(ordinal))
			{
				ControlFlowRegion enclosingRegion = region.EnclosingRegion;
				if (region.Kind == ControlFlowRegionKind.Try && enclosingRegion.Kind == ControlFlowRegionKind.TryAndFinally && !stepThroughSingleFinally(enclosingRegion.NestedRegions[1]))
				{
					return false;
				}
				region = enclosingRegion;
			}
			return true;
		}
		bool stepThroughSingleFinally(ControlFlowRegion @finally)
		{
			if (!continueDispatchAfterFinally.TryGetValue(@finally, out var value))
			{
				BitVector other = MarkReachableBlocks(blocks, @finally.FirstBlockOrdinal, @finally.LastBlockOrdinal, toVisit, continueDispatchAfterFinally, dispatchedExceptionsFromRegions, out var fellThrough2);
				visited.UnionWith(in other);
				value = fellThrough2 && blocks[@finally.LastBlockOrdinal].FallThrough.Kind == ControlFlowBranchSemantics.StructuredExceptionHandling;
				continueDispatchAfterFinally.Add(@finally, value);
			}
			return value;
		}
	}

	private static void Pack(ArrayBuilder<BasicBlockBuilder> blocks, RegionBuilder root, PooledDictionary<BasicBlockBuilder, RegionBuilder> regionMap)
	{
		bool flag = true;
		while ((flag | PackRegions(root, blocks, regionMap)) && PackBlocks(blocks, regionMap))
		{
			flag = false;
		}
	}

	private static bool PackRegions(RegionBuilder root, ArrayBuilder<BasicBlockBuilder> blocks, PooledDictionary<BasicBlockBuilder, RegionBuilder> regionMap)
	{
		return PackRegion(root);
		bool PackRegion(RegionBuilder region)
		{
			bool result = false;
			if (region.HasRegions)
			{
				for (int num = region.Regions.Count - 1; num >= 0; num--)
				{
					RegionBuilder regionBuilder = region.Regions[num];
					if (PackRegion(regionBuilder))
					{
						result = true;
					}
					if (regionBuilder.Kind == ControlFlowRegionKind.LocalLifetime && regionBuilder.Locals.IsEmpty && !regionBuilder.HasLocalFunctions && !regionBuilder.HasCaptureIds)
					{
						MergeSubRegionAndFree(regionBuilder, blocks, regionMap);
						result = true;
					}
				}
			}
			switch (region.Kind)
			{
			case ControlFlowRegionKind.Root:
			case ControlFlowRegionKind.LocalLifetime:
			case ControlFlowRegionKind.Try:
			case ControlFlowRegionKind.Filter:
			case ControlFlowRegionKind.Catch:
			case ControlFlowRegionKind.Finally:
			case ControlFlowRegionKind.StaticLocalInitializer:
			case ControlFlowRegionKind.ErroneousBody:
			{
				ArrayBuilder<RegionBuilder> regions = region.Regions;
				if (regions != null && regions.Count == 1)
				{
					RegionBuilder regionBuilder2 = region.Regions[0];
					if (regionBuilder2.Kind == ControlFlowRegionKind.LocalLifetime && regionBuilder2.FirstBlock == region.FirstBlock && regionBuilder2.LastBlock == region.LastBlock)
					{
						region.Locals = region.Locals.Concat(regionBuilder2.Locals);
						region.AddRange(regionBuilder2.LocalFunctions);
						region.AddCaptureIds(regionBuilder2.CaptureIds);
						MergeSubRegionAndFree(regionBuilder2, blocks, regionMap);
						result = true;
						break;
					}
				}
				if (region.HasRegions)
				{
					for (int num2 = region.Regions.Count - 1; num2 >= 0; num2--)
					{
						RegionBuilder regionBuilder3 = region.Regions[num2];
						if (regionBuilder3.Kind == ControlFlowRegionKind.LocalLifetime && !regionBuilder3.HasLocalFunctions && !regionBuilder3.HasRegions && regionBuilder3.FirstBlock == regionBuilder3.LastBlock)
						{
							BasicBlockBuilder firstBlock = regionBuilder3.FirstBlock;
							if (!firstBlock.HasStatements && firstBlock.BranchValue == null)
							{
								regionMap[firstBlock] = region;
								regionBuilder3.Free();
								region.Regions.RemoveAt(num2);
								result = true;
							}
						}
					}
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(region.Kind);
			case ControlFlowRegionKind.FilterAndHandler:
			case ControlFlowRegionKind.TryAndCatch:
			case ControlFlowRegionKind.TryAndFinally:
				break;
			}
			return result;
		}
	}

	private static void MergeSubRegionAndFree(RegionBuilder subRegion, ArrayBuilder<BasicBlockBuilder> blocks, PooledDictionary<BasicBlockBuilder, RegionBuilder> regionMap)
	{
		RegionBuilder enclosing = subRegion.Enclosing;
		int num = subRegion.FirstBlock.Ordinal;
		if (subRegion.HasRegions)
		{
			foreach (RegionBuilder region in subRegion.Regions)
			{
				for (int i = num; i < region.FirstBlock.Ordinal; i++)
				{
					regionMap[blocks[i]] = enclosing;
				}
				num = region.LastBlock.Ordinal + 1;
			}
			enclosing.ReplaceRegion(subRegion, subRegion.Regions);
		}
		else
		{
			enclosing.Remove(subRegion);
		}
		for (int j = num; j <= subRegion.LastBlock.Ordinal; j++)
		{
			regionMap[blocks[j]] = enclosing;
		}
		subRegion.Free();
	}

	private static bool PackBlocks(ArrayBuilder<BasicBlockBuilder> blocks, PooledDictionary<BasicBlockBuilder, RegionBuilder> regionMap)
	{
		ArrayBuilder<RegionBuilder> fromCurrent = null;
		ArrayBuilder<RegionBuilder> fromDestination = null;
		ArrayBuilder<RegionBuilder> fromPredecessor = null;
		ArrayBuilder<BasicBlockBuilder> arrayBuilder = null;
		bool result = false;
		bool flag;
		do
		{
			flag = false;
			int num = blocks.Count - 1;
			for (int i = 1; i < num; i++)
			{
				BasicBlockBuilder basicBlockBuilder = blocks[i];
				basicBlockBuilder.Ordinal = i;
				if (basicBlockBuilder.HasStatements)
				{
					BasicBlockBuilder singletonPredecessorOrDefault = basicBlockBuilder.GetSingletonPredecessorOrDefault();
					if (singletonPredecessorOrDefault == null || singletonPredecessorOrDefault.HasCondition || singletonPredecessorOrDefault.Ordinal >= basicBlockBuilder.Ordinal || singletonPredecessorOrDefault.Kind == BasicBlockKind.Entry || singletonPredecessorOrDefault.FallThrough.Destination != basicBlockBuilder || regionMap[singletonPredecessorOrDefault] != regionMap[basicBlockBuilder])
					{
						continue;
					}
					singletonPredecessorOrDefault.MoveStatementsFrom(basicBlockBuilder);
					flag = true;
				}
				ref BasicBlockBuilder.Branch fallThrough = ref basicBlockBuilder.FallThrough;
				if (!basicBlockBuilder.HasCondition)
				{
					if (fallThrough.Destination == basicBlockBuilder)
					{
						continue;
					}
					RegionBuilder regionBuilder = regionMap[basicBlockBuilder];
					if (regionBuilder.FirstBlock == regionBuilder.LastBlock)
					{
						if (regionBuilder.Kind == ControlFlowRegionKind.Finally && fallThrough.Destination == null && fallThrough.Kind == ControlFlowBranchSemantics.StructuredExceptionHandling && !basicBlockBuilder.HasPredecessors)
						{
							RegionBuilder enclosing = regionBuilder.Enclosing;
							RegionBuilder regionBuilder2 = enclosing.Regions.First();
							if (regionBuilder2.Locals.IsEmpty && !regionBuilder2.HasLocalFunctions && !regionBuilder2.HasCaptureIds)
							{
								i = regionBuilder2.FirstBlock.Ordinal - 1;
								MergeSubRegionAndFree(regionBuilder2, blocks, regionMap);
							}
							else
							{
								regionBuilder2.Kind = ControlFlowRegionKind.LocalLifetime;
								i--;
							}
							MergeSubRegionAndFree(regionBuilder, blocks, regionMap);
							RegionBuilder enclosing2 = enclosing.Enclosing;
							MergeSubRegionAndFree(enclosing, blocks, regionMap);
							num--;
							removeBlock(basicBlockBuilder, enclosing2);
							result = true;
							flag = true;
						}
						continue;
					}
					if (fallThrough.Kind == ControlFlowBranchSemantics.StructuredExceptionHandling)
					{
						if (basicBlockBuilder.HasPredecessors)
						{
							BasicBlockBuilder singletonPredecessorOrDefault2 = basicBlockBuilder.GetSingletonPredecessorOrDefault();
							if (singletonPredecessorOrDefault2 == null || singletonPredecessorOrDefault2.Ordinal != i - 1 || singletonPredecessorOrDefault2.FallThrough.Destination != basicBlockBuilder || singletonPredecessorOrDefault2.Conditional.Destination == basicBlockBuilder || regionMap[singletonPredecessorOrDefault2] != regionBuilder)
							{
								continue;
							}
							singletonPredecessorOrDefault2.FallThrough = basicBlockBuilder.FallThrough;
						}
					}
					else
					{
						IOperation branchValue = basicBlockBuilder.BranchValue;
						if (tryGetImplicitEntryRegion(basicBlockBuilder, regionBuilder) != null && (branchValue != null || fallThrough.Destination != blocks[i + 1]))
						{
							continue;
						}
						if (branchValue != null)
						{
							if (!basicBlockBuilder.HasPredecessors && fallThrough.Kind == ControlFlowBranchSemantics.Return)
							{
								if (fallThrough.Destination.Kind != BasicBlockKind.Exit || !branchValue.IsImplicit || branchValue.Kind != OperationKind.LocalReference || !((ILocalReferenceOperation)branchValue).Local.IsFunctionValue)
								{
									continue;
								}
							}
							else
							{
								BasicBlockBuilder singletonPredecessorOrDefault3 = basicBlockBuilder.GetSingletonPredecessorOrDefault();
								if (singletonPredecessorOrDefault3 == null || singletonPredecessorOrDefault3.BranchValue != null || singletonPredecessorOrDefault3.Kind == BasicBlockKind.Entry || regionMap[singletonPredecessorOrDefault3] != regionBuilder)
								{
									continue;
								}
							}
						}
						RegionBuilder regionBuilder3 = ((fallThrough.Destination == null) ? null : regionMap[fallThrough.Destination]);
						if (basicBlockBuilder.HasPredecessors)
						{
							if (arrayBuilder == null)
							{
								arrayBuilder = ArrayBuilder<BasicBlockBuilder>.GetInstance();
							}
							else
							{
								arrayBuilder.Clear();
							}
							basicBlockBuilder.GetPredecessors(arrayBuilder);
							if (regionBuilder != regionBuilder3)
							{
								fromCurrent?.Clear();
								fromDestination?.Clear();
								if (!checkBranchesFromPredecessors(arrayBuilder, regionBuilder, regionBuilder3))
								{
									continue;
								}
							}
							foreach (BasicBlockBuilder item in arrayBuilder)
							{
								if (tryMergeBranch(item, ref item.FallThrough, basicBlockBuilder) && branchValue != null)
								{
									item.BranchValue = branchValue;
								}
								tryMergeBranch(item, ref item.Conditional, basicBlockBuilder);
							}
						}
						fallThrough.Destination?.RemovePredecessor(basicBlockBuilder);
					}
					i--;
					num--;
					removeBlock(basicBlockBuilder, regionBuilder);
					result = true;
					flag = true;
				}
				else
				{
					if (fallThrough.Kind == ControlFlowBranchSemantics.StructuredExceptionHandling)
					{
						continue;
					}
					BasicBlockBuilder singletonPredecessorOrDefault4 = basicBlockBuilder.GetSingletonPredecessorOrDefault();
					if (singletonPredecessorOrDefault4 == null)
					{
						continue;
					}
					RegionBuilder regionBuilder4 = regionMap[basicBlockBuilder];
					if (tryGetImplicitEntryRegion(basicBlockBuilder, regionBuilder4) == null && singletonPredecessorOrDefault4.Kind != BasicBlockKind.Entry && singletonPredecessorOrDefault4.FallThrough.Destination == basicBlockBuilder && !singletonPredecessorOrDefault4.HasCondition && regionMap[singletonPredecessorOrDefault4] == regionBuilder4)
					{
						mergeBranch(singletonPredecessorOrDefault4, ref singletonPredecessorOrDefault4.FallThrough, ref fallThrough);
						fallThrough.Destination?.RemovePredecessor(basicBlockBuilder);
						singletonPredecessorOrDefault4.BranchValue = basicBlockBuilder.BranchValue;
						singletonPredecessorOrDefault4.ConditionKind = basicBlockBuilder.ConditionKind;
						singletonPredecessorOrDefault4.Conditional = basicBlockBuilder.Conditional;
						BasicBlockBuilder destination = basicBlockBuilder.Conditional.Destination;
						if (destination != null)
						{
							destination.AddPredecessor(singletonPredecessorOrDefault4);
							destination.RemovePredecessor(basicBlockBuilder);
						}
						i--;
						num--;
						removeBlock(basicBlockBuilder, regionBuilder4);
						result = true;
						flag = true;
					}
				}
			}
			blocks[0].Ordinal = 0;
			blocks[num].Ordinal = num;
		}
		while (flag);
		fromCurrent?.Free();
		fromDestination?.Free();
		fromPredecessor?.Free();
		arrayBuilder?.Free();
		return result;
		bool checkBranchesFromPredecessors(ArrayBuilder<BasicBlockBuilder> predecessors, RegionBuilder currentRegion, RegionBuilder destinationRegionOpt)
		{
			foreach (BasicBlockBuilder predecessor in predecessors)
			{
				RegionBuilder regionBuilder5 = regionMap[predecessor];
				if (regionBuilder5 != currentRegion)
				{
					if (destinationRegionOpt == null)
					{
						return false;
					}
					fromPredecessor?.Clear();
					collectAncestorsAndSelf(currentRegion, ref fromCurrent);
					collectAncestorsAndSelf(destinationRegionOpt, ref fromDestination);
					collectAncestorsAndSelf(regionBuilder5, ref fromPredecessor);
					int num2 = getIndexOfLastLeftRegion(fromCurrent, fromDestination);
					int num3 = getIndexOfLastLeftRegion(fromPredecessor, fromDestination);
					int num4 = getIndexOfLastLeftRegion(fromPredecessor, fromCurrent);
					if (fromPredecessor.Count - num4 + fromCurrent.Count - num2 != fromPredecessor.Count - num3)
					{
						return false;
					}
				}
				else if (predecessor.Kind == BasicBlockKind.Entry && destinationRegionOpt == null)
				{
					return false;
				}
			}
			return true;
		}
		static void collectAncestorsAndSelf(RegionBuilder from, ref ArrayBuilder<RegionBuilder> builder)
		{
			if (builder == null)
			{
				builder = ArrayBuilder<RegionBuilder>.GetInstance();
			}
			else if (builder.Count != 0)
			{
				return;
			}
			do
			{
				builder.Add(from);
				from = from.Enclosing;
			}
			while (from != null);
			builder.ReverseContents();
		}
		static int getIndexOfLastLeftRegion(ArrayBuilder<RegionBuilder> from, ArrayBuilder<RegionBuilder> to)
		{
			int j;
			for (j = 0; j < from.Count && j < to.Count && from[j] == to[j]; j++)
			{
			}
			return j;
		}
		static void mergeBranch(BasicBlockBuilder predecessor, ref BasicBlockBuilder.Branch predecessorBranch, ref BasicBlockBuilder.Branch successorBranch)
		{
			predecessorBranch.Destination = successorBranch.Destination;
			successorBranch.Destination?.AddPredecessor(predecessor);
			predecessorBranch.Kind = successorBranch.Kind;
		}
		void removeBlock(BasicBlockBuilder block, RegionBuilder region)
		{
			if (region.FirstBlock == block)
			{
				BasicBlockBuilder firstBlock = (region.FirstBlock = blocks[block.Ordinal + 1]);
				RegionBuilder enclosing3 = region.Enclosing;
				while (enclosing3 != null && enclosing3.FirstBlock == block)
				{
					enclosing3.FirstBlock = firstBlock;
					enclosing3 = enclosing3.Enclosing;
				}
			}
			else if (region.LastBlock == block)
			{
				BasicBlockBuilder lastBlock = (region.LastBlock = blocks[block.Ordinal - 1]);
				RegionBuilder enclosing4 = region.Enclosing;
				while (enclosing4 != null && enclosing4.LastBlock == block)
				{
					enclosing4.LastBlock = lastBlock;
					enclosing4 = enclosing4.Enclosing;
				}
			}
			regionMap.Remove(block);
			blocks.RemoveAt(block.Ordinal);
			block.Free();
		}
		static RegionBuilder tryGetImplicitEntryRegion(BasicBlockBuilder block, RegionBuilder currentRegion)
		{
			do
			{
				if (currentRegion.FirstBlock != block)
				{
					return null;
				}
				ControlFlowRegionKind kind = currentRegion.Kind;
				if ((uint)(kind - 3) <= 1u || kind == ControlFlowRegionKind.Finally)
				{
					return currentRegion;
				}
				currentRegion = currentRegion.Enclosing;
			}
			while (currentRegion != null);
			return null;
		}
		static bool tryMergeBranch(BasicBlockBuilder predecessor, ref BasicBlockBuilder.Branch predecessorBranch, BasicBlockBuilder successor)
		{
			if (predecessorBranch.Destination == successor)
			{
				mergeBranch(predecessor, ref predecessorBranch, ref successor.FallThrough);
				return true;
			}
			return false;
		}
	}

	private static void CheckUnresolvedBranches(ArrayBuilder<BasicBlockBuilder> blocks, PooledDictionary<ILabelSymbol, BasicBlockBuilder> labeledBlocks)
	{
		if (labeledBlocks == null)
		{
			return;
		}
		PooledHashSet<BasicBlockBuilder> unresolved = null;
		foreach (BasicBlockBuilder value in labeledBlocks.Values)
		{
			if (value.Ordinal == -1)
			{
				if (unresolved == null)
				{
					unresolved = PooledHashSet<BasicBlockBuilder>.GetInstance();
				}
				unresolved.Add(value);
			}
		}
		if (unresolved == null)
		{
			return;
		}
		foreach (BasicBlockBuilder block in blocks)
		{
			fixupBranch(ref block.Conditional);
			fixupBranch(ref block.FallThrough);
		}
		unresolved.Free();
		void fixupBranch(ref BasicBlockBuilder.Branch branch)
		{
			if (branch.Destination != null && unresolved.Contains(branch.Destination))
			{
				branch.Destination = null;
				branch.Kind = ControlFlowBranchSemantics.Error;
			}
		}
	}

	private void VisitStatement(IOperation operation)
	{
		if (operation != null)
		{
			IOperation currentStatement = _currentStatement;
			_currentStatement = operation;
			EvalStackFrame frame = PushStackFrame();
			AddStatement(base.Visit(operation, null));
			PopStackFrameAndLeaveRegion(frame);
			_currentStatement = currentStatement;
		}
	}

	private void AddStatement(IOperation statement)
	{
		if (statement != null)
		{
			Operation.SetParentOperation(statement, null);
			CurrentBasicBlock.AddStatement(statement);
		}
	}

	private void AppendNewBlock(BasicBlockBuilder block, bool linkToPrevious = true)
	{
		if (linkToPrevious)
		{
			BasicBlockBuilder basicBlockBuilder = _blocks.Last();
			if (basicBlockBuilder.FallThrough.Destination == null)
			{
				LinkBlocks(basicBlockBuilder, block);
			}
		}
		if (block.Ordinal != -1)
		{
			throw ExceptionUtilities.Unreachable;
		}
		block.Ordinal = _blocks.Count;
		_blocks.Add(block);
		_currentBasicBlock = block;
		_currentRegion.ExtendToInclude(block);
		_regionMap.Add(block, _currentRegion);
	}

	private void EnterRegion(RegionBuilder region, bool spillingStack = false)
	{
		if (!spillingStack)
		{
			SpillEvalStack();
		}
		_currentRegion?.Add(region);
		_currentRegion = region;
		_currentBasicBlock = null;
	}

	private void LeaveRegion()
	{
		if (_currentRegion.IsEmpty)
		{
			AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
		}
		RegionBuilder currentRegion = _currentRegion;
		_currentRegion = _currentRegion.Enclosing;
		_currentRegion?.ExtendToInclude(currentRegion.LastBlock);
		_currentBasicBlock = null;
	}

	private static void LinkBlocks(BasicBlockBuilder prevBlock, BasicBlockBuilder nextBlock, ControlFlowBranchSemantics branchKind = ControlFlowBranchSemantics.Regular)
	{
		prevBlock.FallThrough.Destination = nextBlock;
		prevBlock.FallThrough.Kind = branchKind;
		nextBlock.AddPredecessor(prevBlock);
	}

	public override IOperation VisitBlock(IBlockOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals));
		VisitStatements(operation.Operations);
		LeaveRegion();
		return FinishVisitingStatement(operation);
	}

	private void StartVisitingStatement(IOperation operation)
	{
		SpillEvalStack();
	}

	private IOperation FinishVisitingStatement(IOperation originalOperation, IOperation result = null)
	{
		if (_currentStatement == originalOperation)
		{
			return result;
		}
		return result ?? MakeInvalidOperation(originalOperation.Syntax, originalOperation.Type, ImmutableArray<IOperation>.Empty);
	}

	private void VisitStatements(ArrayBuilder<IOperation> statements)
	{
		foreach (IOperation statement in statements)
		{
			VisitStatement(statement);
		}
	}

	private void VisitStatements(ImmutableArray<IOperation> statements)
	{
		foreach (IOperation item in statements)
		{
			VisitStatement(item);
		}
	}

	private void VisitStatements(IEnumerable<IOperation> statements)
	{
		foreach (IOperation statement in statements)
		{
			VisitStatement(statement);
		}
	}

	internal override IOperation VisitWith(IWithOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		ImplicitInstanceInfo currentImplicitInstance = _currentImplicitInstance;
		_currentImplicitInstance = new ImplicitInstanceInfo(VisitAndCapture(operation.Value));
		VisitStatement(operation.Body);
		_currentImplicitInstance = currentImplicitInstance;
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitConstructorBodyOperation(IConstructorBodyOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals));
		if (operation.Initializer != null)
		{
			VisitStatement(operation.Initializer);
		}
		VisitMethodBodyBaseOperation(operation);
		LeaveRegion();
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitMethodBodyOperation(IMethodBodyOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		VisitMethodBodyBaseOperation(operation);
		return FinishVisitingStatement(operation);
	}

	private void VisitMethodBodyBaseOperation(IMethodBodyBaseOperation operation)
	{
		VisitMethodBodies(operation.BlockBody, operation.ExpressionBody);
	}

	private void VisitMethodBodies(IBlockOperation blockBody, IBlockOperation expressionBody)
	{
		if (blockBody != null)
		{
			VisitStatement(blockBody);
			if (expressionBody != null)
			{
				LinkBlocks(CurrentBasicBlock, _exit);
				_currentBasicBlock = null;
				EnterRegion(new RegionBuilder(ControlFlowRegionKind.ErroneousBody));
				VisitStatement(expressionBody);
				LeaveRegion();
			}
		}
		else if (expressionBody != null)
		{
			VisitStatement(expressionBody);
		}
	}

	public override IOperation VisitConditional(IConditionalOperation operation, int? captureIdForResult)
	{
		if (operation == _currentStatement)
		{
			if (operation.WhenFalse == null)
			{
				BasicBlockBuilder dest = null;
				VisitConditionalBranch(operation.Condition, ref dest, sense: false);
				VisitStatement(operation.WhenTrue);
				AppendNewBlock(dest);
			}
			else
			{
				BasicBlockBuilder dest2 = null;
				VisitConditionalBranch(operation.Condition, ref dest2, sense: false);
				VisitStatement(operation.WhenTrue);
				BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
				LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
				_currentBasicBlock = null;
				AppendNewBlock(dest2);
				VisitStatement(operation.WhenFalse);
				AppendNewBlock(basicBlockBuilder);
			}
			return null;
		}
		SpillEvalStack();
		BasicBlockBuilder dest3 = null;
		VisitConditionalBranch(operation.Condition, ref dest3, sense: false);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation result;
		if (operation.WhenTrue is IConversionOperation conversionOperation && conversionOperation.Operand.Kind == OperationKind.Throw)
		{
			base.Visit(conversionOperation.Operand, null);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
			_currentBasicBlock = null;
			AppendNewBlock(dest3);
			result = Visit(operation.WhenFalse);
		}
		else if (operation.WhenFalse is IConversionOperation conversionOperation2 && conversionOperation2.Operand.Kind == OperationKind.Throw)
		{
			result = Visit(operation.WhenTrue);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
			_currentBasicBlock = null;
			AppendNewBlock(dest3);
			base.Visit(conversionOperation2.Operand, null);
		}
		else
		{
			RegionBuilder regionBuilder = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, default, isStackSpillRegion: true);
			EnterRegion(regionBuilder);
			int num = captureIdForResult ?? GetNextCaptureId(regionBuilder);
			VisitAndCapture(operation.WhenTrue, num);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
			_currentBasicBlock = null;
			AppendNewBlock(dest3);
			VisitAndCapture(operation.WhenFalse, num);
			result = GetCaptureReference(num, operation);
		}
		AppendNewBlock(basicBlockBuilder2);
		return result;
	}

	private void VisitAndCapture(IOperation operation, int captureId)
	{
		EvalStackFrame frame = PushStackFrame();
		IOperation result = base.Visit(operation, captureId);
		PopStackFrame(frame);
		CaptureResultIfNotAlready(operation.Syntax, captureId, result);
		LeaveRegionIfAny(frame);
	}

	private IOperation VisitAndCapture(IOperation operation)
	{
		EvalStackFrame frame = PushStackFrame();
		PushOperand(base.Visit(operation, null));
		SpillEvalStack();
		return PopStackFrame(frame, PopOperand());
	}

	private void CaptureResultIfNotAlready(SyntaxNode syntax, int captureId, IOperation result)
	{
		if (result.Kind != OperationKind.FlowCaptureReference || captureId != ((IFlowCaptureReferenceOperation)result).Id.Value)
		{
			SpillEvalStack();
			AddStatement(new FlowCapture(captureId, syntax, result));
		}
	}

	private EvalStackFrame PushStackFrame()
	{
		EvalStackFrame evalStackFrame = new EvalStackFrame();
		_evalStack.Push((evalStackFrame, null));
		return evalStackFrame;
	}

	private void PopStackFrame(EvalStackFrame frame, bool mergeNestedRegions = true)
	{
		int count = _evalStack.Count;
		if (_startSpillingAt == count)
		{
			_startSpillingAt--;
		}
		_evalStack.Pop();
		if ((frame.RegionBuilderOpt != null) & mergeNestedRegions)
		{
			while (_currentRegion != frame.RegionBuilderOpt)
			{
				RegionBuilder currentRegion = _currentRegion;
				_currentRegion = currentRegion.Enclosing;
				_currentRegion.AddCaptureIds(currentRegion.CaptureIds);
				_currentRegion.ExtendToInclude(currentRegion.LastBlock);
				MergeSubRegionAndFree(currentRegion, _blocks, _regionMap);
			}
		}
	}

	private void PopStackFrameAndLeaveRegion(EvalStackFrame frame)
	{
		PopStackFrame(frame);
		LeaveRegionIfAny(frame);
	}

	private void LeaveRegionIfAny(EvalStackFrame frame)
	{
		RegionBuilder regionBuilderOpt = frame.RegionBuilderOpt;
		if (regionBuilderOpt != null)
		{
			while (_currentRegion != regionBuilderOpt)
			{
				LeaveRegion();
			}
			LeaveRegion();
		}
	}

	private T PopStackFrame<T>(EvalStackFrame frame, T value)
	{
		PopStackFrame(frame);
		return value;
	}

	private T PopStackFrameAndLeaveRegion<T>(EvalStackFrame frame, T value)
	{
		PopStackFrameAndLeaveRegion(frame);
		return value;
	}

	private void LeaveRegionsUpTo(RegionBuilder resultCaptureRegion)
	{
		while (_currentRegion != resultCaptureRegion)
		{
			LeaveRegion();
		}
	}

	private int GetNextCaptureId(RegionBuilder owner)
	{
		int nextId = _captureIdDispenser.GetNextId();
		owner.AddCaptureId(nextId);
		return nextId;
	}

	private void SpillEvalStack()
	{
		int num = -1;
		for (int num2 = _startSpillingAt - 1; num2 >= 0; num2--)
		{
			if (_evalStack[num2].frameOpt != null)
			{
				num = num2;
				break;
			}
		}
		for (int i = _startSpillingAt; i < _evalStack.Count; i++)
		{
			var (evalStackFrame, operation) = _evalStack[i];
			if (evalStackFrame != null)
			{
				num = i;
				evalStackFrame.RegionBuilderOpt = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, default, isStackSpillRegion: true);
				EnterRegion(evalStackFrame.RegionBuilderOpt, spillingStack: true);
			}
			else
			{
				if (operation.Kind == OperationKind.FlowCaptureReference || operation.Kind == OperationKind.DeclarationExpression || operation.Kind == OperationKind.Discard || operation.Kind == OperationKind.OmittedArgument)
				{
					continue;
				}
				RegionBuilder regionBuilder = _evalStack[num].frameOpt.RegionBuilderOpt;
				if (_currentRegion != regionBuilder)
				{
					PooledHashSet<CaptureId> instance = PooledHashSet<CaptureId>.GetInstance();
					for (int j = num + 1; j < _evalStack.Count; j++)
					{
						IOperation item = _evalStack[j].operationOpt;
						if (item == null)
						{
							continue;
						}
						if (j < i)
						{
							if (item is IFlowCaptureReferenceOperation flowCaptureReferenceOperation)
							{
								instance.Add(flowCaptureReferenceOperation.Id);
							}
						}
						else
						{
							if (j <= i)
							{
								continue;
							}
							foreach (IFlowCaptureReferenceOperation item2 in item.DescendantsAndSelf().OfType<IFlowCaptureReferenceOperation>())
							{
								instance.Add(item2.Id);
							}
						}
					}
					RegionBuilder regionBuilder2 = _currentRegion;
					do
					{
						if (regionBuilder2.HasCaptureIds && regionBuilder2.CaptureIds.Any((CaptureId id, PooledHashSet<CaptureId> set) => set.Contains(id), instance))
						{
							regionBuilder = regionBuilder2;
							break;
						}
						regionBuilder2 = regionBuilder2.Enclosing;
					}
					while (regionBuilder2 != regionBuilder);
					instance.Free();
				}
				int nextCaptureId = GetNextCaptureId(regionBuilder);
				AddStatement(new FlowCapture(nextCaptureId, operation.Syntax, operation));
				_evalStack[i] = (null, GetCaptureReference(nextCaptureId, operation));
				while (_currentRegion != regionBuilder)
				{
					LeaveRegion();
				}
			}
		}
		_startSpillingAt = _evalStack.Count;
	}

	private void PushOperand(IOperation operation)
	{
		_evalStack.Push((null, operation));
	}

	private IOperation PopOperand()
	{
		int count = _evalStack.Count;
		if (_startSpillingAt == count)
		{
			_startSpillingAt--;
		}
		return _evalStack.Pop().operationOpt;
	}

	private IOperation PeekOperand()
	{
		return _evalStack.Peek().operationOpt;
	}

	private void VisitAndPushArray<T>(ImmutableArray<T> array, Func<T, IOperation> unwrapper = null) where T : IOperation
	{
		foreach (T item in array)
		{
			IOperation operation;
			if (unwrapper != null)
			{
				operation = unwrapper(item);
			}
			else
			{
				IOperation operation2 = item;
				operation = operation2;
			}
			PushOperand(Visit(operation));
		}
	}

	private ImmutableArray<T> PopArray<T>(ImmutableArray<T> originalArray, Func<IOperation, int, ImmutableArray<T>, T> wrapper = null) where T : IOperation
	{
		int length = originalArray.Length;
		if (length == 0)
		{
			return ImmutableArray<T>.Empty;
		}
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance(length);
		for (int num = length - 1; num >= 0; num--)
		{
			IOperation operation = PopOperand();
			instance.Add((wrapper != null) ? wrapper(operation, num, originalArray) : ((T)operation));
		}
		instance.ReverseContents();
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<T> VisitArray<T>(ImmutableArray<T> originalArray, Func<T, IOperation> unwrapper = null, Func<IOperation, int, ImmutableArray<T>, T> wrapper = null) where T : IOperation
	{
		VisitAndPushArray(originalArray, unwrapper);
		return PopArray(originalArray, wrapper);
	}

	private ImmutableArray<IArgumentOperation> VisitArguments(ImmutableArray<IArgumentOperation> arguments)
	{
		return VisitArray(arguments, UnwrapArgument, RewriteArgumentFromArray);
	}

	private static IOperation UnwrapArgument(IArgumentOperation argument)
	{
		return argument.Value;
	}

	private IArgumentOperation RewriteArgumentFromArray(IOperation visitedArgument, int index, ImmutableArray<IArgumentOperation> args)
	{
		BaseArgument baseArgument = (BaseArgument)args[index];
		return new ArgumentOperation(visitedArgument, baseArgument.ArgumentKind, baseArgument.Parameter, baseArgument.InConversionConvertibleOpt, baseArgument.OutConversionConvertibleOpt, null, baseArgument.Syntax, IsImplicit(baseArgument));
	}

	public override IOperation VisitSimpleAssignment(ISimpleAssignmentOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		PushOperand(Visit(operation.Target));
		IOperation value = Visit(operation.Value);
		return PopStackFrame(frame, new SimpleAssignmentExpression(PopOperand(), operation.IsRef, value, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
	}

	public override IOperation VisitCompoundAssignment(ICompoundAssignmentOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		BaseCompoundAssignmentExpression baseCompoundAssignmentExpression = (BaseCompoundAssignmentExpression)operation;
		PushOperand(Visit(baseCompoundAssignmentExpression.Target));
		IOperation value = Visit(baseCompoundAssignmentExpression.Value);
		return PopStackFrame(frame, new CompoundAssignmentOperation(PopOperand(), value, baseCompoundAssignmentExpression.InConversionConvertible, baseCompoundAssignmentExpression.OutConversionConvertible, operation.OperatorKind, operation.IsLifted, operation.IsChecked, operation.OperatorMethod, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
	}

	public override IOperation VisitArrayElementReference(IArrayElementReferenceOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		PushOperand(Visit(operation.ArrayReference));
		ImmutableArray<IOperation> indices = VisitArray(operation.Indices);
		IOperation arrayReference = PopOperand();
		PopStackFrame(frame);
		return new ArrayElementReferenceExpression(arrayReference, indices, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	private static bool IsConditional(IBinaryOperation operation)
	{
		BinaryOperatorKind operatorKind = operation.OperatorKind;
		if ((uint)(operatorKind - 13) <= 1u)
		{
			return true;
		}
		return false;
	}

	public override IOperation VisitBinaryOperator(IBinaryOperation operation, int? captureIdForResult)
	{
		if (IsConditional(operation))
		{
			if (operation.OperatorMethod != null)
			{
				return VisitUserDefinedBinaryConditionalOperator(operation, captureIdForResult);
			}
			if (ITypeSymbolHelpers.IsBooleanType(operation.Type) && ITypeSymbolHelpers.IsBooleanType(operation.LeftOperand.Type) && ITypeSymbolHelpers.IsBooleanType(operation.RightOperand.Type))
			{
				return VisitBinaryConditionalOperator(operation, sense: true, captureIdForResult, null, null);
			}
			if (operation.IsLifted && ITypeSymbolHelpers.IsNullableOfBoolean(operation.Type) && ITypeSymbolHelpers.IsNullableOfBoolean(operation.LeftOperand.Type) && ITypeSymbolHelpers.IsNullableOfBoolean(operation.RightOperand.Type))
			{
				return VisitNullableBinaryConditionalOperator(operation, captureIdForResult);
			}
			if (ITypeSymbolHelpers.IsObjectType(operation.Type) && ITypeSymbolHelpers.IsObjectType(operation.LeftOperand.Type) && ITypeSymbolHelpers.IsObjectType(operation.RightOperand.Type))
			{
				return VisitObjectBinaryConditionalOperator(operation, captureIdForResult);
			}
			if (ITypeSymbolHelpers.IsDynamicType(operation.Type) && (ITypeSymbolHelpers.IsDynamicType(operation.LeftOperand.Type) || ITypeSymbolHelpers.IsDynamicType(operation.RightOperand.Type)))
			{
				return VisitDynamicBinaryConditionalOperator(operation, captureIdForResult);
			}
		}
		EvalStackFrame frame = PushStackFrame();
		PushOperand(Visit(operation.LeftOperand));
		IOperation rightOperand = Visit(operation.RightOperand);
		return PopStackFrame(frame, new BinaryOperatorExpression(operation.OperatorKind, PopOperand(), rightOperand, operation.IsLifted, operation.IsChecked, operation.IsCompareText, operation.OperatorMethod, ((BaseBinaryOperatorExpression)operation).UnaryOperatorMethod, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
	}

	public override IOperation VisitTupleBinaryOperator(ITupleBinaryOperation operation, int? captureIdForResult)
	{
		var (leftOperand, rightOperand) = VisitPreservingTupleOperations(operation.LeftOperand, operation.RightOperand);
		return new TupleBinaryOperatorExpression(operation.OperatorKind, leftOperand, rightOperand, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitUnaryOperator(IUnaryOperation operation, int? captureIdForResult)
	{
		if (IsBooleanLogicalNot(operation))
		{
			return VisitConditionalExpression(operation.Operand, sense: false, captureIdForResult, null, null);
		}
		return new UnaryOperatorExpression(operation.OperatorKind, Visit(operation.Operand), operation.IsLifted, operation.IsChecked, operation.OperatorMethod, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	private static bool IsBooleanLogicalNot(IUnaryOperation operation)
	{
		if (operation.OperatorKind == UnaryOperatorKind.Not && operation.OperatorMethod == null && ITypeSymbolHelpers.IsBooleanType(operation.Type))
		{
			return ITypeSymbolHelpers.IsBooleanType(operation.Operand.Type);
		}
		return false;
	}

	private static bool CalculateAndOrSense(IBinaryOperation binOp, bool sense)
	{
		return binOp.OperatorKind switch
		{
			BinaryOperatorKind.ConditionalOr => !sense, 
			BinaryOperatorKind.ConditionalAnd => sense, 
			_ => throw ExceptionUtilities.UnexpectedValue(binOp.OperatorKind), 
		};
	}

	private IOperation VisitBinaryConditionalOperator(IBinaryOperation binOp, bool sense, int? captureIdForResult, BasicBlockBuilder fallToTrueOpt, BasicBlockBuilder fallToFalseOpt)
	{
		if (!CalculateAndOrSense(binOp, sense))
		{
			return VisitShortCircuitingOperator(binOp, sense, sense, stopValue: true, captureIdForResult, fallToTrueOpt, fallToFalseOpt);
		}
		return VisitShortCircuitingOperator(binOp, sense, !sense, stopValue: false, captureIdForResult, fallToTrueOpt, fallToFalseOpt);
	}

	private IOperation VisitNullableBinaryConditionalOperator(IBinaryOperation binOp, int? captureIdForResult)
	{
		SpillEvalStack();
		IOperation leftOperand = binOp.LeftOperand;
		IOperation rightOperand = binOp.RightOperand;
		bool flag = CalculateAndOrSense(binOp, sense: true);
		RegionBuilder currentRegion = _currentRegion;
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder basicBlockBuilder3 = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation operation = VisitAndCapture(leftOperand);
		IOperation operation2 = operation;
		if (flag)
		{
			operation2 = negateNullable(operation2);
		}
		LinkBlocks(condition: Operation.SetParentOperation(UnwrapNullableValue(operation2), null), previous: CurrentBasicBlock, jumpIfTrue: true, branch: RegularBranch(basicBlockBuilder3));
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
		_currentBasicBlock = null;
		int id = captureIdForResult ?? GetNextCaptureId(currentRegion);
		AppendNewBlock(basicBlockBuilder2);
		EvalStackFrame frame = PushStackFrame();
		IOperation operation3 = VisitAndCapture(rightOperand);
		operation2 = operation3;
		if (!flag)
		{
			operation2 = negateNullable(operation2);
		}
		operation2 = UnwrapNullableValue(operation2);
		LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation2, null), jumpIfTrue: true, RegularBranch(basicBlockBuilder3));
		_currentBasicBlock = null;
		AddStatement(new FlowCapture(id, binOp.Syntax, OperationCloner.CloneOperation(operation3)));
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		_currentBasicBlock = null;
		PopStackFrameAndLeaveRegion(frame);
		AppendNewBlock(basicBlockBuilder3);
		AddStatement(new FlowCapture(id, binOp.Syntax, OperationCloner.CloneOperation(operation)));
		LeaveRegionsUpTo(currentRegion);
		AppendNewBlock(basicBlockBuilder);
		return GetCaptureReference(id, binOp);
		static IOperation negateNullable(IOperation operand)
		{
			return new UnaryOperatorExpression(UnaryOperatorKind.Not, operand, isLifted: true, isChecked: false, null, null, operand.Syntax, operand.Type, default, isImplicit: true);
		}
	}

	private IOperation VisitObjectBinaryConditionalOperator(IBinaryOperation binOp, int? captureIdForResult)
	{
		SpillEvalStack();
		INamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		IOperation leftOperand = binOp.LeftOperand;
		IOperation rightOperand = binOp.RightOperand;
		bool flag = CalculateAndOrSense(binOp, sense: true);
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		EvalStackFrame frame = PushStackFrame();
		IOperation operation = CreateConversion(Visit(leftOperand), specialType);
		LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation, null), flag, RegularBranch(basicBlockBuilder2));
		_currentBasicBlock = null;
		PopStackFrameAndLeaveRegion(frame);
		RegionBuilder currentRegion = _currentRegion;
		int nextCaptureId = GetNextCaptureId(currentRegion);
		AddStatement(new FlowCapture(nextCaptureId, binOp.Syntax, new LiteralExpression(null, leftOperand.Syntax, specialType, !flag, isImplicit: true)));
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		_currentBasicBlock = null;
		AppendNewBlock(basicBlockBuilder2);
		frame = PushStackFrame();
		operation = CreateConversion(Visit(rightOperand), specialType);
		AddStatement(new FlowCapture(nextCaptureId, binOp.Syntax, operation));
		PopStackFrame(frame);
		LeaveRegionsUpTo(currentRegion);
		AppendNewBlock(basicBlockBuilder);
		SyntaxNode syntax = binOp.Syntax;
		Optional<object> constantValue = default;
		operation = new FlowCaptureReference(nextCaptureId, syntax, specialType, constantValue);
		return new ConversionOperation(operation, _compilation.ClassifyConvertibleConversion(operation, binOp.Type, out constantValue), isTryCast: false, isChecked: false, null, binOp.Syntax, binOp.Type, binOp.ConstantValue, isImplicit: true);
	}

	private IOperation CreateConversion(IOperation operand, ITypeSymbol type)
	{
		Optional<object> constantValue;
		return new ConversionOperation(operand, _compilation.ClassifyConvertibleConversion(operand, type, out constantValue), isTryCast: false, isChecked: false, null, operand.Syntax, type, constantValue, isImplicit: true);
	}

	private IOperation VisitDynamicBinaryConditionalOperator(IBinaryOperation binOp, int? captureIdForResult)
	{
		SpillEvalStack();
		RegionBuilder currentRegion = _currentRegion;
		INamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		IOperation leftOperand = binOp.LeftOperand;
		IOperation rightOperand = binOp.RightOperand;
		IMethodSymbol unaryOperatorMethod = ((BaseBinaryOperatorExpression)binOp).UnaryOperatorMethod;
		bool flag = CalculateAndOrSense(binOp, sense: true);
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation operation = VisitAndCapture(leftOperand);
		IOperation operation2 = operation;
		bool jumpIfTrue;
		if (ITypeSymbolHelpers.IsBooleanType(leftOperand.Type))
		{
			jumpIfTrue = flag;
		}
		else if (ITypeSymbolHelpers.IsDynamicType(leftOperand.Type) || unaryOperatorMethod != null)
		{
			jumpIfTrue = false;
			operation2 = ((unaryOperatorMethod != null && (!ITypeSymbolHelpers.IsBooleanType(unaryOperatorMethod.ReturnType) || (!ITypeSymbolHelpers.IsNullableType(leftOperand.Type) && ITypeSymbolHelpers.IsNullableType(unaryOperatorMethod.Parameters[0].Type)))) ? MakeInvalidOperation(specialType, operation2) : new UnaryOperatorExpression(flag ? UnaryOperatorKind.False : UnaryOperatorKind.True, operation2, isLifted: false, isChecked: false, unaryOperatorMethod, null, operation2.Syntax, specialType, default, isImplicit: true));
		}
		else
		{
			operation2 = CreateConversion(operation2, specialType);
			jumpIfTrue = flag;
		}
		LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation2, null), jumpIfTrue, RegularBranch(basicBlockBuilder2));
		_currentBasicBlock = null;
		int id = captureIdForResult ?? GetNextCaptureId(currentRegion);
		IOperation operation3 = OperationCloner.CloneOperation(operation);
		if (!ITypeSymbolHelpers.IsDynamicType(leftOperand.Type))
		{
			operation3 = CreateConversion(operation3, binOp.Type);
		}
		AddStatement(new FlowCapture(id, binOp.Syntax, operation3));
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		_currentBasicBlock = null;
		AppendNewBlock(basicBlockBuilder2);
		EvalStackFrame frame = PushStackFrame();
		PushOperand(OperationCloner.CloneOperation(operation));
		IOperation rightOperand2 = Visit(rightOperand);
		AddStatement(new FlowCapture(id, binOp.Syntax, new BinaryOperatorExpression(flag ? BinaryOperatorKind.And : BinaryOperatorKind.Or, PopOperand(), rightOperand2, isLifted: false, binOp.IsChecked, binOp.IsCompareText, binOp.OperatorMethod, null, null, binOp.Syntax, binOp.Type, binOp.ConstantValue, IsImplicit(binOp))));
		PopStackFrameAndLeaveRegion(frame);
		LeaveRegionsUpTo(currentRegion);
		AppendNewBlock(basicBlockBuilder);
		return GetCaptureReference(id, binOp);
	}

	private IOperation VisitUserDefinedBinaryConditionalOperator(IBinaryOperation binOp, int? captureIdForResult)
	{
		SpillEvalStack();
		RegionBuilder currentRegion = _currentRegion;
		INamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		bool isLifted = binOp.IsLifted;
		IOperation leftOperand = binOp.LeftOperand;
		IOperation rightOperand = binOp.RightOperand;
		IMethodSymbol unaryOperatorMethod = ((BaseBinaryOperatorExpression)binOp).UnaryOperatorMethod;
		bool flag = CalculateAndOrSense(binOp, sense: true);
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation operation = VisitAndCapture(leftOperand);
		IOperation operation2 = operation;
		if (ITypeSymbolHelpers.IsNullableType(leftOperand.Type))
		{
			if ((unaryOperatorMethod == null) ? isLifted : (!ITypeSymbolHelpers.IsNullableType(unaryOperatorMethod.Parameters[0].Type)))
			{
				operation2 = MakeIsNullOperation(operation2, specialType);
				LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation2, null), jumpIfTrue: true, RegularBranch(basicBlockBuilder2));
				_currentBasicBlock = null;
				operation2 = UnwrapNullableValue(OperationCloner.CloneOperation(operation));
			}
		}
		else if (unaryOperatorMethod != null && ITypeSymbolHelpers.IsNullableType(unaryOperatorMethod.Parameters[0].Type))
		{
			operation2 = MakeInvalidOperation(unaryOperatorMethod.Parameters[0].Type, operation2);
		}
		LinkBlocks(condition: Operation.SetParentOperation((unaryOperatorMethod == null || !ITypeSymbolHelpers.IsBooleanType(unaryOperatorMethod.ReturnType)) ? MakeInvalidOperation(specialType, operation2) : new UnaryOperatorExpression(flag ? UnaryOperatorKind.False : UnaryOperatorKind.True, operation2, isLifted: false, isChecked: false, unaryOperatorMethod, null, operation2.Syntax, unaryOperatorMethod.ReturnType, default, isImplicit: true), null), previous: CurrentBasicBlock, jumpIfTrue: false, branch: RegularBranch(basicBlockBuilder2));
		_currentBasicBlock = null;
		int id = captureIdForResult ?? GetNextCaptureId(currentRegion);
		AddStatement(new FlowCapture(id, binOp.Syntax, OperationCloner.CloneOperation(operation)));
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		_currentBasicBlock = null;
		AppendNewBlock(basicBlockBuilder2);
		EvalStackFrame frame = PushStackFrame();
		PushOperand(OperationCloner.CloneOperation(operation));
		IOperation rightOperand2 = Visit(rightOperand);
		AddStatement(new FlowCapture(id, binOp.Syntax, new BinaryOperatorExpression(flag ? BinaryOperatorKind.And : BinaryOperatorKind.Or, PopOperand(), rightOperand2, isLifted, binOp.IsChecked, binOp.IsCompareText, binOp.OperatorMethod, null, null, binOp.Syntax, binOp.Type, binOp.ConstantValue, IsImplicit(binOp))));
		PopStackFrameAndLeaveRegion(frame);
		LeaveRegionsUpTo(currentRegion);
		AppendNewBlock(basicBlockBuilder);
		return GetCaptureReference(id, binOp);
	}

	private IOperation VisitShortCircuitingOperator(IBinaryOperation condition, bool sense, bool stopSense, bool stopValue, int? captureIdForResult, BasicBlockBuilder fallToTrueOpt, BasicBlockBuilder fallToFalseOpt)
	{
		SpillEvalStack();
		ref BasicBlockBuilder reference = ref stopValue ? ref fallToTrueOpt : ref fallToFalseOpt;
		bool flag = reference == null;
		VisitConditionalBranch(condition.LeftOperand, ref reference, stopSense);
		RegionBuilder currentRegion = _currentRegion;
		int num = captureIdForResult ?? GetNextCaptureId(currentRegion);
		CaptureResultIfNotAlready(result: VisitConditionalExpression(condition.RightOperand, sense, num, fallToTrueOpt, fallToFalseOpt), syntax: condition.RightOperand.Syntax, captureId: num);
		LeaveRegionsUpTo(currentRegion);
		if (flag)
		{
			BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
			_currentBasicBlock = null;
			AppendNewBlock(reference);
			Optional<object> constantValue = new Optional<object>(stopValue);
			IOperation operation;
			if (reference.GetSingletonPredecessorOrDefault() == null)
			{
				operation = condition;
			}
			else
			{
				operation = condition.LeftOperand;
			}
			SyntaxNode syntax = operation.Syntax;
			AddStatement(new FlowCapture(num, syntax, new LiteralExpression(null, syntax, condition.Type, constantValue, isImplicit: true)));
			AppendNewBlock(basicBlockBuilder);
		}
		return GetCaptureReference(num, condition);
	}

	private IOperation VisitConditionalExpression(IOperation condition, bool sense, int? captureIdForResult, BasicBlockBuilder fallToTrueOpt, BasicBlockBuilder fallToFalseOpt)
	{
		IUnaryOperation unaryOperation = null;
		while (true)
		{
			IOperation operation = condition;
			if (operation == null)
			{
				break;
			}
			if (!(operation is IParenthesizedOperation parenthesizedOperation))
			{
				if (!(operation is IUnaryOperation unaryOperation2))
				{
					break;
				}
				IUnaryOperation unaryOperation3 = unaryOperation2;
				if (!IsBooleanLogicalNot(unaryOperation3))
				{
					break;
				}
				unaryOperation = unaryOperation3;
				condition = unaryOperation3.Operand;
				sense = !sense;
			}
			else
			{
				condition = parenthesizedOperation.Operand;
			}
		}
		if (condition.Kind == OperationKind.BinaryOperator)
		{
			IBinaryOperation binOp = (IBinaryOperation)condition;
			if (IsBooleanConditionalOperator(binOp))
			{
				return VisitBinaryConditionalOperator(binOp, sense, captureIdForResult, fallToTrueOpt, fallToFalseOpt);
			}
		}
		condition = Visit(condition);
		if (!sense)
		{
			if (unaryOperation == null)
			{
				return new UnaryOperatorExpression(UnaryOperatorKind.Not, condition, isLifted: false, isChecked: false, null, null, condition.Syntax, condition.Type, default, isImplicit: true);
			}
			return new UnaryOperatorExpression(unaryOperation.OperatorKind, condition, unaryOperation.IsLifted, unaryOperation.IsChecked, unaryOperation.OperatorMethod, null, unaryOperation.Syntax, unaryOperation.Type, unaryOperation.ConstantValue, IsImplicit(unaryOperation));
		}
		return condition;
	}

	private static bool IsBooleanConditionalOperator(IBinaryOperation binOp)
	{
		if (IsConditional(binOp) && binOp.OperatorMethod == null && ITypeSymbolHelpers.IsBooleanType(binOp.Type) && ITypeSymbolHelpers.IsBooleanType(binOp.LeftOperand.Type))
		{
			return ITypeSymbolHelpers.IsBooleanType(binOp.RightOperand.Type);
		}
		return false;
	}

	private void VisitConditionalBranch(IOperation condition, ref BasicBlockBuilder dest, bool sense)
	{
		SpillEvalStack();
		VisitConditionalBranchCore(condition, ref dest, sense);
	}

	private void VisitConditionalBranchCore(IOperation condition, ref BasicBlockBuilder dest, bool sense)
	{
		while (true)
		{
			if (condition.Kind == OperationKind.Parenthesized)
			{
				condition = ((IParenthesizedOperation)condition).Operand;
				continue;
			}
			switch (condition.Kind)
			{
			case OperationKind.BinaryOperator:
			{
				IBinaryOperation binaryOperation = (IBinaryOperation)condition;
				if (IsBooleanConditionalOperator(binaryOperation))
				{
					if (CalculateAndOrSense(binaryOperation, sense))
					{
						BasicBlockBuilder dest2 = null;
						VisitConditionalBranchCore(binaryOperation.LeftOperand, ref dest2, !sense);
						VisitConditionalBranchCore(binaryOperation.RightOperand, ref dest, sense);
						AppendNewBlock(dest2);
						return;
					}
					VisitConditionalBranchCore(binaryOperation.LeftOperand, ref dest, sense);
					condition = binaryOperation.RightOperand;
					continue;
				}
				break;
			}
			case OperationKind.UnaryOperator:
			{
				IUnaryOperation unaryOperation = (IUnaryOperation)condition;
				if (IsBooleanLogicalNot(unaryOperation))
				{
					sense = !sense;
					condition = unaryOperation.Operand;
					continue;
				}
				break;
			}
			case OperationKind.Conditional:
				if (ITypeSymbolHelpers.IsBooleanType(condition.Type))
				{
					IConditionalOperation conditionalOperation = (IConditionalOperation)condition;
					if (ITypeSymbolHelpers.IsBooleanType(conditionalOperation.WhenTrue.Type) && ITypeSymbolHelpers.IsBooleanType(conditionalOperation.WhenFalse.Type))
					{
						BasicBlockBuilder dest3 = null;
						VisitConditionalBranchCore(conditionalOperation.Condition, ref dest3, sense: false);
						VisitConditionalBranchCore(conditionalOperation.WhenTrue, ref dest, sense);
						BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
						LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
						_currentBasicBlock = null;
						AppendNewBlock(dest3);
						VisitConditionalBranchCore(conditionalOperation.WhenFalse, ref dest, sense);
						AppendNewBlock(basicBlockBuilder);
						return;
					}
				}
				break;
			case OperationKind.Coalesce:
				if (ITypeSymbolHelpers.IsBooleanType(condition.Type))
				{
					ICoalesceOperation coalesceOperation = (ICoalesceOperation)condition;
					if (ITypeSymbolHelpers.IsBooleanType(coalesceOperation.WhenNull.Type))
					{
						BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
						EvalStackFrame frame = PushStackFrame();
						IOperation operation = NullCheckAndConvertCoalesceValue(coalesceOperation, basicBlockBuilder2);
						operation = Operation.SetParentOperation(operation, null);
						dest = dest ?? new BasicBlockBuilder(BasicBlockKind.Block);
						LinkBlocks(CurrentBasicBlock, operation, sense, RegularBranch(dest));
						_currentBasicBlock = null;
						BasicBlockBuilder basicBlockBuilder3 = new BasicBlockBuilder(BasicBlockKind.Block);
						LinkBlocks(CurrentBasicBlock, basicBlockBuilder3);
						_currentBasicBlock = null;
						PopStackFrameAndLeaveRegion(frame);
						AppendNewBlock(basicBlockBuilder2);
						VisitConditionalBranchCore(coalesceOperation.WhenNull, ref dest, sense);
						AppendNewBlock(basicBlockBuilder3);
						return;
					}
				}
				break;
			case OperationKind.Conversion:
			{
				IConversionOperation conversionOperation = (IConversionOperation)condition;
				if (conversionOperation.Operand.Kind == OperationKind.Throw)
				{
					base.Visit(conversionOperation.Operand, null);
					dest = dest ?? new BasicBlockBuilder(BasicBlockKind.Block);
					return;
				}
				break;
			}
			}
			break;
		}
		EvalStackFrame frame2 = PushStackFrame();
		condition = Operation.SetParentOperation(Visit(condition), null);
		dest = dest ?? new BasicBlockBuilder(BasicBlockKind.Block);
		LinkBlocks(CurrentBasicBlock, condition, sense, RegularBranch(dest));
		_currentBasicBlock = null;
		PopStackFrameAndLeaveRegion(frame2);
	}

	private static void LinkBlocks(BasicBlockBuilder previous, IOperation condition, bool jumpIfTrue, BasicBlockBuilder.Branch branch)
	{
		branch.Destination.AddPredecessor(previous);
		previous.BranchValue = condition;
		previous.ConditionKind = ((!jumpIfTrue) ? ControlFlowConditionKind.WhenFalse : ControlFlowConditionKind.WhenTrue);
		previous.Conditional = branch;
	}

	private IOperation NullCheckAndConvertCoalesceValue(ICoalesceOperation operation, BasicBlockBuilder whenNull)
	{
		IOperation value = operation.Value;
		SyntaxNode syntax = value.Syntax;
		ITypeSymbol type = value.Type;
		PushOperand(Visit(value));
		SpillEvalStack();
		IOperation operation2 = PopOperand();
		LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(MakeIsNullOperation(operation2), null), jumpIfTrue: true, RegularBranch(whenNull));
		_currentBasicBlock = null;
		CommonConversion valueConversion = operation.ValueConversion;
		IOperation operation3 = OperationCloner.CloneOperation(operation2);
		IOperation operation4 = null;
		if (valueConversion.Exists)
		{
			IOperation operation5 = ((!ITypeSymbolHelpers.IsNullableType(type) || (valueConversion.IsIdentity && ITypeSymbolHelpers.IsNullableType(operation.Type))) ? operation3 : TryUnwrapNullableValue(operation3));
			if (operation5 != null)
			{
				operation4 = ((!valueConversion.IsIdentity) ? new ConversionOperation(operation5, ((BaseCoalesceExpression)operation).ConvertibleValueConversion, isTryCast: false, isChecked: false, null, syntax, operation.Type, default, isImplicit: true) : operation5);
			}
		}
		if (operation4 == null)
		{
			operation4 = MakeInvalidOperation(operation.Type, operation3);
		}
		return operation4;
	}

	public override IOperation VisitCoalesce(ICoalesceOperation operation, int? captureIdForResult)
	{
		SpillEvalStack();
		IConversionOperation conversionOperation = operation.WhenNull as IConversionOperation;
		bool num = conversionOperation != null && conversionOperation.Operand.Kind == OperationKind.Throw;
		RegionBuilder currentRegion = _currentRegion;
		EvalStackFrame frame = PushStackFrame();
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation operation2 = NullCheckAndConvertCoalesceValue(operation, basicBlockBuilder);
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		IOperation result;
		if (num)
		{
			result = operation2;
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
			_currentBasicBlock = null;
			PopStackFrame(frame);
			AppendNewBlock(basicBlockBuilder);
			base.Visit(conversionOperation.Operand, null);
		}
		else
		{
			int num2 = captureIdForResult ?? GetNextCaptureId(currentRegion);
			AddStatement(new FlowCapture(num2, operation.Value.Syntax, operation2));
			result = GetCaptureReference(num2, operation);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
			_currentBasicBlock = null;
			PopStackFrameAndLeaveRegion(frame);
			AppendNewBlock(basicBlockBuilder);
			VisitAndCapture(operation.WhenNull, num2);
			LeaveRegionsUpTo(currentRegion);
		}
		AppendNewBlock(basicBlockBuilder2);
		return result;
	}

	private static BasicBlockBuilder.Branch RegularBranch(BasicBlockBuilder destination)
	{
		return new BasicBlockBuilder.Branch
		{
			Destination = destination,
			Kind = ControlFlowBranchSemantics.Regular
		};
	}

	private static IOperation MakeInvalidOperation(ITypeSymbol type, IOperation child)
	{
		return new InvalidOperation(ImmutableArray.Create(child), null, child.Syntax, type, default, isImplicit: true);
	}

	private static IOperation MakeInvalidOperation(SyntaxNode syntax, ITypeSymbol type, IOperation child1, IOperation child2)
	{
		return MakeInvalidOperation(syntax, type, ImmutableArray.Create(child1, child2));
	}

	private static IOperation MakeInvalidOperation(SyntaxNode syntax, ITypeSymbol type, ImmutableArray<IOperation> children)
	{
		return new InvalidOperation(children, null, syntax, type, default, isImplicit: true);
	}

	private IsNullOperation MakeIsNullOperation(IOperation operand)
	{
		return MakeIsNullOperation(operand, _compilation.GetSpecialType(SpecialType.System_Boolean));
	}

	private static IsNullOperation MakeIsNullOperation(IOperation operand, ITypeSymbol booleanType)
	{
		Optional<object> constantValue = operand.ConstantValue;
		return new IsNullOperation(operand.Syntax, operand, booleanType, constantValue.HasValue ? new Optional<object>(constantValue.Value == null) : default(Optional<object>));
	}

	private IOperation TryUnwrapNullableValue(IOperation value)
	{
		ITypeSymbol type = value.Type;
		IMethodSymbol methodSymbol = (IMethodSymbol)_compilation.CommonGetSpecialTypeMember(SpecialMember.System_Nullable_T_GetValueOrDefault);
		if (methodSymbol != null)
		{
			foreach (ISymbol member in type.GetMembers(methodSymbol.Name))
			{
				if (member.OriginalDefinition.Equals(methodSymbol))
				{
					methodSymbol = (IMethodSymbol)member;
					return new InvocationExpression(methodSymbol, value, isVirtual: false, ImmutableArray<IArgumentOperation>.Empty, null, value.Syntax, methodSymbol.ReturnType, default, isImplicit: true);
				}
			}
		}
		return null;
	}

	private IOperation UnwrapNullableValue(IOperation value)
	{
		return TryUnwrapNullableValue(value) ?? MakeInvalidOperation(ITypeSymbolHelpers.GetNullableUnderlyingType(value.Type), value);
	}

	public override IOperation VisitConditionalAccess(IConditionalAccessOperation operation, int? captureIdForResult)
	{
		SpillEvalStack();
		RegionBuilder currentRegion = _currentRegion;
		int num;
		if (_currentStatement != operation)
		{
			if (_currentStatement == operation.Parent)
			{
				IOperation currentStatement = _currentStatement;
				num = ((currentStatement != null && currentStatement.Kind == OperationKind.ExpressionStatement) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		ArrayBuilder<EvalStackFrame> instance = ArrayBuilder<EvalStackFrame>.GetInstance();
		if (!flag)
		{
			instance.Push(PushStackFrame());
		}
		IConditionalAccessOperation conditionalAccessOperation = operation;
		while (true)
		{
			instance.Push(PushStackFrame());
			if (conditionalAccessOperation.WhenNotNull.Kind != OperationKind.ConditionalAccess)
			{
				break;
			}
			conditionalAccessOperation = (IConditionalAccessOperation)conditionalAccessOperation.WhenNotNull;
		}
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		conditionalAccessOperation = operation;
		IOperation operation2;
		while (true)
		{
			operation2 = conditionalAccessOperation.Operation;
			_ = operation2.Syntax;
			ITypeSymbol type = operation2.Type;
			PushOperand(Visit(operation2));
			SpillEvalStack();
			IOperation operation3 = PopOperand();
			PopStackFrame(instance.Pop());
			LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(MakeIsNullOperation(operation3), null), jumpIfTrue: true, RegularBranch(basicBlockBuilder));
			_currentBasicBlock = null;
			IOperation operation4 = OperationCloner.CloneOperation(operation3);
			if (ITypeSymbolHelpers.IsNullableType(type))
			{
				operation4 = UnwrapNullableValue(operation4);
			}
			_currentConditionalAccessInstance = operation4;
			if (conditionalAccessOperation.WhenNotNull.Kind != OperationKind.ConditionalAccess)
			{
				break;
			}
			conditionalAccessOperation = (IConditionalAccessOperation)conditionalAccessOperation.WhenNotNull;
		}
		if (flag)
		{
			instance.Free();
			IOperation operation5 = Visit(conditionalAccessOperation.WhenNotNull);
			_currentConditionalAccessInstance = null;
			if (_currentStatement != operation)
			{
				IExpressionStatementOperation expressionStatementOperation = (IExpressionStatementOperation)_currentStatement;
				operation5 = new ExpressionStatement(operation5, null, expressionStatementOperation.Syntax, expressionStatementOperation.Type, expressionStatementOperation.ConstantValue, IsImplicit(expressionStatementOperation));
			}
			AddStatement(operation5);
			AppendNewBlock(basicBlockBuilder);
			return null;
		}
		EvalStackFrame frame = instance.Pop();
		instance.Free();
		int num2 = captureIdForResult ?? GetNextCaptureId(currentRegion);
		if (ITypeSymbolHelpers.IsNullableType(operation.Type) && !ITypeSymbolHelpers.IsNullableType(conditionalAccessOperation.WhenNotNull.Type))
		{
			IOperation operand = Visit(conditionalAccessOperation.WhenNotNull);
			AddStatement(new FlowCapture(num2, conditionalAccessOperation.WhenNotNull.Syntax, MakeNullable(operand, operation.Type)));
		}
		else
		{
			CaptureResultIfNotAlready(conditionalAccessOperation.WhenNotNull.Syntax, num2, Visit(conditionalAccessOperation.WhenNotNull, num2));
		}
		PopStackFrame(frame);
		LeaveRegionsUpTo(currentRegion);
		_currentConditionalAccessInstance = null;
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
		_currentBasicBlock = null;
		AppendNewBlock(basicBlockBuilder);
		IOperation operation6;
		if (operation.Operation != operation2)
		{
			operation6 = operation;
		}
		else
		{
			operation6 = operation2;
		}
		SyntaxNode syntax = operation6.Syntax;
		AddStatement(new FlowCapture(num2, syntax, new DefaultValueExpression(null, syntax, operation.Type, (operation.Type.IsReferenceType && !ITypeSymbolHelpers.IsNullableType(operation.Type)) ? new Optional<object>(null) : default(Optional<object>), isImplicit: true)));
		AppendNewBlock(basicBlockBuilder2);
		return GetCaptureReference(num2, operation);
	}

	public override IOperation VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, int? captureIdForResult)
	{
		IOperation currentConditionalAccessInstance = _currentConditionalAccessInstance;
		_currentConditionalAccessInstance = null;
		return currentConditionalAccessInstance;
	}

	public override IOperation VisitExpressionStatement(IExpressionStatementOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		IOperation operation2 = Visit(operation.Operation);
		if (operation2 == null)
		{
			return FinishVisitingStatement(operation);
		}
		if (operation.Operation.Kind == OperationKind.Throw)
		{
			return FinishVisitingStatement(operation);
		}
		return FinishVisitingStatement(operation, new ExpressionStatement(operation2, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
	}

	public override IOperation VisitWhileLoop(IWhileLoopOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		RegionBuilder region = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals);
		BasicBlockBuilder labeledOrNewBlock = GetLabeledOrNewBlock(operation.ContinueLabel);
		BasicBlockBuilder dest = GetLabeledOrNewBlock(operation.ExitLabel);
		if (operation.ConditionIsTop)
		{
			AppendNewBlock(labeledOrNewBlock);
			EnterRegion(region);
			VisitConditionalBranch(operation.Condition, ref dest, operation.ConditionIsUntil);
			VisitStatement(operation.Body);
			LinkBlocks(CurrentBasicBlock, labeledOrNewBlock);
		}
		else
		{
			BasicBlockBuilder dest2 = new BasicBlockBuilder(BasicBlockKind.Block);
			AppendNewBlock(dest2);
			EnterRegion(region);
			VisitStatement(operation.Body);
			AppendNewBlock(labeledOrNewBlock);
			if (operation.Condition != null)
			{
				VisitConditionalBranch(operation.Condition, ref dest2, !operation.ConditionIsUntil);
			}
			else
			{
				LinkBlocks(CurrentBasicBlock, dest2);
				_currentBasicBlock = null;
			}
		}
		LeaveRegion();
		AppendNewBlock(dest);
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitTry(ITryOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		BasicBlockBuilder labeledOrNewBlock = GetLabeledOrNewBlock(operation.ExitLabel);
		if (operation.Catches.IsEmpty && operation.Finally == null)
		{
			VisitStatement(operation.Body);
			AppendNewBlock(labeledOrNewBlock);
			return FinishVisitingStatement(operation);
		}
		bool flag = operation.Finally != null;
		if (flag)
		{
			RegionBuilder region = new RegionBuilder(ControlFlowRegionKind.TryAndFinally);
			EnterRegion(region);
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.Try));
		}
		bool flag2 = !operation.Catches.IsEmpty;
		if (flag2)
		{
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.TryAndCatch));
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.Try));
		}
		VisitStatement(operation.Body);
		LinkBlocks(CurrentBasicBlock, labeledOrNewBlock);
		if (flag2)
		{
			LeaveRegion();
			foreach (ICatchClauseOperation @catch in operation.Catches)
			{
				IOperation exceptionDeclarationOrExpression = @catch.ExceptionDeclarationOrExpression;
				IOperation filter = @catch.Filter;
				bool flag3 = filter != null;
				BasicBlockBuilder dest = new BasicBlockBuilder(BasicBlockKind.Block);
				if (flag3)
				{
					RegionBuilder region2 = new RegionBuilder(ControlFlowRegionKind.FilterAndHandler, @catch.ExceptionType, @catch.Locals);
					EnterRegion(region2);
					RegionBuilder region3 = new RegionBuilder(ControlFlowRegionKind.Filter, @catch.ExceptionType);
					EnterRegion(region3);
					AddExceptionStore(@catch.ExceptionType, exceptionDeclarationOrExpression);
					VisitConditionalBranch(filter, ref dest, sense: true);
					BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
					AppendNewBlock(basicBlockBuilder);
					basicBlockBuilder.FallThrough.Kind = ControlFlowBranchSemantics.StructuredExceptionHandling;
					LeaveRegion();
				}
				RegionBuilder region4 = new RegionBuilder(ControlFlowRegionKind.Catch, @catch.ExceptionType, flag3 ? default(ImmutableArray<ILocalSymbol>) : @catch.Locals);
				EnterRegion(region4);
				AppendNewBlock(dest, linkToPrevious: false);
				if (!flag3)
				{
					AddExceptionStore(@catch.ExceptionType, exceptionDeclarationOrExpression);
				}
				VisitStatement(@catch.Handler);
				LinkBlocks(CurrentBasicBlock, labeledOrNewBlock);
				LeaveRegion();
				if (flag3)
				{
					LeaveRegion();
				}
			}
			LeaveRegion();
		}
		if (flag)
		{
			LeaveRegion();
			RegionBuilder region5 = new RegionBuilder(ControlFlowRegionKind.Finally);
			EnterRegion(region5);
			AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
			VisitStatement(operation.Finally);
			BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
			AppendNewBlock(basicBlockBuilder2);
			basicBlockBuilder2.FallThrough.Kind = ControlFlowBranchSemantics.StructuredExceptionHandling;
			LeaveRegion();
			LeaveRegion();
		}
		AppendNewBlock(labeledOrNewBlock, linkToPrevious: false);
		return FinishVisitingStatement(operation);
	}

	private void AddExceptionStore(ITypeSymbol exceptionType, IOperation exceptionDeclarationOrExpression)
	{
		if (exceptionDeclarationOrExpression != null)
		{
			SyntaxNode syntax = exceptionDeclarationOrExpression.Syntax;
			IOperation operation;
			if (exceptionDeclarationOrExpression.Kind == OperationKind.VariableDeclarator)
			{
				ILocalSymbol symbol = ((IVariableDeclaratorOperation)exceptionDeclarationOrExpression).Symbol;
				operation = new LocalReferenceExpression(symbol, isDeclaration: true, null, syntax, symbol.Type, default, isImplicit: true);
			}
			else
			{
				operation = Visit(exceptionDeclarationOrExpression);
			}
			if (operation != null)
			{
				AddStatement(new SimpleAssignmentExpression(operation, isRef: false, new CaughtExceptionOperation(syntax, exceptionType), null, syntax, null, default, isImplicit: true));
			}
		}
	}

	public override IOperation VisitCatchClause(ICatchClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitReturn(IReturnOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		IOperation operation2 = Visit(operation.ReturnedValue);
		switch (operation.Kind)
		{
		case OperationKind.YieldReturn:
			AddStatement(new ReturnStatement(OperationKind.YieldReturn, operation2, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
			break;
		case OperationKind.Return:
		case OperationKind.YieldBreak:
		{
			BasicBlockBuilder currentBasicBlock = CurrentBasicBlock;
			LinkBlocks(CurrentBasicBlock, _exit, (operation2 == null) ? ControlFlowBranchSemantics.Regular : ControlFlowBranchSemantics.Return);
			currentBasicBlock.BranchValue = Operation.SetParentOperation(operation2, null);
			_currentBasicBlock = null;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(operation.Kind);
		}
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitLabeled(ILabeledOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		BasicBlockBuilder basicBlockBuilder = GetLabeledOrNewBlock(operation.Label);
		if (basicBlockBuilder.Ordinal != -1)
		{
			basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		}
		AppendNewBlock(basicBlockBuilder);
		VisitStatement(operation.Operation);
		return FinishVisitingStatement(operation);
	}

	private BasicBlockBuilder GetLabeledOrNewBlock(ILabelSymbol labelOpt)
	{
		if (labelOpt == null)
		{
			return new BasicBlockBuilder(BasicBlockKind.Block);
		}
		BasicBlockBuilder value;
		if (_labeledBlocks == null)
		{
			_labeledBlocks = PooledDictionary<ILabelSymbol, BasicBlockBuilder>.GetInstance();
		}
		else if (_labeledBlocks.TryGetValue(labelOpt, out value))
		{
			return value;
		}
		value = new BasicBlockBuilder(BasicBlockKind.Block);
		_labeledBlocks.Add(labelOpt, value);
		return value;
	}

	public override IOperation VisitBranch(IBranchOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		LinkBlocks(CurrentBasicBlock, GetLabeledOrNewBlock(operation.Target));
		_currentBasicBlock = null;
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitEmpty(IEmptyOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitThrow(IThrowOperation operation, int? captureIdForResult)
	{
		bool flag = _currentStatement == operation;
		if (!flag)
		{
			SpillEvalStack();
		}
		EvalStackFrame frame = PushStackFrame();
		IOperation branchValue = Operation.SetParentOperation(Visit(operation.Exception), null);
		BasicBlockBuilder currentBasicBlock = CurrentBasicBlock;
		currentBasicBlock.BranchValue = branchValue;
		currentBasicBlock.FallThrough.Kind = ((operation.Exception == null) ? ControlFlowBranchSemantics.Rethrow : ControlFlowBranchSemantics.Throw);
		PopStackFrameAndLeaveRegion(frame);
		AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block), linkToPrevious: false);
		if (flag)
		{
			return null;
		}
		return Operation.CreateOperationNone(null, operation.Syntax, default, ImmutableArray<IOperation>.Empty, isImplicit: true);
	}

	public override IOperation VisitUsing(IUsingOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		ITypeSymbol iDisposable = _compilation.GetSpecialType(SpecialType.System_IDisposable);
		RegionBuilder region = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals);
		EnterRegion(region);
		if (operation.Resources.Kind == OperationKind.VariableDeclarationGroup)
		{
			IVariableDeclarationGroupOperation variableDeclarationGroupOperation = (IVariableDeclarationGroupOperation)operation.Resources;
			ArrayBuilder<(IVariableDeclarationOperation, IVariableDeclaratorOperation)> instance = ArrayBuilder<(IVariableDeclarationOperation, IVariableDeclaratorOperation)>.GetInstance(variableDeclarationGroupOperation.Declarations.Length);
			foreach (IVariableDeclarationOperation declaration2 in variableDeclarationGroupOperation.Declarations)
			{
				foreach (IVariableDeclaratorOperation declarator in declaration2.Declarators)
				{
					instance.Add((declaration2, declarator));
				}
			}
			instance.ReverseContents();
			processQueue(instance);
		}
		else
		{
			EvalStackFrame frame = PushStackFrame();
			IOperation operation2 = Visit(operation.Resources);
			if (shouldConvertToIDisposableBeforeTry(operation2))
			{
				operation2 = ConvertToIDisposable(operation2, iDisposable);
			}
			PushOperand(operation2);
			SpillEvalStack();
			operation2 = PopOperand();
			PopStackFrame(frame);
			processResource(operation2, null);
			LeaveRegionIfAny(frame);
		}
		LeaveRegion();
		return FinishVisitingStatement(operation);
		void processQueue(ArrayBuilder<(IVariableDeclarationOperation, IVariableDeclaratorOperation)> resourceQueueOpt)
		{
			if (resourceQueueOpt == null || resourceQueueOpt.Count == 0)
			{
				VisitStatement(operation.Body);
			}
			else
			{
				var (declaration, variableDeclaratorOperation) = resourceQueueOpt.Pop();
				HandleVariableDeclarator(declaration, variableDeclaratorOperation);
				ILocalSymbol symbol = variableDeclaratorOperation.Symbol;
				processResource(new LocalReferenceExpression(symbol, isDeclaration: false, null, variableDeclaratorOperation.Syntax, symbol.Type, default, isImplicit: true), resourceQueueOpt);
			}
		}
		void processResource(IOperation resource, ArrayBuilder<(IVariableDeclarationOperation, IVariableDeclaratorOperation)> resourceQueueOpt)
		{
			RegionBuilder regionBuilder = null;
			if (shouldConvertToIDisposableBeforeTry(resource))
			{
				regionBuilder = new RegionBuilder(ControlFlowRegionKind.LocalLifetime);
				EnterRegion(regionBuilder);
				resource = ConvertToIDisposable(resource, iDisposable);
				int nextCaptureId = GetNextCaptureId(regionBuilder);
				AddStatement(new FlowCapture(nextCaptureId, resource.Syntax, resource));
				resource = GetCaptureReference(nextCaptureId, resource);
			}
			BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.TryAndFinally));
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.Try));
			processQueue(resourceQueueOpt);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
			LeaveRegion();
			AddDisposingFinally(resource, knownToImplementIDisposable: true, iDisposable);
			LeaveRegion();
			if (regionBuilder != null)
			{
				LeaveRegion();
			}
			AppendNewBlock(basicBlockBuilder, linkToPrevious: false);
		}
		static bool shouldConvertToIDisposableBeforeTry(IOperation resource)
		{
			if (resource.Type != null)
			{
				return resource.Type.Kind == SymbolKind.DynamicType;
			}
			return true;
		}
	}

	private void AddDisposingFinally(IOperation resource, bool knownToImplementIDisposable, ITypeSymbol iDisposable)
	{
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		basicBlockBuilder.FallThrough.Kind = ControlFlowBranchSemantics.StructuredExceptionHandling;
		RegionBuilder regionBuilder = new RegionBuilder(ControlFlowRegionKind.Finally);
		EnterRegion(regionBuilder);
		AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
		if (!knownToImplementIDisposable)
		{
			resource = ConvertToIDisposable(resource, iDisposable, isTryCast: true);
			int nextCaptureId = GetNextCaptureId(regionBuilder);
			AddStatement(new FlowCapture(nextCaptureId, resource.Syntax, resource));
			resource = GetCaptureReference(nextCaptureId, resource);
		}
		if (!knownToImplementIDisposable || !isNotNullableValueType(resource.Type))
		{
			IOperation operation = MakeIsNullOperation(OperationCloner.CloneOperation(resource));
			operation = Operation.SetParentOperation(operation, null);
			LinkBlocks(CurrentBasicBlock, operation, jumpIfTrue: true, RegularBranch(basicBlockBuilder));
			_currentBasicBlock = null;
		}
		if (!resource.Type.Equals(iDisposable))
		{
			resource = ConvertToIDisposable(resource, iDisposable);
		}
		AddStatement(tryDispose(resource) ?? MakeInvalidOperation(null, resource));
		AppendNewBlock(basicBlockBuilder);
		LeaveRegion();
		static bool isNotNullableValueType(ITypeSymbol type)
		{
			if (type != null && type.IsValueType)
			{
				return !ITypeSymbolHelpers.IsNullableType(type);
			}
			return false;
		}
		IOperation tryDispose(IOperation value)
		{
			IMethodSymbol methodSymbol = (IMethodSymbol)_compilation.CommonGetSpecialTypeMember(SpecialMember.System_IDisposable__Dispose);
			if (methodSymbol != null)
			{
				return new InvocationExpression(methodSymbol, value, isVirtual: true, ImmutableArray<IArgumentOperation>.Empty, null, value.Syntax, methodSymbol.ReturnType, default, isImplicit: true);
			}
			return null;
		}
	}

	private IOperation ConvertToIDisposable(IOperation operand, ITypeSymbol iDisposable, bool isTryCast = false)
	{
		Optional<object> constantValue;
		return new ConversionOperation(operand, _compilation.ClassifyConvertibleConversion(operand, iDisposable, out constantValue), isTryCast, isChecked: false, null, operand.Syntax, iDisposable, constantValue, isImplicit: true);
	}

	public override IOperation VisitLock(ILockOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		ITypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Object);
		BaseLockStatement baseLockStatement = (BaseLockStatement)operation;
		RegionBuilder regionBuilder = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, (baseLockStatement.LockTakenSymbol != null) ? ImmutableArray.Create(baseLockStatement.LockTakenSymbol) : ImmutableArray<ILocalSymbol>.Empty);
		EnterRegion(regionBuilder);
		EvalStackFrame frame = PushStackFrame();
		IOperation operation2 = Visit(operation.LockedValue);
		if (!specialType.Equals(operation2.Type))
		{
			operation2 = CreateConversion(operation2, specialType);
		}
		PushOperand(operation2);
		SpillEvalStack();
		operation2 = PopOperand();
		PopStackFrame(frame);
		IMethodSymbol methodSymbol = (IMethodSymbol)_compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Threading_Monitor__Enter2);
		bool flag = methodSymbol == null;
		if (flag)
		{
			methodSymbol = (IMethodSymbol)_compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Threading_Monitor__Enter);
			if (methodSymbol == null)
			{
				AddStatement(MakeInvalidOperation(null, operation2));
			}
			else
			{
				AddStatement(new InvocationExpression(methodSymbol, null, isVirtual: false, ImmutableArray.Create((IArgumentOperation)new ArgumentOperation(operation2, ArgumentKind.Explicit, methodSymbol.Parameters[0], null, null, null, operation2.Syntax, isImplicit: true)), null, operation2.Syntax, methodSymbol.ReturnType, default, isImplicit: true));
			}
		}
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.TryAndFinally));
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.Try));
		if (!flag)
		{
			AddStatement(new InvocationExpression(methodSymbol, null, isVirtual: false, ImmutableArray.Create(item2: (IArgumentOperation)new ArgumentOperation(new LocalReferenceExpression(baseLockStatement.LockTakenSymbol, isDeclaration: true, null, operation2.Syntax, baseLockStatement.LockTakenSymbol.Type, default, isImplicit: true), ArgumentKind.Explicit, methodSymbol.Parameters[1], null, null, null, operation2.Syntax, isImplicit: true), item1: (IArgumentOperation)new ArgumentOperation(operation2, ArgumentKind.Explicit, methodSymbol.Parameters[0], null, null, null, operation2.Syntax, isImplicit: true)), null, operation2.Syntax, methodSymbol.ReturnType, default, isImplicit: true));
		}
		VisitStatement(operation.Body);
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		LeaveRegion();
		BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block)
		{
			FallThrough = 
			{
				Kind = ControlFlowBranchSemantics.StructuredExceptionHandling
			}
		};
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.Finally));
		AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
		if (!flag)
		{
			IOperation operation3 = new LocalReferenceExpression(baseLockStatement.LockTakenSymbol, isDeclaration: false, null, operation2.Syntax, baseLockStatement.LockTakenSymbol.Type, default, isImplicit: true);
			operation3 = Operation.SetParentOperation(operation3, null);
			LinkBlocks(CurrentBasicBlock, operation3, jumpIfTrue: false, RegularBranch(basicBlockBuilder2));
			_currentBasicBlock = null;
		}
		IMethodSymbol methodSymbol2 = (IMethodSymbol)_compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Threading_Monitor__Exit);
		operation2 = OperationCloner.CloneOperation(operation2);
		if (methodSymbol2 == null)
		{
			AddStatement(MakeInvalidOperation(null, operation2));
		}
		else
		{
			AddStatement(new InvocationExpression(methodSymbol2, null, isVirtual: false, ImmutableArray.Create((IArgumentOperation)new ArgumentOperation(operation2, ArgumentKind.Explicit, methodSymbol2.Parameters[0], null, null, null, operation2.Syntax, isImplicit: true)), null, operation2.Syntax, methodSymbol2.ReturnType, default, isImplicit: true));
		}
		AppendNewBlock(basicBlockBuilder2);
		LeaveRegion();
		LeaveRegion();
		LeaveRegionsUpTo(regionBuilder);
		LeaveRegion();
		AppendNewBlock(basicBlockBuilder, linkToPrevious: false);
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitForEachLoop(IForEachLoopOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		RegionBuilder enumeratorCaptureRegion = new RegionBuilder(ControlFlowRegionKind.LocalLifetime);
		EnterRegion(enumeratorCaptureRegion);
		ForEachLoopOperationInfo info = ((BaseForEachLoopStatement)operation).Info;
		RegionBuilder regionBuilder = null;
		if (!operation.Locals.IsEmpty && operation.LoopControlVariable.Kind == OperationKind.VariableDeclarator)
		{
			ILocalSymbol symbol = ((IVariableDeclaratorOperation)operation.LoopControlVariable).Symbol;
			foreach (IOperation item in operation.Collection.DescendantsAndSelf())
			{
				if (item is ILocalReferenceOperation localReferenceOperation && localReferenceOperation.Local.Equals(symbol))
				{
					regionBuilder = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, ImmutableArray.Create(symbol));
					EnterRegion(regionBuilder);
					break;
				}
			}
		}
		IOperation operation2 = getEnumerator();
		if (regionBuilder != null)
		{
			LeaveRegion();
		}
		if (info.NeedsDispose)
		{
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.TryAndFinally));
			EnterRegion(new RegionBuilder(ControlFlowRegionKind.Try));
		}
		BasicBlockBuilder labeledOrNewBlock = GetLabeledOrNewBlock(operation.ContinueLabel);
		BasicBlockBuilder labeledOrNewBlock2 = GetLabeledOrNewBlock(operation.ExitLabel);
		AppendNewBlock(labeledOrNewBlock);
		EvalStackFrame frame = PushStackFrame();
		IOperation condition = Operation.SetParentOperation(getCondition(operation2), null);
		LinkBlocks(CurrentBasicBlock, condition, jumpIfTrue: false, RegularBranch(labeledOrNewBlock2));
		_currentBasicBlock = null;
		PopStackFrameAndLeaveRegion(frame);
		RegionBuilder region = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals);
		EnterRegion(region);
		frame = PushStackFrame();
		AddStatement(getLoopControlVariableAssignment(applyConversion(info.CurrentConversion, getCurrent(OperationCloner.CloneOperation(operation2)), info.ElementType)));
		PopStackFrameAndLeaveRegion(frame);
		VisitStatement(operation.Body);
		LinkBlocks(CurrentBasicBlock, labeledOrNewBlock);
		LeaveRegion();
		AppendNewBlock(labeledOrNewBlock2);
		if (info.NeedsDispose)
		{
			BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
			LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
			LeaveRegion();
			AddDisposingFinally(OperationCloner.CloneOperation(operation2), info.KnownToImplementIDisposable, _compilation.GetSpecialType(SpecialType.System_IDisposable));
			LeaveRegion();
			AppendNewBlock(basicBlockBuilder, linkToPrevious: false);
		}
		LeaveRegion();
		return FinishVisitingStatement(operation);
		static IOperation applyConversion(IConvertibleConversion conversionOpt, IOperation operand, ITypeSymbol targetType)
		{
			if (conversionOpt != null && !conversionOpt.ToCommonConversion().IsIdentity)
			{
				operand = new ConversionOperation(operand, conversionOpt, isTryCast: false, isChecked: false, null, operand.Syntax, targetType, default, isImplicit: true);
			}
			return operand;
		}
		IOperation getCondition(IOperation enumeratorRef)
		{
			if (info.MoveNextMethod != null)
			{
				return makeInvocationDroppingInstanceForStaticMethods(info.MoveNextMethod, enumeratorRef, info.MoveNextArguments);
			}
			return MakeInvalidOperation(_compilation.GetSpecialType(SpecialType.System_Boolean), enumeratorRef);
		}
		IOperation getCurrent(IOperation enumeratorRef)
		{
			if (info.CurrentProperty != null)
			{
				return new PropertyReferenceExpression(info.CurrentProperty, info.CurrentProperty.IsStatic ? null : enumeratorRef, makeArguments(info.CurrentArguments), null, operation.LoopControlVariable.Syntax, info.CurrentProperty.Type, default, isImplicit: true);
			}
			return MakeInvalidOperation(null, enumeratorRef);
		}
		IOperation getEnumerator()
		{
			EvalStackFrame frame2 = PushStackFrame();
			IOperation result;
			if (info.GetEnumeratorMethod != null)
			{
				IOperation value = makeInvocation(operation.Collection.Syntax, info.GetEnumeratorMethod, info.GetEnumeratorMethod.IsStatic ? null : Visit(operation.Collection), info.GetEnumeratorArguments);
				int nextCaptureId = GetNextCaptureId(enumeratorCaptureRegion);
				AddStatement(new FlowCapture(nextCaptureId, operation.Collection.Syntax, value));
				result = new FlowCaptureReference(nextCaptureId, operation.Collection.Syntax, info.GetEnumeratorMethod.ReturnType, default);
			}
			else
			{
				AddStatement(MakeInvalidOperation(null, Visit(operation.Collection)));
				result = new InvalidOperation(ImmutableArray<IOperation>.Empty, null, operation.Collection.Syntax, null, default, isImplicit: true);
			}
			PopStackFrameAndLeaveRegion(frame2);
			return result;
		}
		IOperation getLoopControlVariableAssignment(IOperation current)
		{
			switch (operation.LoopControlVariable.Kind)
			{
			case OperationKind.VariableDeclarator:
			{
				IVariableDeclaratorOperation variableDeclaratorOperation = (IVariableDeclaratorOperation)operation.LoopControlVariable;
				ILocalSymbol symbol2 = variableDeclaratorOperation.Symbol;
				current = applyConversion(info.ElementConversion, current, symbol2.Type);
				return new SimpleAssignmentExpression(new LocalReferenceExpression(symbol2, isDeclaration: true, null, variableDeclaratorOperation.Syntax, symbol2.Type, default, isImplicit: true), symbol2.RefKind != RefKind.None, current, null, variableDeclaratorOperation.Syntax, null, default, isImplicit: true);
			}
			case OperationKind.Tuple:
			case OperationKind.DeclarationExpression:
				return new DeconstructionAssignmentExpression(VisitPreservingTupleOperations(operation.LoopControlVariable), current, null, operation.LoopControlVariable.Syntax, operation.LoopControlVariable.Type, default, isImplicit: true);
			default:
				return new SimpleAssignmentExpression(Visit(operation.LoopControlVariable), isRef: false, current, null, operation.LoopControlVariable.Syntax, operation.LoopControlVariable.Type, default, isImplicit: true);
			}
		}
		ImmutableArray<IArgumentOperation> makeArguments(Lazy<ImmutableArray<IArgumentOperation>> arguments)
		{
			if (arguments != null)
			{
				return VisitArguments(arguments.Value);
			}
			return ImmutableArray<IArgumentOperation>.Empty;
		}
		InvocationExpression makeInvocation(SyntaxNode syntax, IMethodSymbol method, IOperation instanceOpt, Lazy<ImmutableArray<IArgumentOperation>> arguments)
		{
			return new InvocationExpression(method, instanceOpt, method.IsVirtual || method.IsAbstract || method.IsOverride, makeArguments(arguments), null, syntax, method.ReturnType, default, isImplicit: true);
		}
		InvocationExpression makeInvocationDroppingInstanceForStaticMethods(IMethodSymbol method, IOperation instance, Lazy<ImmutableArray<IArgumentOperation>> arguments)
		{
			return makeInvocation(instance.Syntax, method, method.IsStatic ? null : instance, arguments);
		}
	}

	public override IOperation VisitForToLoop(IForToLoopOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		(ILocalSymbol, ForToLoopOperationUserDefinedInfo) info = ((BaseForToLoopStatement)operation).Info;
		ILocalSymbol loopObject = info.Item1;
		ForToLoopOperationUserDefinedInfo userDefinedInfo = info.Item2;
		bool isObjectLoop = loopObject != null;
		ImmutableArray<ILocalSymbol> locals = operation.Locals;
		if (isObjectLoop)
		{
			locals = locals.Insert(0, loopObject);
		}
		ITypeSymbol booleanType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BasicBlockBuilder labeledOrNewBlock = GetLabeledOrNewBlock(operation.ContinueLabel);
		BasicBlockBuilder @break = GetLabeledOrNewBlock(operation.ExitLabel);
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		BasicBlockBuilder bodyBlock = new BasicBlockBuilder(BasicBlockKind.Block);
		RegionBuilder loopRegion = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, locals);
		EnterRegion(loopRegion);
		int limitValueId = -1;
		int stepValueId = -1;
		IFlowCaptureReferenceOperation positiveFlag = null;
		ITypeSymbol stepEnumUnderlyingTypeOrSelf = ITypeSymbolHelpers.GetEnumUnderlyingTypeOrSelf(operation.StepValue.Type);
		initializeLoop();
		AppendNewBlock(basicBlockBuilder);
		checkLoopCondition();
		AppendNewBlock(bodyBlock);
		VisitStatement(operation.Body);
		AppendNewBlock(labeledOrNewBlock);
		incrementLoopControlVariable();
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		_currentBasicBlock = null;
		LeaveRegion();
		AppendNewBlock(@break);
		return FinishVisitingStatement(operation);
		void checkLoopCondition()
		{
			if (isObjectLoop)
			{
				EvalStackFrame frame = PushStackFrame();
				PushOperand(visitLoopControlVariableReference(forceImplicit: true));
				IOperation operation2 = tryCallObjectForLoopControlHelper(operation.LimitValue.Syntax, WellKnownMember.Microsoft_VisualBasic_CompilerServices_ObjectFlowControl_ForLoopControl__ForNextCheckObj);
				LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation2, null), jumpIfTrue: false, RegularBranch(@break));
				LinkBlocks(CurrentBasicBlock, bodyBlock);
				_currentBasicBlock = null;
				PopStackFrameAndLeaveRegion(frame);
			}
			else if (userDefinedInfo != null)
			{
				EvalStackFrame frame2 = PushStackFrame();
				PushOperand(visitLoopControlVariableReference(forceImplicit: true));
				SpillEvalStack();
				IOperation forToLoopBinaryOperatorLeftOperand = PopOperand();
				BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
				LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(positiveFlag, null), jumpIfTrue: false, RegularBranch(basicBlockBuilder2));
				_currentBasicBlock = null;
				_forToLoopBinaryOperatorLeftOperand = forToLoopBinaryOperatorLeftOperand;
				_forToLoopBinaryOperatorRightOperand = GetCaptureReference(limitValueId, operation.LimitValue);
				VisitConditionalBranch(userDefinedInfo.LessThanOrEqual.Value, ref @break, sense: false);
				LinkBlocks(CurrentBasicBlock, bodyBlock);
				AppendNewBlock(basicBlockBuilder2);
				_forToLoopBinaryOperatorLeftOperand = OperationCloner.CloneOperation(_forToLoopBinaryOperatorLeftOperand);
				_forToLoopBinaryOperatorRightOperand = OperationCloner.CloneOperation(_forToLoopBinaryOperatorRightOperand);
				VisitConditionalBranch(userDefinedInfo.GreaterThanOrEqual.Value, ref @break, sense: false);
				LinkBlocks(CurrentBasicBlock, bodyBlock);
				_currentBasicBlock = null;
				PopStackFrameAndLeaveRegion(frame2);
				_forToLoopBinaryOperatorLeftOperand = null;
				_forToLoopBinaryOperatorRightOperand = null;
			}
			else
			{
				EvalStackFrame frame3 = PushStackFrame();
				PushOperand(visitLoopControlVariableReference(forceImplicit: true));
				IOperation operation3 = GetCaptureReference(limitValueId, operation.LimitValue);
				BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.None;
				if (ITypeSymbolHelpers.IsUnsignedIntegralType(stepEnumUnderlyingTypeOrSelf))
				{
					binaryOperatorKind = BinaryOperatorKind.LessThanOrEqual;
				}
				else if (operation.StepValue.ConstantValue.HasValue)
				{
					object value = operation.StepValue.ConstantValue.Value;
					ConstantValueTypeDiscriminator discriminator = ConstantValue.GetDiscriminator(stepEnumUnderlyingTypeOrSelf.SpecialType);
					if (value != null && discriminator != ConstantValueTypeDiscriminator.Bad)
					{
						ConstantValue constantValue = ConstantValue.Create(value, discriminator);
						if (constantValue.IsNegativeNumeric)
						{
							binaryOperatorKind = BinaryOperatorKind.GreaterThanOrEqual;
						}
						else if (constantValue.IsNumeric)
						{
							binaryOperatorKind = BinaryOperatorKind.LessThanOrEqual;
						}
					}
				}
				if (binaryOperatorKind == BinaryOperatorKind.None && ITypeSymbolHelpers.IsSignedIntegralType(stepEnumUnderlyingTypeOrSelf))
				{
					binaryOperatorKind = BinaryOperatorKind.LessThanOrEqual;
					PushOperand(negateIfStepNegative(PopOperand()));
					operation3 = negateIfStepNegative(operation3);
				}
				if (binaryOperatorKind != BinaryOperatorKind.None)
				{
					IOperation operation4 = new BinaryOperatorExpression(binaryOperatorKind, PopOperand(), operation3, isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation.LimitValue.Syntax, booleanType, default, isImplicit: true);
					LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation4, null), jumpIfTrue: false, RegularBranch(@break));
					LinkBlocks(CurrentBasicBlock, bodyBlock);
					_currentBasicBlock = null;
					PopStackFrameAndLeaveRegion(frame3);
				}
				else if (positiveFlag == null)
				{
					IOperation operation4 = MakeInvalidOperation(operation.LimitValue.Syntax, booleanType, PopOperand(), operation3);
					LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation4, null), jumpIfTrue: false, RegularBranch(@break));
					LinkBlocks(CurrentBasicBlock, bodyBlock);
					_currentBasicBlock = null;
					PopStackFrameAndLeaveRegion(frame3);
				}
				else
				{
					if (ITypeSymbolHelpers.IsNullableType(operation.LimitValue.Type))
					{
						IOperation operation5 = new BinaryOperatorExpression(BinaryOperatorKind.Or, MakeIsNullOperation(operation3, booleanType), MakeIsNullOperation(PopOperand(), booleanType), isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation.StepValue.Syntax, _compilation.GetSpecialType(SpecialType.System_Boolean), default, isImplicit: true);
						BasicBlockBuilder basicBlockBuilder3 = new BasicBlockBuilder(BasicBlockKind.Block);
						LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation5, null), jumpIfTrue: false, RegularBranch(basicBlockBuilder3));
						LinkBlocks(CurrentBasicBlock, @break);
						PopStackFrameAndLeaveRegion(frame3);
						AppendNewBlock(basicBlockBuilder3);
						frame3 = PushStackFrame();
						PushOperand(UnwrapNullableValue(visitLoopControlVariableReference(forceImplicit: true)));
						operation3 = UnwrapNullableValue(GetCaptureReference(limitValueId, operation.LimitValue));
					}
					SpillEvalStack();
					IOperation operation6 = PopOperand();
					BasicBlockBuilder basicBlockBuilder4 = new BasicBlockBuilder(BasicBlockKind.Block);
					LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(positiveFlag, null), jumpIfTrue: false, RegularBranch(basicBlockBuilder4));
					_currentBasicBlock = null;
					IOperation operation4 = new BinaryOperatorExpression(BinaryOperatorKind.LessThanOrEqual, operation6, operation3, isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation.LimitValue.Syntax, booleanType, default, isImplicit: true);
					LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation4, null), jumpIfTrue: false, RegularBranch(@break));
					LinkBlocks(CurrentBasicBlock, bodyBlock);
					AppendNewBlock(basicBlockBuilder4);
					operation4 = new BinaryOperatorExpression(BinaryOperatorKind.GreaterThanOrEqual, OperationCloner.CloneOperation(operation6), OperationCloner.CloneOperation(operation3), isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation.LimitValue.Syntax, booleanType, default, isImplicit: true);
					LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation4, null), jumpIfTrue: false, RegularBranch(@break));
					LinkBlocks(CurrentBasicBlock, bodyBlock);
					_currentBasicBlock = null;
					PopStackFrameAndLeaveRegion(frame3);
				}
			}
		}
		void incrementLoopControlVariable()
		{
			if (!isObjectLoop)
			{
				if (userDefinedInfo != null)
				{
					EvalStackFrame frame = PushStackFrame();
					IOperation operation2 = visitLoopControlVariableReference(forceImplicit: true);
					PushOperand(operation2);
					_forToLoopBinaryOperatorLeftOperand = visitLoopControlVariableReference(forceImplicit: true);
					_forToLoopBinaryOperatorRightOperand = GetCaptureReference(stepValueId, operation.StepValue);
					IOperation value = Visit(userDefinedInfo.Addition.Value);
					_forToLoopBinaryOperatorLeftOperand = null;
					_forToLoopBinaryOperatorRightOperand = null;
					operation2 = PopOperand();
					AddStatement(new SimpleAssignmentExpression(operation2, isRef: false, value, null, operation2.Syntax, null, default, isImplicit: true));
					PopStackFrameAndLeaveRegion(frame);
				}
				else
				{
					BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
					bool flag = ITypeSymbolHelpers.IsNullableType(operation.StepValue.Type);
					EvalStackFrame frame2 = PushStackFrame();
					PushOperand(visitLoopControlVariableReference(forceImplicit: true));
					IOperation operation4;
					if (flag)
					{
						SpillEvalStack();
						BasicBlockBuilder basicBlockBuilder3 = new BasicBlockBuilder(BasicBlockKind.Block);
						EvalStackFrame frame3 = PushStackFrame();
						IOperation operation3 = new BinaryOperatorExpression(BinaryOperatorKind.Or, MakeIsNullOperation(GetCaptureReference(stepValueId, operation.StepValue), booleanType), MakeIsNullOperation(visitLoopControlVariableReference(forceImplicit: true), booleanType), isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation.StepValue.Syntax, _compilation.GetSpecialType(SpecialType.System_Boolean), default, isImplicit: true);
						operation3 = Operation.SetParentOperation(operation3, null);
						LinkBlocks(CurrentBasicBlock, operation3, jumpIfTrue: false, RegularBranch(basicBlockBuilder3));
						_currentBasicBlock = null;
						PopStackFrameAndLeaveRegion(frame3);
						operation4 = OperationCloner.CloneOperation(PeekOperand());
						AddStatement(new SimpleAssignmentExpression(operation4, isRef: false, new DefaultValueExpression(null, operation4.Syntax, operation4.Type, default, isImplicit: true), null, operation4.Syntax, null, default, isImplicit: true));
						LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
						AppendNewBlock(basicBlockBuilder3);
					}
					IOperation operation5 = visitLoopControlVariableReference(forceImplicit: true);
					IOperation operation6 = GetCaptureReference(stepValueId, operation.StepValue);
					if (flag)
					{
						operation5 = UnwrapNullableValue(operation5);
						operation6 = UnwrapNullableValue(operation6);
					}
					IOperation operation7 = new BinaryOperatorExpression(BinaryOperatorKind.Add, operation5, operation6, isLifted: false, operation.IsChecked, isCompareText: false, null, null, null, operation.StepValue.Syntax, operation5.Type, default, isImplicit: true);
					operation4 = PopOperand();
					if (flag)
					{
						operation7 = MakeNullable(operation7, operation4.Type);
					}
					AddStatement(new SimpleAssignmentExpression(operation4, isRef: false, operation7, null, operation4.Syntax, null, default, isImplicit: true));
					PopStackFrame(frame2, !flag);
					LeaveRegionIfAny(frame2);
					AppendNewBlock(basicBlockBuilder2);
				}
			}
		}
		void initializeLoop()
		{
			EvalStackFrame frame = PushStackFrame();
			PushOperand(visitLoopControlVariableReference(forceImplicit: false));
			PushOperand(Visit(operation.InitialValue));
			if (isObjectLoop)
			{
				PushOperand(Visit(operation.LimitValue));
				PushOperand(Visit(operation.StepValue));
				IOperation operation2 = tryCallObjectForLoopControlHelper(operation.LoopControlVariable.Syntax, WellKnownMember.Microsoft_VisualBasic_CompilerServices_ObjectFlowControl_ForLoopControl__ForLoopInitObj);
				LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation2, null), jumpIfTrue: false, RegularBranch(@break));
				LinkBlocks(CurrentBasicBlock, bodyBlock);
				_currentBasicBlock = null;
			}
			else
			{
				SpillEvalStack();
				_ = _currentRegion;
				limitValueId = GetNextCaptureId(loopRegion);
				VisitAndCapture(operation.LimitValue, limitValueId);
				stepValueId = GetNextCaptureId(loopRegion);
				VisitAndCapture(operation.StepValue, stepValueId);
				IOperation operation3 = GetCaptureReference(stepValueId, operation.StepValue);
				if (userDefinedInfo != null)
				{
					_forToLoopBinaryOperatorLeftOperand = GetCaptureReference(stepValueId, operation.StepValue);
					_forToLoopBinaryOperatorRightOperand = GetCaptureReference(stepValueId, operation.StepValue);
					IOperation forToLoopBinaryOperatorRightOperand = Visit(userDefinedInfo.Subtraction.Value);
					_forToLoopBinaryOperatorLeftOperand = operation3;
					_forToLoopBinaryOperatorRightOperand = forToLoopBinaryOperatorRightOperand;
					int nextCaptureId = GetNextCaptureId(loopRegion);
					VisitAndCapture(userDefinedInfo.GreaterThanOrEqual.Value, nextCaptureId);
					positiveFlag = GetCaptureReference(nextCaptureId, userDefinedInfo.GreaterThanOrEqual.Value);
					_forToLoopBinaryOperatorLeftOperand = null;
					_forToLoopBinaryOperatorRightOperand = null;
				}
				else if (!operation.StepValue.ConstantValue.HasValue && !ITypeSymbolHelpers.IsSignedIntegralType(stepEnumUnderlyingTypeOrSelf) && !ITypeSymbolHelpers.IsUnsignedIntegralType(stepEnumUnderlyingTypeOrSelf))
				{
					IOperation operation4 = null;
					if (ITypeSymbolHelpers.IsNullableType(operation3.Type))
					{
						operation4 = MakeIsNullOperation(GetCaptureReference(stepValueId, operation.StepValue), booleanType);
						operation3 = UnwrapNullableValue(operation3);
					}
					ITypeSymbol enumUnderlyingTypeOrSelf = ITypeSymbolHelpers.GetEnumUnderlyingTypeOrSelf(operation3.Type);
					if (ITypeSymbolHelpers.IsNumericType(enumUnderlyingTypeOrSelf))
					{
						int nextCaptureId2 = GetNextCaptureId(loopRegion);
						BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
						IOperation operation5;
						if (operation4 != null)
						{
							BasicBlockBuilder basicBlockBuilder3 = new BasicBlockBuilder(BasicBlockKind.Block);
							LinkBlocks(CurrentBasicBlock, Operation.SetParentOperation(operation4, null), jumpIfTrue: false, RegularBranch(basicBlockBuilder3));
							_currentBasicBlock = null;
							operation5 = new LiteralExpression(null, operation3.Syntax, booleanType, false, isImplicit: true);
							AddStatement(new FlowCapture(nextCaptureId2, operation5.Syntax, operation5));
							LinkBlocks(CurrentBasicBlock, basicBlockBuilder2);
							AppendNewBlock(basicBlockBuilder3);
						}
						IOperation rightOperand = new LiteralExpression(null, operation3.Syntax, operation3.Type, ConstantValue.Default(enumUnderlyingTypeOrSelf.SpecialType).Value, isImplicit: true);
						operation5 = new BinaryOperatorExpression(BinaryOperatorKind.GreaterThanOrEqual, operation3, rightOperand, isLifted: false, isChecked: false, isCompareText: false, null, null, null, operation3.Syntax, booleanType, default, isImplicit: true);
						AddStatement(new FlowCapture(nextCaptureId2, operation5.Syntax, operation5));
						AppendNewBlock(basicBlockBuilder2);
						positiveFlag = GetCaptureReference(nextCaptureId2, operation5);
					}
				}
				IOperation value = PopOperand();
				AddStatement(new SimpleAssignmentExpression(PopOperand(), isRef: false, value, null, operation.InitialValue.Syntax, null, default, isImplicit: true));
			}
			PopStackFrameAndLeaveRegion(frame);
		}
		IOperation negateIfStepNegative(IOperation operand)
		{
			int num = stepEnumUnderlyingTypeOrSelf.SpecialType.VBForToShiftBits();
			LiteralExpression rightOperand = new LiteralExpression(null, operand.Syntax, _compilation.GetSpecialType(SpecialType.System_Int32), num, isImplicit: true);
			BinaryOperatorExpression leftOperand = new BinaryOperatorExpression(BinaryOperatorKind.RightShift, GetCaptureReference(stepValueId, operation.StepValue), rightOperand, isLifted: false, isChecked: false, isCompareText: false, null, null, null, operand.Syntax, operation.StepValue.Type, default, isImplicit: true);
			return new BinaryOperatorExpression(BinaryOperatorKind.ExclusiveOr, leftOperand, operand, isLifted: false, isChecked: false, isCompareText: false, null, null, null, operand.Syntax, operand.Type, default, isImplicit: true);
		}
		IOperation tryCallObjectForLoopControlHelper(SyntaxNode syntax, WellKnownMember helper)
		{
			bool flag = helper == WellKnownMember.Microsoft_VisualBasic_CompilerServices_ObjectFlowControl_ForLoopControl__ForLoopInitObj;
			LocalReferenceExpression value = new LocalReferenceExpression(loopObject, flag, null, operation.LoopControlVariable.Syntax, loopObject.Type, default, isImplicit: true);
			IMethodSymbol methodSymbol = (IMethodSymbol)_compilation.CommonGetWellKnownTypeMember(helper);
			int parametersCount = WellKnownMembers.GetDescriptor(helper).ParametersCount;
			if (methodSymbol == null)
			{
				ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(--parametersCount, null);
				instance[--parametersCount] = value;
				do
				{
					instance[--parametersCount] = PopOperand();
				}
				while (parametersCount != 0);
				return MakeInvalidOperation(operation.LimitValue.Syntax, booleanType, instance.ToImmutableAndFree());
			}
			ArrayBuilder<IArgumentOperation> instance2 = ArrayBuilder<IArgumentOperation>.GetInstance(parametersCount, null);
			instance2[--parametersCount] = new ArgumentOperation(visitLoopControlVariableReference(forceImplicit: true), ArgumentKind.Explicit, methodSymbol.Parameters[parametersCount], null, null, null, syntax, isImplicit: true);
			instance2[--parametersCount] = new ArgumentOperation(value, ArgumentKind.Explicit, methodSymbol.Parameters[parametersCount], null, null, null, syntax, isImplicit: true);
			do
			{
				IOperation operation2 = PopOperand();
				instance2[--parametersCount] = new ArgumentOperation(operation2, ArgumentKind.Explicit, methodSymbol.Parameters[parametersCount], null, null, null, flag ? operation2.Syntax : syntax, isImplicit: true);
			}
			while (parametersCount != 0);
			return new InvocationExpression(methodSymbol, null, isVirtual: false, instance2.ToImmutableAndFree(), null, operation.LimitValue.Syntax, methodSymbol.ReturnType, default, isImplicit: true);
		}
		IOperation visitLoopControlVariableReference(bool forceImplicit)
		{
			OperationKind kind = operation.LoopControlVariable.Kind;
			if (kind != OperationKind.VariableDeclarator)
			{
				_forceImplicit = forceImplicit;
				IOperation result = Visit(operation.LoopControlVariable);
				_forceImplicit = false;
				return result;
			}
			IVariableDeclaratorOperation variableDeclaratorOperation = (IVariableDeclaratorOperation)operation.LoopControlVariable;
			ILocalSymbol symbol = variableDeclaratorOperation.Symbol;
			return new LocalReferenceExpression(symbol, isDeclaration: true, null, variableDeclaratorOperation.Syntax, symbol.Type, default, isImplicit: true);
		}
	}

	private static FlowCaptureReference GetCaptureReference(int id, IOperation underlying)
	{
		return new FlowCaptureReference(id, underlying.Syntax, underlying.Type, underlying.ConstantValue);
	}

	internal override IOperation VisitAggregateQuery(IAggregateQueryOperation operation, int? captureIdForResult)
	{
		SpillEvalStack();
		IOperation currentAggregationGroup = _currentAggregationGroup;
		_currentAggregationGroup = VisitAndCapture(operation.Group);
		IOperation result = Visit(operation.Aggregation);
		_currentAggregationGroup = currentAggregationGroup;
		return result;
	}

	public override IOperation VisitSwitch(ISwitchOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		INamedTypeSymbol booleanType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		IOperation switchValue = VisitAndCapture(operation.Value);
		ImmutableArray<ILocalSymbol> locals = getLocals();
		RegionBuilder region = new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, locals);
		EnterRegion(region);
		BasicBlockBuilder defaultBody = null;
		BasicBlockBuilder @break = GetLabeledOrNewBlock(operation.ExitLabel);
		foreach (ISwitchCaseOperation @case in operation.Cases)
		{
			handleSection(@case);
		}
		if (defaultBody != null)
		{
			LinkBlocks(CurrentBasicBlock, defaultBody);
		}
		LeaveRegion();
		AppendNewBlock(@break);
		return FinishVisitingStatement(operation);
		ImmutableArray<ILocalSymbol> getLocals()
		{
			ImmutableArray<ILocalSymbol> immutableArray = operation.Locals;
			foreach (ISwitchCaseOperation case2 in operation.Cases)
			{
				immutableArray = immutableArray.Concat(case2.Locals);
			}
			return immutableArray;
		}
		void handleCase(ICaseClauseOperation caseClause, BasicBlockBuilder body, BasicBlockBuilder nextCase)
		{
			BasicBlockBuilder labeled = GetLabeledOrNewBlock(caseClause.Label);
			LinkBlocks(labeled, body);
			IOperation condition;
			switch (caseClause.CaseKind)
			{
			case CaseKind.SingleValue:
				handleEqualityCheck(((ISingleValueCaseClauseOperation)caseClause).Value);
				break;
			case CaseKind.Pattern:
			{
				IPatternCaseClauseOperation patternCaseClauseOperation = (IPatternCaseClauseOperation)caseClause;
				EvalStackFrame frame = PushStackFrame();
				PushOperand(OperationCloner.CloneOperation(switchValue));
				IPatternOperation pattern = (IPatternOperation)Visit(patternCaseClauseOperation.Pattern);
				condition = new IsPatternExpression(PopOperand(), pattern, null, patternCaseClauseOperation.Pattern.Syntax, booleanType, default, isImplicit: true);
				condition = Operation.SetParentOperation(condition, null);
				LinkBlocks(CurrentBasicBlock, condition, jumpIfTrue: false, RegularBranch(nextCase));
				PopStackFrameAndLeaveRegion(frame);
				if (patternCaseClauseOperation.Guard != null)
				{
					AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block));
					VisitConditionalBranch(patternCaseClauseOperation.Guard, ref nextCase, sense: false);
				}
				AppendNewBlock(labeled);
				_currentBasicBlock = null;
				break;
			}
			case CaseKind.Relational:
			{
				IRelationalCaseClauseOperation relationalCaseClauseOperation = (IRelationalCaseClauseOperation)caseClause;
				if (relationalCaseClauseOperation.Relation != BinaryOperatorKind.Equals)
				{
					throw ExceptionUtilities.UnexpectedValue(relationalCaseClauseOperation.Relation);
				}
				handleEqualityCheck(relationalCaseClauseOperation.Value);
				break;
			}
			case CaseKind.Default:
				_ = (IDefaultCaseClauseOperation)caseClause;
				if (defaultBody == null)
				{
					defaultBody = labeled;
				}
				LinkBlocks(CurrentBasicBlock, nextCase);
				AppendNewBlock(labeled);
				_currentBasicBlock = null;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(caseClause.CaseKind);
			}
			void handleEqualityCheck(IOperation compareWith)
			{
				bool flag = ITypeSymbolHelpers.IsNullableType(operation.Value.Type);
				bool flag2 = ITypeSymbolHelpers.IsNullableType(compareWith.Type);
				bool flag3 = flag | flag2;
				EvalStackFrame frame2 = PushStackFrame();
				PushOperand(OperationCloner.CloneOperation(switchValue));
				IOperation operation2 = Visit(compareWith);
				IOperation operation3 = PopOperand();
				if (flag3)
				{
					if (!flag)
					{
						if (operation3.Type != null)
						{
							operation3 = MakeNullable(operation3, compareWith.Type);
						}
					}
					else if (!flag2 && operation2.Type != null)
					{
						operation2 = MakeNullable(operation2, operation.Value.Type);
					}
				}
				condition = new BinaryOperatorExpression(BinaryOperatorKind.Equals, operation3, operation2, flag3, isChecked: false, isCompareText: false, null, null, null, compareWith.Syntax, booleanType, default, isImplicit: true);
				condition = Operation.SetParentOperation(condition, null);
				LinkBlocks(CurrentBasicBlock, condition, jumpIfTrue: false, RegularBranch(nextCase));
				PopStackFrameAndLeaveRegion(frame2);
				AppendNewBlock(labeled);
				_currentBasicBlock = null;
			}
		}
		void handleSection(ISwitchCaseOperation section)
		{
			BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
			BasicBlockBuilder dest = new BasicBlockBuilder(BasicBlockKind.Block);
			IOperation condition = ((BaseSwitchCase)section).Condition;
			if (condition != null)
			{
				_currentSwitchOperationExpression = switchValue;
				VisitConditionalBranch(condition, ref dest, sense: false);
				_currentSwitchOperationExpression = null;
			}
			else
			{
				foreach (ICaseClauseOperation clause in section.Clauses)
				{
					BasicBlockBuilder basicBlockBuilder2 = new BasicBlockBuilder(BasicBlockKind.Block);
					handleCase(clause, basicBlockBuilder, basicBlockBuilder2);
					AppendNewBlock(basicBlockBuilder2);
				}
				LinkBlocks(CurrentBasicBlock, dest);
			}
			AppendNewBlock(basicBlockBuilder);
			VisitStatements(section.Body);
			LinkBlocks(CurrentBasicBlock, @break);
			AppendNewBlock(dest);
		}
	}

	private IOperation MakeNullable(IOperation operand, ITypeSymbol type)
	{
		return CreateConversion(operand, type);
	}

	public override IOperation VisitSwitchCase(ISwitchCaseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitRelationalCaseClause(IRelationalCaseClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitRangeCaseClause(IRangeCaseClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitPatternCaseClause(IPatternCaseClauseOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitEnd(IEndOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		BasicBlockBuilder currentBasicBlock = CurrentBasicBlock;
		AppendNewBlock(new BasicBlockBuilder(BasicBlockKind.Block), linkToPrevious: false);
		currentBasicBlock.FallThrough.Kind = ControlFlowBranchSemantics.ProgramTermination;
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitForLoop(IForLoopOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals));
		ImmutableArray<IOperation> before = operation.Before;
		if (before.Length == 1 && before[0].Kind == OperationKind.VariableDeclarationGroup)
		{
			HandleVariableDeclarations((VariableDeclarationGroupOperation)before.Single());
		}
		else
		{
			VisitStatements(before);
		}
		BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
		AppendNewBlock(basicBlockBuilder);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.ConditionLocals));
		BasicBlockBuilder dest = GetLabeledOrNewBlock(operation.ExitLabel);
		if (operation.Condition != null)
		{
			VisitConditionalBranch(operation.Condition, ref dest, sense: false);
		}
		VisitStatement(operation.Body);
		BasicBlockBuilder labeledOrNewBlock = GetLabeledOrNewBlock(operation.ContinueLabel);
		AppendNewBlock(labeledOrNewBlock);
		VisitStatements(operation.AtLoopBottom);
		LinkBlocks(CurrentBasicBlock, basicBlockBuilder);
		LeaveRegion();
		LeaveRegion();
		AppendNewBlock(dest);
		return FinishVisitingStatement(operation);
	}

	internal override IOperation VisitFixed(IFixedOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, operation.Locals));
		HandleVariableDeclarations(operation.Variables);
		VisitStatement(operation.Body);
		LeaveRegion();
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		HandleVariableDeclarations(operation);
		return FinishVisitingStatement(operation);
	}

	private void HandleVariableDeclarations(IVariableDeclarationGroupOperation operation)
	{
		foreach (IVariableDeclarationOperation declaration in operation.Declarations)
		{
			HandleVariableDeclaration(declaration);
		}
	}

	private void HandleVariableDeclaration(IVariableDeclarationOperation operation)
	{
		foreach (IVariableDeclaratorOperation declarator in operation.Declarators)
		{
			HandleVariableDeclarator(operation, declarator);
		}
	}

	private void HandleVariableDeclarator(IVariableDeclarationOperation declaration, IVariableDeclaratorOperation declarator)
	{
		if (declarator.Initializer != null || declaration.Initializer != null)
		{
			ILocalSymbol symbol = declarator.Symbol;
			BasicBlockBuilder basicBlockBuilder = null;
			if (symbol.IsStatic)
			{
				basicBlockBuilder = new BasicBlockBuilder(BasicBlockKind.Block);
				ITypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
				StaticLocalInitializationSemaphoreOperation staticLocalInitializationSemaphoreOperation = new StaticLocalInitializationSemaphoreOperation(symbol, declarator.Syntax, specialType);
				Operation.SetParentOperation(staticLocalInitializationSemaphoreOperation, null);
				LinkBlocks(CurrentBasicBlock, staticLocalInitializationSemaphoreOperation, jumpIfTrue: false, RegularBranch(basicBlockBuilder));
				_currentBasicBlock = null;
				EnterRegion(new RegionBuilder(ControlFlowRegionKind.StaticLocalInitializer));
			}
			EvalStackFrame frame = PushStackFrame();
			IOperation operation = null;
			SyntaxNode syntax = null;
			if (declarator.Initializer != null)
			{
				operation = Visit(declarator.Initializer.Value);
				syntax = declarator.Syntax;
			}
			if (declaration.Initializer != null)
			{
				IOperation operation2 = Visit(declaration.Initializer.Value);
				syntax = declaration.Syntax;
				operation = ((operation == null) ? operation2 : new InvalidOperation(ImmutableArray.Create(operation, operation2), null, declaration.Syntax, symbol.Type, default, isImplicit: true));
			}
			LocalReferenceExpression localReferenceExpression = new LocalReferenceExpression(symbol, isDeclaration: true, null, declarator.Syntax, symbol.Type, default, isImplicit: true);
			SimpleAssignmentExpression statement = new SimpleAssignmentExpression(localReferenceExpression, symbol.IsRef, operation, null, syntax, localReferenceExpression.Type, default, isImplicit: true);
			AddStatement(statement);
			PopStackFrameAndLeaveRegion(frame);
			if (symbol.IsStatic)
			{
				LeaveRegion();
				AppendNewBlock(basicBlockBuilder);
			}
		}
	}

	public override IOperation VisitVariableDeclaration(IVariableDeclarationOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitVariableDeclarator(IVariableDeclaratorOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitVariableInitializer(IVariableInitializerOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitFlowCapture(IFlowCaptureOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitFlowCaptureReference(IFlowCaptureReferenceOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitIsNull(IIsNullOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitCaughtException(ICaughtExceptionOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitInvocation(IInvocationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		IOperation instance = (operation.TargetMethod.IsStatic ? null : operation.Instance);
		var (instance2, arguments) = VisitInstanceWithArguments(instance, operation.Arguments);
		PopStackFrame(frame);
		return new InvocationExpression(operation.TargetMethod, instance2, operation.IsVirtual, arguments, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	private (IOperation visitedInstance, ImmutableArray<IArgumentOperation> visitedArguments) VisitInstanceWithArguments(IOperation instance, ImmutableArray<IArgumentOperation> arguments)
	{
		if (instance != null)
		{
			PushOperand(Visit(instance));
		}
		ImmutableArray<IArgumentOperation> item = VisitArguments(arguments);
		return (visitedInstance: (instance == null) ? null : PopOperand(), visitedArguments: item);
	}

	internal override IOperation VisitNoPiaObjectCreation(INoPiaObjectCreationOperation operation, int? argument)
	{
		EvalStackFrame frame = PushStackFrame();
		IOperation objectCreation = new NoPiaObjectCreationOperation(null, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		return PopStackFrame(frame, HandleObjectOrCollectionInitializer(operation.Initializer, objectCreation));
	}

	public override IOperation VisitObjectCreation(IObjectCreationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		EvalStackFrame frame2 = PushStackFrame();
		ImmutableArray<IArgumentOperation> arguments = VisitArguments(operation.Arguments);
		PopStackFrame(frame2);
		IOperation objectCreation = new ObjectCreationExpression(operation.Constructor, null, arguments, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		return PopStackFrame(frame, HandleObjectOrCollectionInitializer(operation.Initializer, objectCreation));
	}

	public override IOperation VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		TypeParameterObjectCreationExpression objectCreation = new TypeParameterObjectCreationExpression(null, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		return PopStackFrame(frame, HandleObjectOrCollectionInitializer(operation.Initializer, objectCreation));
	}

	public override IOperation VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		EvalStackFrame frame2 = PushStackFrame();
		ImmutableArray<IOperation> arguments = VisitArray(operation.Arguments);
		PopStackFrame(frame2);
		HasDynamicArgumentsExpression hasDynamicArgumentsExpression = (HasDynamicArgumentsExpression)operation;
		IOperation objectCreation = new DynamicObjectCreationExpression(arguments, hasDynamicArgumentsExpression.ArgumentNames, hasDynamicArgumentsExpression.ArgumentRefKinds, null, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		return PopStackFrame(frame, HandleObjectOrCollectionInitializer(operation.Initializer, objectCreation));
	}

	private IOperation HandleObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation initializer, IOperation objectCreation)
	{
		if (initializer == null || initializer.Initializers.IsEmpty)
		{
			return objectCreation;
		}
		PushOperand(objectCreation);
		SpillEvalStack();
		objectCreation = PopOperand();
		visitInitializer(initializer, objectCreation);
		return objectCreation;
		void handleInitializer(IOperation innerInitializer)
		{
			switch (innerInitializer.Kind)
			{
			case OperationKind.MemberInitializer:
				handleMemberInitializer((IMemberInitializerOperation)innerInitializer);
				break;
			case OperationKind.SimpleAssignment:
				handleSimpleAssignment((ISimpleAssignmentOperation)innerInitializer);
				break;
			default:
			{
				EvalStackFrame frame = PushStackFrame();
				AddStatement(Visit(innerInitializer));
				PopStackFrameAndLeaveRegion(frame);
				break;
			}
			}
		}
		void handleMemberInitializer(IMemberInitializerOperation memberInitializer)
		{
			EvalStackFrame frame = PushStackFrame();
			IOperation initializedInstance = (tryPushTarget(memberInitializer.InitializedMember) ? popTarget(memberInitializer.InitializedMember) : Visit(memberInitializer.InitializedMember));
			visitInitializer(memberInitializer.Initializer, initializedInstance);
			PopStackFrameAndLeaveRegion(frame);
		}
		void handleSimpleAssignment(ISimpleAssignmentOperation assignmentOperation)
		{
			EvalStackFrame frame = PushStackFrame();
			IOperation statement;
			if (!tryPushTarget(assignmentOperation.Target))
			{
				statement = Visit(assignmentOperation);
			}
			else
			{
				IOperation value = Visit(assignmentOperation.Value);
				statement = new SimpleAssignmentExpression(popTarget(assignmentOperation.Target), assignmentOperation.IsRef, value, null, assignmentOperation.Syntax, assignmentOperation.Type, assignmentOperation.ConstantValue, IsImplicit(assignmentOperation));
			}
			AddStatement(statement);
			PopStackFrameAndLeaveRegion(frame);
		}
		IOperation popTarget(IOperation originalTarget)
		{
			switch (originalTarget.Kind)
			{
			case OperationKind.FieldReference:
			{
				IFieldReferenceOperation fieldReferenceOperation = (IFieldReferenceOperation)originalTarget;
				IOperation instance = ((!fieldReferenceOperation.Member.IsStatic && fieldReferenceOperation.Instance != null) ? PopOperand() : null);
				return new FieldReferenceExpression(fieldReferenceOperation.Field, fieldReferenceOperation.IsDeclaration, instance, null, fieldReferenceOperation.Syntax, fieldReferenceOperation.Type, fieldReferenceOperation.ConstantValue, IsImplicit(fieldReferenceOperation));
			}
			case OperationKind.EventReference:
			{
				IEventReferenceOperation eventReferenceOperation = (IEventReferenceOperation)originalTarget;
				IOperation instance = ((!eventReferenceOperation.Member.IsStatic && eventReferenceOperation.Instance != null) ? PopOperand() : null);
				return new EventReferenceExpression(eventReferenceOperation.Event, instance, null, eventReferenceOperation.Syntax, eventReferenceOperation.Type, eventReferenceOperation.ConstantValue, IsImplicit(eventReferenceOperation));
			}
			case OperationKind.PropertyReference:
			{
				IPropertyReferenceOperation propertyReferenceOperation = (IPropertyReferenceOperation)originalTarget;
				IOperation instance = ((!propertyReferenceOperation.Member.IsStatic && propertyReferenceOperation.Instance != null) ? PopOperand() : null);
				ImmutableArray<IArgumentOperation> arguments = PopArray(propertyReferenceOperation.Arguments, RewriteArgumentFromArray);
				return new PropertyReferenceExpression(propertyReferenceOperation.Property, instance, arguments, null, propertyReferenceOperation.Syntax, propertyReferenceOperation.Type, propertyReferenceOperation.ConstantValue, IsImplicit(propertyReferenceOperation));
			}
			case OperationKind.ArrayElementReference:
			{
				IArrayElementReferenceOperation arrayElementReferenceOperation = (IArrayElementReferenceOperation)originalTarget;
				IOperation instance = PopOperand();
				ImmutableArray<IOperation> indices = PopArray(arrayElementReferenceOperation.Indices);
				return new ArrayElementReferenceExpression(instance, indices, null, originalTarget.Syntax, originalTarget.Type, originalTarget.ConstantValue, IsImplicit(originalTarget));
			}
			case OperationKind.DynamicIndexerAccess:
			{
				BaseDynamicIndexerAccessExpression baseDynamicIndexerAccessExpression = (BaseDynamicIndexerAccessExpression)originalTarget;
				IOperation instance = ((baseDynamicIndexerAccessExpression.Operation != null) ? PopOperand() : null);
				ImmutableArray<IOperation> arguments2 = PopArray(baseDynamicIndexerAccessExpression.Arguments);
				return new DynamicIndexerAccessExpression(instance, arguments2, baseDynamicIndexerAccessExpression.ArgumentNames, baseDynamicIndexerAccessExpression.ArgumentRefKinds, null, baseDynamicIndexerAccessExpression.Syntax, baseDynamicIndexerAccessExpression.Type, baseDynamicIndexerAccessExpression.ConstantValue, IsImplicit(baseDynamicIndexerAccessExpression));
			}
			case OperationKind.DynamicMemberReference:
			{
				IDynamicMemberReferenceOperation dynamicMemberReferenceOperation = (IDynamicMemberReferenceOperation)originalTarget;
				IOperation instance = ((dynamicMemberReferenceOperation.Instance != null) ? PopOperand() : null);
				return new DynamicMemberReferenceExpression(instance, dynamicMemberReferenceOperation.MemberName, dynamicMemberReferenceOperation.TypeArguments, dynamicMemberReferenceOperation.ContainingType, null, dynamicMemberReferenceOperation.Syntax, dynamicMemberReferenceOperation.Type, dynamicMemberReferenceOperation.ConstantValue, IsImplicit(dynamicMemberReferenceOperation));
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(originalTarget.Kind);
			}
		}
		bool tryPushTarget(IOperation instance)
		{
			switch (instance.Kind)
			{
			case OperationKind.FieldReference:
			case OperationKind.PropertyReference:
			case OperationKind.EventReference:
			{
				IMemberReferenceOperation memberReferenceOperation = (IMemberReferenceOperation)instance;
				if (memberReferenceOperation.Kind == OperationKind.PropertyReference)
				{
					VisitAndPushArray(((IPropertyReferenceOperation)memberReferenceOperation).Arguments, UnwrapArgument);
					SpillEvalStack();
				}
				if (!memberReferenceOperation.Member.IsStatic && memberReferenceOperation.Instance != null)
				{
					PushOperand(Visit(memberReferenceOperation.Instance));
				}
				return true;
			}
			case OperationKind.ArrayElementReference:
			{
				IArrayElementReferenceOperation arrayElementReferenceOperation = (IArrayElementReferenceOperation)instance;
				VisitAndPushArray(arrayElementReferenceOperation.Indices);
				SpillEvalStack();
				PushOperand(Visit(arrayElementReferenceOperation.ArrayReference));
				return true;
			}
			case OperationKind.DynamicIndexerAccess:
			{
				IDynamicIndexerAccessOperation dynamicIndexerAccessOperation = (IDynamicIndexerAccessOperation)instance;
				VisitAndPushArray(dynamicIndexerAccessOperation.Arguments);
				SpillEvalStack();
				if (dynamicIndexerAccessOperation.Operation != null)
				{
					PushOperand(Visit(dynamicIndexerAccessOperation.Operation));
				}
				return true;
			}
			case OperationKind.DynamicMemberReference:
			{
				IDynamicMemberReferenceOperation dynamicMemberReferenceOperation = (IDynamicMemberReferenceOperation)instance;
				if (dynamicMemberReferenceOperation.Instance != null)
				{
					PushOperand(Visit(dynamicMemberReferenceOperation.Instance));
				}
				return true;
			}
			default:
				return false;
			}
		}
		void visitInitializer(IObjectOrCollectionInitializerOperation initializerOperation, IOperation initializedInstance)
		{
			ImplicitInstanceInfo currentImplicitInstance = _currentImplicitInstance;
			_currentImplicitInstance = new ImplicitInstanceInfo(initializedInstance);
			foreach (IOperation initializer2 in initializerOperation.Initializers)
			{
				handleInitializer(initializer2);
			}
			_currentImplicitInstance = currentImplicitInstance;
		}
	}

	public override IOperation VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, int? captureIdForResult)
	{
		return MakeInvalidOperation(operation.Syntax, operation.Type, ImmutableArray<IOperation>.Empty);
	}

	public override IOperation VisitMemberInitializer(IMemberInitializerOperation operation, int? captureIdForResult)
	{
		return MakeInvalidOperation(operation.Syntax, operation.Type, ImmutableArray<IOperation>.Empty);
	}

	public override IOperation VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, int? captureIdForResult)
	{
		if (operation.Initializers.IsEmpty)
		{
			return new AnonymousObjectCreationExpression(ImmutableArray<IOperation>.Empty, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		}
		ImplicitInstanceInfo currentImplicitInstance = _currentImplicitInstance;
		_currentImplicitInstance = new ImplicitInstanceInfo((INamedTypeSymbol)operation.Type);
		SpillEvalStack();
		EvalStackFrame frame = PushStackFrame();
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(operation.Initializers.Length);
		for (int i = 0; i < operation.Initializers.Length; i++)
		{
			ISimpleAssignmentOperation simpleAssignmentOperation = (ISimpleAssignmentOperation)operation.Initializers[i];
			IPropertyReferenceOperation propertyReferenceOperation = (IPropertyReferenceOperation)simpleAssignmentOperation.Target;
			InstanceReferenceExpression instance2 = new InstanceReferenceExpression(InstanceReferenceKind.ImplicitReceiver, null, propertyReferenceOperation.Instance.Syntax, propertyReferenceOperation.Instance.Type, propertyReferenceOperation.Instance.ConstantValue, IsImplicit(propertyReferenceOperation.Instance));
			SimpleAssignmentExpression item = new SimpleAssignmentExpression(new PropertyReferenceExpression(propertyReferenceOperation.Property, instance2, ImmutableArray<IArgumentOperation>.Empty, null, propertyReferenceOperation.Syntax, propertyReferenceOperation.Type, propertyReferenceOperation.ConstantValue, IsImplicit(propertyReferenceOperation)), value: visitAndCaptureInitializer(propertyReferenceOperation.Property, simpleAssignmentOperation.Value), isRef: simpleAssignmentOperation.IsRef, semanticModel: null, syntax: simpleAssignmentOperation.Syntax, type: simpleAssignmentOperation.Type, constantValue: simpleAssignmentOperation.ConstantValue, isImplicit: IsImplicit(simpleAssignmentOperation));
			instance.Add(item);
		}
		_currentImplicitInstance.Free();
		_currentImplicitInstance = currentImplicitInstance;
		for (int j = 0; j < instance.Count; j++)
		{
			PopOperand();
		}
		PopStackFrame(frame);
		return new AnonymousObjectCreationExpression(instance.ToImmutableAndFree(), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		IOperation visitAndCaptureInitializer(IPropertySymbol initializedProperty, IOperation initializer)
		{
			PushOperand(Visit(initializer));
			SpillEvalStack();
			IOperation operation2 = PeekOperand();
			_currentImplicitInstance.AnonymousTypePropertyValues[initializedProperty] = operation2;
			return operation2;
		}
	}

	public override IOperation VisitLocalFunction(ILocalFunctionOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		RegionBuilder regionBuilder = _currentRegion;
		while (regionBuilder.IsStackSpillRegion)
		{
			regionBuilder = regionBuilder.Enclosing;
		}
		regionBuilder.Add(operation.Symbol, operation);
		return FinishVisitingStatement(operation);
	}

	private IOperation VisitLocalFunctionAsRoot(ILocalFunctionOperation operation)
	{
		VisitMethodBodies(operation.Body, operation.IgnoredBody);
		return null;
	}

	public override IOperation VisitAnonymousFunction(IAnonymousFunctionOperation operation, int? captureIdForResult)
	{
		_haveAnonymousFunction = true;
		return new FlowAnonymousFunctionOperation(GetCurrentContext(), operation, IsImplicit(operation));
	}

	public override IOperation VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitArrayCreation(IArrayCreationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		VisitAndPushArray(operation.DimensionSizes);
		IArrayInitializerOperation initializer = (IArrayInitializerOperation)Visit(operation.Initializer);
		ImmutableArray<IOperation> dimensionSizes = PopArray(operation.DimensionSizes);
		PopStackFrame(frame);
		return new ArrayCreationExpression(dimensionSizes, initializer, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitArrayInitializer(IArrayInitializerOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		visitAndPushArrayInitializerValues(operation);
		return PopStackFrame(frame, popAndAssembleArrayInitializerValues(operation));
		IArrayInitializerOperation popAndAssembleArrayInitializerValues(IArrayInitializerOperation initializer)
		{
			ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(initializer.ElementValues.Length);
			for (int num = initializer.ElementValues.Length - 1; num >= 0; num--)
			{
				IOperation operation2 = initializer.ElementValues[num];
				IOperation item = ((operation2.Kind != OperationKind.ArrayInitializer) ? PopOperand() : popAndAssembleArrayInitializerValues((IArrayInitializerOperation)operation2));
				instance.Add(item);
			}
			instance.ReverseContents();
			return new ArrayInitializer(instance.ToImmutableAndFree(), null, initializer.Syntax, initializer.ConstantValue, IsImplicit(initializer));
		}
		void visitAndPushArrayInitializerValues(IArrayInitializerOperation initializer)
		{
			foreach (IOperation elementValue in initializer.ElementValues)
			{
				if (elementValue.Kind == OperationKind.ArrayInitializer)
				{
					visitAndPushArrayInitializerValues((IArrayInitializerOperation)elementValue);
				}
				else
				{
					PushOperand(Visit(elementValue));
				}
			}
		}
	}

	public override IOperation VisitInstanceReference(IInstanceReferenceOperation operation, int? captureIdForResult)
	{
		if (operation.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
		{
			if (_currentImplicitInstance.ImplicitInstance != null)
			{
				return OperationCloner.CloneOperation(_currentImplicitInstance.ImplicitInstance);
			}
			return MakeInvalidOperation(operation.Syntax, operation.Type, ImmutableArray<IOperation>.Empty);
		}
		return new InstanceReferenceExpression(operation.ReferenceKind, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDynamicInvocation(IDynamicInvocationOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		if (operation.Operation != null)
		{
			if (operation.Operation.Kind == OperationKind.DynamicMemberReference)
			{
				IOperation instance = ((IDynamicMemberReferenceOperation)operation.Operation).Instance;
				if (instance != null)
				{
					PushOperand(Visit(instance));
				}
			}
			else
			{
				PushOperand(Visit(operation.Operation));
			}
		}
		ImmutableArray<IOperation> arguments = VisitArray(operation.Arguments);
		IOperation operation2;
		if (operation.Operation == null)
		{
			operation2 = null;
		}
		else if (operation.Operation.Kind == OperationKind.DynamicMemberReference)
		{
			IDynamicMemberReferenceOperation dynamicMemberReferenceOperation = (IDynamicMemberReferenceOperation)operation.Operation;
			operation2 = new DynamicMemberReferenceExpression((dynamicMemberReferenceOperation.Instance != null) ? PopOperand() : null, dynamicMemberReferenceOperation.MemberName, dynamicMemberReferenceOperation.TypeArguments, dynamicMemberReferenceOperation.ContainingType, null, dynamicMemberReferenceOperation.Syntax, dynamicMemberReferenceOperation.Type, dynamicMemberReferenceOperation.ConstantValue, IsImplicit(dynamicMemberReferenceOperation));
		}
		else
		{
			operation2 = PopOperand();
		}
		PopStackFrame(frame);
		return new DynamicInvocationExpression(operation2, arguments, ((HasDynamicArgumentsExpression)operation).ArgumentNames, ((HasDynamicArgumentsExpression)operation).ArgumentRefKinds, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, int? captureIdForResult)
	{
		if (operation.Operation != null)
		{
			PushOperand(Visit(operation.Operation));
		}
		ImmutableArray<IOperation> arguments = VisitArray(operation.Arguments);
		return new DynamicIndexerAccessExpression((operation.Operation != null) ? PopOperand() : null, arguments, ((HasDynamicArgumentsExpression)operation).ArgumentNames, ((HasDynamicArgumentsExpression)operation).ArgumentRefKinds, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, int? captureIdForResult)
	{
		return new DynamicMemberReferenceExpression(Visit(operation.Instance), operation.MemberName, operation.TypeArguments, operation.ContainingType, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, int? captureIdForResult)
	{
		var (target, value) = VisitPreservingTupleOperations(operation.Target, operation.Value);
		return new DeconstructionAssignmentExpression(target, value, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	private void PushTargetAndUnwrapTupleIfNecessary(IOperation value)
	{
		if (value.Kind == OperationKind.Tuple)
		{
			foreach (IOperation element in ((ITupleOperation)value).Elements)
			{
				PushTargetAndUnwrapTupleIfNecessary(element);
			}
		}
		else
		{
			PushOperand(Visit(value));
		}
	}

	private IOperation PopTargetAndWrapTupleIfNecessary(IOperation value)
	{
		if (value.Kind == OperationKind.Tuple)
		{
			ITupleOperation tupleOperation = (ITupleOperation)value;
			int length = tupleOperation.Elements.Length;
			ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(length);
			for (int num = length - 1; num >= 0; num--)
			{
				instance.Add(PopTargetAndWrapTupleIfNecessary(tupleOperation.Elements[num]));
			}
			instance.ReverseContents();
			return new TupleExpression(instance.ToImmutableAndFree(), null, tupleOperation.Syntax, tupleOperation.Type, tupleOperation.NaturalType, tupleOperation.ConstantValue, IsImplicit(tupleOperation));
		}
		return PopOperand();
	}

	public override IOperation VisitDeclarationExpression(IDeclarationExpressionOperation operation, int? captureIdForResult)
	{
		return new DeclarationExpression(VisitPreservingTupleOperations(operation.Expression), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	private IOperation VisitPreservingTupleOperations(IOperation operation)
	{
		EvalStackFrame frame = PushStackFrame();
		PushTargetAndUnwrapTupleIfNecessary(operation);
		return PopStackFrame(frame, PopTargetAndWrapTupleIfNecessary(operation));
	}

	private (IOperation visitedLeft, IOperation visitedRight) VisitPreservingTupleOperations(IOperation left, IOperation right)
	{
		EvalStackFrame frame = PushStackFrame();
		PushTargetAndUnwrapTupleIfNecessary(left);
		IOperation item = Visit(right);
		IOperation item2 = PopTargetAndWrapTupleIfNecessary(left);
		PopStackFrame(frame);
		return (visitedLeft: item2, visitedRight: item);
	}

	public override IOperation VisitTuple(ITupleOperation operation, int? captureIdForResult)
	{
		return VisitPreservingTupleOperations(operation);
	}

	internal override IOperation VisitNoneOperation(IOperation operation, int? captureIdForResult)
	{
		if (_currentStatement == operation)
		{
			return VisitNoneOperationStatement(operation);
		}
		return VisitNoneOperationExpression(operation);
	}

	private IOperation VisitNoneOperationStatement(IOperation operation)
	{
		VisitStatements(operation.Children);
		return Operation.CreateOperationNone(null, operation.Syntax, operation.ConstantValue, ImmutableArray<IOperation>.Empty, IsImplicit(operation));
	}

	private IOperation VisitNoneOperationExpression(IOperation operation)
	{
		return PopStackFrame(PushStackFrame(), Operation.CreateOperationNone(null, operation.Syntax, operation.ConstantValue, VisitArray(operation.Children.ToImmutableArray()), IsImplicit(operation)));
	}

	public override IOperation VisitInterpolatedString(IInterpolatedStringOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		foreach (IInterpolatedStringContentOperation part in operation.Parts)
		{
			if (part.Kind == OperationKind.Interpolation)
			{
				IInterpolationOperation interpolationOperation = (IInterpolationOperation)part;
				PushOperand(Visit(interpolationOperation.Expression));
				if (interpolationOperation.Alignment != null)
				{
					PushOperand(Visit(interpolationOperation.Alignment));
				}
			}
		}
		ArrayBuilder<IInterpolatedStringContentOperation> instance = ArrayBuilder<IInterpolatedStringContentOperation>.GetInstance(operation.Parts.Length);
		for (int num = operation.Parts.Length - 1; num >= 0; num--)
		{
			IInterpolatedStringContentOperation interpolatedStringContentOperation = operation.Parts[num];
			IInterpolatedStringContentOperation item;
			if (interpolatedStringContentOperation.Kind == OperationKind.Interpolation)
			{
				IInterpolationOperation interpolationOperation2 = (IInterpolationOperation)interpolatedStringContentOperation;
				IOperation formatString = ((interpolationOperation2.FormatString == null) ? null : VisitLiteral((ILiteralOperation)interpolationOperation2.FormatString, null));
				IOperation alignment = ((interpolationOperation2.Alignment != null) ? PopOperand() : null);
				item = new Interpolation(PopOperand(), alignment, formatString, null, interpolatedStringContentOperation.Syntax, interpolatedStringContentOperation.Type, interpolatedStringContentOperation.ConstantValue, IsImplicit(interpolatedStringContentOperation));
			}
			else
			{
				IInterpolatedStringTextOperation interpolatedStringTextOperation = (IInterpolatedStringTextOperation)interpolatedStringContentOperation;
				item = new InterpolatedStringText(VisitLiteral((ILiteralOperation)interpolatedStringTextOperation.Text, null), null, interpolatedStringContentOperation.Syntax, interpolatedStringContentOperation.Type, interpolatedStringContentOperation.ConstantValue, IsImplicit(interpolatedStringContentOperation));
			}
			instance.Add(item);
		}
		instance.ReverseContents();
		PopStackFrame(frame);
		return new InterpolatedStringExpression(instance.ToImmutableAndFree(), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitInterpolation(IInterpolationOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitNameOf(INameOfOperation operation, int? captureIdForResult)
	{
		return new LiteralExpression(null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitLiteral(ILiteralOperation operation, int? captureIdForResult)
	{
		return new LiteralExpression(null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitLocalReference(ILocalReferenceOperation operation, int? captureIdForResult)
	{
		return new LocalReferenceExpression(operation.Local, operation.IsDeclaration, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitParameterReference(IParameterReferenceOperation operation, int? captureIdForResult)
	{
		return new ParameterReferenceExpression(operation.Parameter, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitFieldReference(IFieldReferenceOperation operation, int? captureIdForResult)
	{
		IOperation instance = (operation.Field.IsStatic ? null : Visit(operation.Instance));
		return new FieldReferenceExpression(operation.Field, operation.IsDeclaration, instance, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitMethodReference(IMethodReferenceOperation operation, int? captureIdForResult)
	{
		IOperation instance = (operation.Method.IsStatic ? null : Visit(operation.Instance));
		return new MethodReferenceExpression(operation.Method, operation.IsVirtual, instance, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitPropertyReference(IPropertyReferenceOperation operation, int? captureIdForResult)
	{
		if (operation.Instance is IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ImplicitReceiver } && operation.Property.ContainingType.IsAnonymousType && operation.Property.ContainingType == _currentImplicitInstance.AnonymousType)
		{
			if (_currentImplicitInstance.AnonymousTypePropertyValues.TryGetValue(operation.Property, out var value))
			{
				if (!(value is IFlowCaptureReferenceOperation { Id: var id }))
				{
					return OperationCloner.CloneOperation(value);
				}
				return GetCaptureReference(id.Value, operation);
			}
			return MakeInvalidOperation(operation.Syntax, operation.Type, ImmutableArray<IOperation>.Empty);
		}
		EvalStackFrame frame = PushStackFrame();
		IOperation instance = (operation.Property.IsStatic ? null : operation.Instance);
		var (instance2, arguments) = VisitInstanceWithArguments(instance, operation.Arguments);
		PopStackFrame(frame);
		return new PropertyReferenceExpression(operation.Property, instance2, arguments, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitEventReference(IEventReferenceOperation operation, int? captureIdForResult)
	{
		IOperation instance = (operation.Event.IsStatic ? null : Visit(operation.Instance));
		return new EventReferenceExpression(operation.Event, instance, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitTypeOf(ITypeOfOperation operation, int? captureIdForResult)
	{
		return new TypeOfExpression(operation.TypeOperand, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitParenthesized(IParenthesizedOperation operation, int? captureIdForResult)
	{
		return new ParenthesizedExpression(Visit(operation.Operand), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitAwait(IAwaitOperation operation, int? captureIdForResult)
	{
		return new AwaitExpression(Visit(operation.Operation), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitSizeOf(ISizeOfOperation operation, int? captureIdForResult)
	{
		return new SizeOfExpression(operation.TypeOperand, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitStop(IStopOperation operation, int? captureIdForResult)
	{
		return new StopStatement(null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitIsType(IIsTypeOperation operation, int? captureIdForResult)
	{
		return new IsTypeExpression(Visit(operation.ValueOperand), operation.TypeOperand, operation.IsNegated, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitParameterInitializer(IParameterInitializerOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		ParameterReferenceExpression rewrittenTarget = new ParameterReferenceExpression(operation.Parameter, null, operation.Syntax, operation.Parameter.Type, default, isImplicit: true);
		VisitInitializer(rewrittenTarget, operation);
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitFieldInitializer(IFieldInitializerOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		foreach (IFieldSymbol initializedField in operation.InitializedFields)
		{
			IInstanceReferenceOperation instance = (initializedField.IsStatic ? null : new InstanceReferenceExpression(InstanceReferenceKind.ContainingTypeInstance, null, operation.Syntax, initializedField.ContainingType, default, isImplicit: true));
			FieldReferenceExpression rewrittenTarget = new FieldReferenceExpression(initializedField, isDeclaration: false, instance, null, operation.Syntax, initializedField.Type, default, isImplicit: true);
			VisitInitializer(rewrittenTarget, operation);
		}
		return FinishVisitingStatement(operation);
	}

	public override IOperation VisitPropertyInitializer(IPropertyInitializerOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		foreach (IPropertySymbol initializedProperty in operation.InitializedProperties)
		{
			InstanceReferenceExpression instance = (initializedProperty.IsStatic ? null : new InstanceReferenceExpression(InstanceReferenceKind.ContainingTypeInstance, null, operation.Syntax, initializedProperty.ContainingType, default, isImplicit: true));
			ImmutableArray<IArgumentOperation> arguments;
			if (!initializedProperty.Parameters.IsEmpty)
			{
				ArrayBuilder<IArgumentOperation> instance2 = ArrayBuilder<IArgumentOperation>.GetInstance(initializedProperty.Parameters.Length);
				foreach (IParameterSymbol parameter in initializedProperty.Parameters)
				{
					ArgumentOperation item = new ArgumentOperation(new InvalidOperation(ImmutableArray<IOperation>.Empty, null, operation.Syntax, parameter.Type, default, isImplicit: true), ArgumentKind.Explicit, parameter, null, null, null, operation.Syntax, isImplicit: true);
					instance2.Add(item);
				}
				arguments = instance2.ToImmutableAndFree();
			}
			else
			{
				arguments = ImmutableArray<IArgumentOperation>.Empty;
			}
			IOperation rewrittenTarget = new PropertyReferenceExpression(initializedProperty, instance, arguments, null, operation.Syntax, initializedProperty.Type, default, isImplicit: true);
			VisitInitializer(rewrittenTarget, operation);
		}
		return FinishVisitingStatement(operation);
	}

	private void VisitInitializer(IOperation rewrittenTarget, ISymbolInitializerOperation initializer)
	{
		EnterRegion(new RegionBuilder(ControlFlowRegionKind.LocalLifetime, null, initializer.Locals));
		EvalStackFrame frame = PushStackFrame();
		SimpleAssignmentExpression statement = new SimpleAssignmentExpression(rewrittenTarget, isRef: false, Visit(initializer.Value), null, initializer.Syntax, rewrittenTarget.Type, default, isImplicit: true);
		AddStatement(statement);
		PopStackFrameAndLeaveRegion(frame);
		LeaveRegion();
	}

	public override IOperation VisitEventAssignment(IEventAssignmentOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		IEventReferenceOperation eventReferenceOperation = getEventReference();
		IOperation handlerValue;
		IOperation eventReference;
		if (eventReferenceOperation != null)
		{
			IOperation operation2 = (eventReferenceOperation.Event.IsStatic ? null : eventReferenceOperation.Instance);
			if (operation2 != null)
			{
				PushOperand(Visit(operation2));
			}
			handlerValue = Visit(operation.HandlerValue);
			IOperation instance = ((operation2 == null) ? null : PopOperand());
			eventReference = new EventReferenceExpression(eventReferenceOperation.Event, instance, null, operation.EventReference.Syntax, operation.EventReference.Type, operation.EventReference.ConstantValue, IsImplicit(operation.EventReference));
		}
		else
		{
			PushOperand(Visit(operation.EventReference));
			handlerValue = Visit(operation.HandlerValue);
			eventReference = PopOperand();
		}
		PopStackFrame(frame);
		return new EventAssignmentOperation(eventReference, handlerValue, operation.Adds, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
		IEventReferenceOperation getEventReference()
		{
			IOperation operation3 = operation.EventReference;
			while (true)
			{
				switch (operation3.Kind)
				{
				case OperationKind.EventReference:
					return (IEventReferenceOperation)operation3;
				case OperationKind.Parenthesized:
					break;
				default:
					return null;
				}
				operation3 = ((IParenthesizedOperation)operation3).Operand;
			}
		}
	}

	public override IOperation VisitRaiseEvent(IRaiseEventOperation operation, int? captureIdForResult)
	{
		StartVisitingStatement(operation);
		EvalStackFrame frame = PushStackFrame();
		IOperation operation2 = (operation.EventReference.Event.IsStatic ? null : operation.EventReference.Instance);
		if (operation2 != null)
		{
			PushOperand(Visit(operation2));
		}
		ImmutableArray<IArgumentOperation> arguments = VisitArguments(operation.Arguments);
		IOperation instance = ((operation2 == null) ? null : PopOperand());
		EventReferenceExpression eventReference = new EventReferenceExpression(operation.EventReference.Event, instance, null, operation.EventReference.Syntax, operation.EventReference.Type, operation.EventReference.ConstantValue, IsImplicit(operation.EventReference));
		PopStackFrame(frame);
		return FinishVisitingStatement(operation, new RaiseEventStatement(eventReference, arguments, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation)));
	}

	public override IOperation VisitAddressOf(IAddressOfOperation operation, int? captureIdForResult)
	{
		return new AddressOfExpression(Visit(operation.Reference), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, int? captureIdForResult)
	{
		return new IncrementExpression(operation.Kind == OperationKind.Decrement, operation.IsPostfix, operation.IsLifted, operation.IsChecked, Visit(operation.Target), operation.OperatorMethod, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDiscardOperation(IDiscardOperation operation, int? captureIdForResult)
	{
		return new DiscardOperation(operation.DiscardSymbol, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitOmittedArgument(IOmittedArgumentOperation operation, int? captureIdForResult)
	{
		return new OmittedArgumentExpression(null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	internal override IOperation VisitPlaceholder(IPlaceholderOperation operation, int? captureIdForResult)
	{
		switch (operation.PlaceholderKind)
		{
		case PlaceholderKind.SwitchOperationExpression:
			if (_currentSwitchOperationExpression != null)
			{
				return OperationCloner.CloneOperation(_currentSwitchOperationExpression);
			}
			break;
		case PlaceholderKind.ForToLoopBinaryOperatorLeftOperand:
			if (_forToLoopBinaryOperatorLeftOperand != null)
			{
				return _forToLoopBinaryOperatorLeftOperand;
			}
			break;
		case PlaceholderKind.ForToLoopBinaryOperatorRightOperand:
			if (_forToLoopBinaryOperatorRightOperand != null)
			{
				return _forToLoopBinaryOperatorRightOperand;
			}
			break;
		case PlaceholderKind.AggregationGroup:
			if (_currentAggregationGroup != null)
			{
				return OperationCloner.CloneOperation(_currentAggregationGroup);
			}
			break;
		}
		return new PlaceholderExpression(operation.PlaceholderKind, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitConversion(IConversionOperation operation, int? captureIdForResult)
	{
		return new ConversionOperation(Visit(operation.Operand), ((BaseConversionExpression)operation).ConvertibleConversion, operation.IsTryCast, operation.IsChecked, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDefaultValue(IDefaultValueOperation operation, int? captureIdForResult)
	{
		return new DefaultValueExpression(null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitIsPattern(IIsPatternOperation operation, int? captureIdForResult)
	{
		EvalStackFrame frame = PushStackFrame();
		PushOperand(Visit(operation.Value));
		IPatternOperation pattern = (IPatternOperation)Visit(operation.Pattern);
		IOperation value = PopOperand();
		PopStackFrame(frame);
		return new IsPatternExpression(value, pattern, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitInvalid(IInvalidOperation operation, int? captureIdForResult)
	{
		ArrayBuilder<IOperation> children = ArrayBuilder<IOperation>.GetInstance();
		children.AddRange(operation.Children);
		if (children.Count != 0 && children.Last().Kind == OperationKind.ObjectOrCollectionInitializer)
		{
			SpillEvalStack();
			EvalStackFrame frame = PushStackFrame();
			IObjectOrCollectionInitializerOperation initializer = (IObjectOrCollectionInitializerOperation)children.Last();
			children.RemoveLast();
			EvalStackFrame frame2 = PushStackFrame();
			foreach (IOperation item in children)
			{
				PushOperand(Visit(item));
			}
			for (int num = children.Count - 1; num >= 0; num--)
			{
				children[num] = PopOperand();
			}
			PopStackFrame(frame2);
			IOperation objectCreation = new InvalidOperation(children.ToImmutableAndFree(), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
			objectCreation = HandleObjectOrCollectionInitializer(initializer, objectCreation);
			PopStackFrame(frame);
			return objectCreation;
		}
		IOperation result = ((_currentStatement != operation) ? visitInvalidOperationExpression(operation) : visitInvalidOperationStatement(operation));
		children.Free();
		return result;
		IOperation visitInvalidOperationExpression(IInvalidOperation invalidOperation)
		{
			return PopStackFrame(PushStackFrame(), new InvalidOperation(VisitArray(invalidOperation.Children.ToImmutableArray()), null, invalidOperation.Syntax, invalidOperation.Type, invalidOperation.ConstantValue, IsImplicit(operation)));
		}
		IOperation visitInvalidOperationStatement(IInvalidOperation invalidOperation)
		{
			VisitStatements(children);
			return new InvalidOperation(ImmutableArray<IOperation>.Empty, null, invalidOperation.Syntax, invalidOperation.Type, invalidOperation.ConstantValue, IsImplicit(invalidOperation));
		}
	}

	public override IOperation VisitTranslatedQuery(ITranslatedQueryOperation operation, int? captureIdForResult)
	{
		return new TranslatedQueryExpression(Visit(operation.Operation), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitConstantPattern(IConstantPatternOperation operation, int? captureIdForResult)
	{
		return new ConstantPattern(Visit(operation.Value), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDeclarationPattern(IDeclarationPatternOperation operation, int? captureIdForResult)
	{
		return new DeclarationPattern(operation.DeclaredSymbol, null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public override IOperation VisitDelegateCreation(IDelegateCreationOperation operation, int? captureIdForResult)
	{
		return new DelegateCreationExpression(Visit(operation.Target), null, operation.Syntax, operation.Type, operation.ConstantValue, IsImplicit(operation));
	}

	public IOperation Visit(IOperation operation)
	{
		return Visit(operation, null);
	}

	public override IOperation Visit(IOperation operation, int? argument)
	{
		if (operation == null)
		{
			return null;
		}
		return PopStackFrame(PushStackFrame(), base.Visit(operation, argument));
	}

	public override IOperation DefaultVisit(IOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IOperation VisitArgument(IArgumentOperation operation, int? captureIdForResult)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
