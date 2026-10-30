using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ExitPointsWalker : AbstractRegionControlFlowPass
{
	private readonly ArrayBuilder<LabelSymbol> _labelsInside;

	private ArrayBuilder<StatementSyntax> _branchesOutOf;

	private ExitPointsWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
		_labelsInside = new ArrayBuilder<LabelSymbol>();
		_branchesOutOf = ArrayBuilder<StatementSyntax>.GetInstance();
	}

	protected override void Free()
	{
		if (_branchesOutOf != null)
		{
			_branchesOutOf.Free();
		}
		_labelsInside.Free();
		base.Free();
	}

	internal static IEnumerable<StatementSyntax> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		ExitPointsWalker exitPointsWalker = new ExitPointsWalker(compilation, member, node, firstInRegion, lastInRegion);
		try
		{
			bool badRegion = false;
			exitPointsWalker.Analyze(ref badRegion);
			ImmutableArray<StatementSyntax> immutableArray = exitPointsWalker._branchesOutOf.ToImmutableAndFree();
			exitPointsWalker._branchesOutOf = null;
			IEnumerable<StatementSyntax> result;
			if (!badRegion)
			{
				IEnumerable<StatementSyntax> enumerable = immutableArray;
				result = enumerable;
			}
			else
			{
				result = SpecializedCollections.EmptyEnumerable<StatementSyntax>();
			}
			return result;
		}
		finally
		{
			exitPointsWalker.Free();
		}
	}

	private new void Analyze(ref bool badRegion)
	{
		Scan(ref badRegion);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.Label);
		}
		return base.VisitLabelStatement(node);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitDoStatement(node);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitForEachStatement(node);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitForStatement(node);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
		}
		return base.VisitSwitchStatement(node);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		if (IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
		}
		return base.VisitWhileStatement(node);
	}

	protected override void EnterRegion()
	{
		base.EnterRegion();
	}

	protected override void LeaveRegion()
	{
		foreach (PendingBranch pendingBranch in PendingBranches)
		{
			if (pendingBranch.Branch == null || !RegionContains(pendingBranch.Branch.Syntax.Span))
			{
				continue;
			}
			switch (pendingBranch.Branch.Kind)
			{
			case BoundKind.GotoStatement:
				if (_labelsInside.Contains(((BoundGotoStatement)pendingBranch.Branch).Label))
				{
					continue;
				}
				break;
			case BoundKind.BreakStatement:
				if (_labelsInside.Contains(((BoundBreakStatement)pendingBranch.Branch).Label))
				{
					continue;
				}
				break;
			case BoundKind.ContinueStatement:
				if (_labelsInside.Contains(((BoundContinueStatement)pendingBranch.Branch).Label))
				{
					continue;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(pendingBranch.Branch.Kind);
			case BoundKind.ReturnStatement:
			case BoundKind.YieldBreakStatement:
				break;
			case BoundKind.AwaitExpression:
			case BoundKind.YieldReturnStatement:
				continue;
			}
			_branchesOutOf.Add((StatementSyntax)pendingBranch.Branch.Syntax);
		}
		base.LeaveRegion();
	}
}
