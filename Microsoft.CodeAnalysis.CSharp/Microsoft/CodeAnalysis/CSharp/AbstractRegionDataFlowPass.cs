using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class AbstractRegionDataFlowPass : DataFlowPass
{
	internal AbstractRegionDataFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> initiallyAssignedVariables = null, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes = null, bool trackUnassignments = false)
		: base(compilation, member, node, firstInRegion, lastInRegion, initiallyAssignedVariables, unassignedVariableAddressOfSyntaxes, trackUnassignments)
	{
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		SetState(ReachableState());
		MakeSlots(MethodParameters);
		if ((object)MethodThisParameter != null)
		{
			GetOrCreateSlot(MethodThisParameter);
		}
		return base.Scan(ref badRegion);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		MakeSlots(node.Symbol.Parameters);
		return base.VisitLambda(node);
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MakeSlots(node.Symbol.Parameters);
		return base.VisitLocalFunctionStatement(node);
	}

	private void MakeSlots(ImmutableArray<ParameterSymbol> parameters)
	{
		foreach (ParameterSymbol item in parameters)
		{
			GetOrCreateSlot(item);
		}
	}
}
