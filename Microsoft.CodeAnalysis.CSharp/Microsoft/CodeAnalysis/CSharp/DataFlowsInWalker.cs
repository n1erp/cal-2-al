using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DataFlowsInWalker : AbstractRegionDataFlowPass
{
	private readonly HashSet<Symbol> _dataFlowsIn = new HashSet<Symbol>();

	private DataFlowsInWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> unassignedVariables, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes)
		: base(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, unassignedVariableAddressOfSyntaxes)
	{
	}

	internal static HashSet<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> unassignedVariables, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes, out bool? succeeded)
	{
		DataFlowsInWalker dataFlowsInWalker = new DataFlowsInWalker(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, unassignedVariableAddressOfSyntaxes);
		try
		{
			bool badRegion = false;
			HashSet<Symbol> hashSet = dataFlowsInWalker.Analyze(ref badRegion);
			succeeded = !badRegion;
			return badRegion ? new HashSet<Symbol>() : hashSet;
		}
		finally
		{
			dataFlowsInWalker.Free();
		}
	}

	private new HashSet<Symbol> Analyze(ref bool badRegion)
	{
		Analyze(ref badRegion, null);
		return _dataFlowsIn;
	}

	private LocalState ResetState(LocalState state)
	{
		bool flag = !state.Reachable;
		state = ReachableState();
		if (flag)
		{
			state.Assign(0);
		}
		return state;
	}

	protected override void EnterRegion()
	{
		State = ResetState(State);
		_dataFlowsIn.Clear();
		base.EnterRegion();
	}

	protected override void NoteBranch(PendingBranch pending, BoundNode gotoStmt, BoundStatement targetStmt)
	{
		if (!gotoStmt.WasCompilerGenerated && !targetStmt.WasCompilerGenerated && !RegionContains(gotoStmt.Syntax.Span) && RegionContains(targetStmt.Syntax.Span))
		{
			pending.State = ResetState(pending.State);
		}
		base.NoteBranch(pending, gotoStmt, targetStmt);
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		if (IsInside && !RegionContains(node.RangeVariableSymbol.Locations[0].SourceSpan))
		{
			_dataFlowsIn.Add(node.RangeVariableSymbol);
		}
		return null;
	}

	protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (RegionContains(node.Span))
		{
			_dataFlowsIn.Add((symbol.Kind == SymbolKind.Field) ? GetNonFieldSymbol(slot) : symbol);
		}
		base.ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
	}

	protected override void ReportUnassignedOutParameter(ParameterSymbol parameter, SyntaxNode node, Location location)
	{
		if (node != null && node is ReturnStatementSyntax && RegionContains(node.Span))
		{
			_dataFlowsIn.Add(parameter);
		}
		base.ReportUnassignedOutParameter(parameter, node, location);
	}
}
