using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

public sealed class BasicBlock
{
	private ControlFlowBranch _lazySuccessor;

	private ControlFlowBranch _lazyConditionalSuccessor;

	private ImmutableArray<ControlFlowBranch> _lazyPredecessors;

	public BasicBlockKind Kind { get; }

	public ImmutableArray<IOperation> Operations { get; }

	public IOperation BranchValue { get; }

	public ControlFlowConditionKind ConditionKind { get; }

	public ControlFlowBranch FallThroughSuccessor => _lazySuccessor;

	public ControlFlowBranch ConditionalSuccessor => _lazyConditionalSuccessor;

	public ImmutableArray<ControlFlowBranch> Predecessors => _lazyPredecessors;

	public int Ordinal { get; }

	public bool IsReachable { get; }

	public ControlFlowRegion EnclosingRegion { get; }

	internal BasicBlock(BasicBlockKind kind, ImmutableArray<IOperation> operations, IOperation branchValue, ControlFlowConditionKind conditionKind, int ordinal, bool isReachable, ControlFlowRegion region)
	{
		Kind = kind;
		Operations = operations;
		BranchValue = branchValue;
		ConditionKind = conditionKind;
		Ordinal = ordinal;
		IsReachable = isReachable;
		EnclosingRegion = region;
	}

	internal void SetSuccessors(ControlFlowBranch successor, ControlFlowBranch conditionalSuccessor)
	{
		_lazySuccessor = successor;
		_lazyConditionalSuccessor = conditionalSuccessor;
	}

	internal void SetPredecessors(ImmutableArray<ControlFlowBranch> predecessors)
	{
		_lazyPredecessors = predecessors;
	}
}
