using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class LabelCollector : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	protected HashSet<LabelSymbol> currentLabels;

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		CollectLabel(node.Label);
		return base.VisitLabelStatement(node);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		CollectLabel(node.ConstantTargetOpt);
		CollectLabel(node.BreakLabel);
		return base.VisitSwitchStatement(node);
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		CollectLabel(node.Label);
		return base.VisitSwitchLabel(node);
	}

	private void CollectLabel(LabelSymbol label)
	{
		if ((object)label != null)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			if (hashSet == null)
			{
				hashSet = (currentLabels = new HashSet<LabelSymbol>());
			}
			hashSet.Add(label);
		}
	}
}
