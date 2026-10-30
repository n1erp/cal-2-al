using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UnmatchedGotoFinder : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private readonly Dictionary<BoundNode, HashSet<LabelSymbol>> _unmatchedLabelsCache;

	private HashSet<LabelSymbol> _gotos;

	private HashSet<LabelSymbol> _targets;

	private UnmatchedGotoFinder(Dictionary<BoundNode, HashSet<LabelSymbol>> unmatchedLabelsCache, int recursionDepth)
		: base(recursionDepth)
	{
		_unmatchedLabelsCache = unmatchedLabelsCache;
	}

	public static HashSet<LabelSymbol> Find(BoundNode node, Dictionary<BoundNode, HashSet<LabelSymbol>> unmatchedLabelsCache, int recursionDepth)
	{
		UnmatchedGotoFinder unmatchedGotoFinder = new UnmatchedGotoFinder(unmatchedLabelsCache, recursionDepth);
		unmatchedGotoFinder.Visit(node);
		HashSet<LabelSymbol> gotos = unmatchedGotoFinder._gotos;
		HashSet<LabelSymbol> targets = unmatchedGotoFinder._targets;
		if (gotos != null && targets != null)
		{
			gotos.RemoveAll(targets);
		}
		return gotos;
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (node != null && _unmatchedLabelsCache.TryGetValue(node, out var value))
		{
			if (value != null)
			{
				foreach (LabelSymbol item in value)
				{
					AddGoto(item);
				}
			}
			return null;
		}
		return base.Visit(node);
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		AddGoto(node.Label);
		return base.VisitGotoStatement(node);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		AddGoto(node.Label);
		return base.VisitConditionalGoto(node);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		AddTarget(node.Label);
		return base.VisitLabelStatement(node);
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		AddTarget(node.Label);
		return base.VisitLabeledStatement(node);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		AddTarget(node.BreakLabel);
		return base.VisitSwitchStatement(node);
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		AddTarget(node.Label);
		return base.VisitSwitchLabel(node);
	}

	private void AddGoto(LabelSymbol label)
	{
		if (_gotos == null)
		{
			_gotos = new HashSet<LabelSymbol>();
		}
		_gotos.Add(label);
	}

	private void AddTarget(LabelSymbol label)
	{
		if (_targets == null)
		{
			_targets = new HashSet<LabelSymbol>();
		}
		_targets.Add(label);
	}
}
