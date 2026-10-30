using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DataFlowsOutWalker : AbstractRegionDataFlowPass
{
	private readonly ImmutableArray<ISymbol> _dataFlowsIn;

	private readonly HashSet<Symbol> _dataFlowsOut = new HashSet<Symbol>();

	private DataFlowsOutWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> unassignedVariables, ImmutableArray<ISymbol> dataFlowsIn)
		: base(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, null, trackUnassignments: true)
	{
		_dataFlowsIn = dataFlowsIn;
	}

	internal static HashSet<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> unassignedVariables, ImmutableArray<ISymbol> dataFlowsIn)
	{
		DataFlowsOutWalker dataFlowsOutWalker = new DataFlowsOutWalker(compilation, member, node, firstInRegion, lastInRegion, unassignedVariables, dataFlowsIn);
		try
		{
			bool badRegion = false;
			HashSet<Symbol> hashSet = dataFlowsOutWalker.Analyze(ref badRegion);
			return badRegion ? new HashSet<Symbol>() : hashSet;
		}
		finally
		{
			dataFlowsOutWalker.Free();
		}
	}

	private new HashSet<Symbol> Analyze(ref bool badRegion)
	{
		Analyze(ref badRegion, null);
		return _dataFlowsOut;
	}

	protected override void EnterRegion()
	{
		foreach (Symbol item in _dataFlowsIn)
		{
			int orCreateSlot = GetOrCreateSlot(item);
			if (orCreateSlot > 0 && !State.IsAssigned(orCreateSlot))
			{
				_dataFlowsOut.Add(item);
			}
		}
		base.EnterRegion();
	}

	protected override void NoteWrite(Symbol variable, BoundExpression value, bool read)
	{
		if (State.Reachable && IsInside)
		{
			ParameterSymbol parameterSymbol = variable as ParameterSymbol;
			if (FlowsOut(parameterSymbol))
			{
				_dataFlowsOut.Add(parameterSymbol);
			}
		}
		base.NoteWrite(variable, value, read);
	}

	protected override void AssignImpl(BoundNode node, BoundExpression value, bool isRef, bool written, bool read)
	{
		if (IsInside)
		{
			written = false;
			if (State.Reachable)
			{
				ParameterSymbol parameterSymbol = Param(node);
				if (FlowsOut(parameterSymbol))
				{
					_dataFlowsOut.Add(parameterSymbol);
				}
			}
		}
		base.AssignImpl(node, value, isRef, written, read);
	}

	private bool FlowsOut(ParameterSymbol param)
	{
		if ((object)param != null && param.RefKind != RefKind.None && !param.IsImplicitlyDeclared)
		{
			return !RegionContains(param.Locations[0].SourceSpan);
		}
		return false;
	}

	private ParameterSymbol Param(BoundNode node)
	{
		return node.Kind switch
		{
			BoundKind.Parameter => ((BoundParameter)node).ParameterSymbol, 
			BoundKind.ThisReference => MethodThisParameter, 
			_ => null, 
		};
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		return base.VisitQueryClause(node);
	}

	protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (!IsInside)
		{
			_dataFlowsOut.Add((symbol.Kind == SymbolKind.Field) ? GetNonFieldSymbol(slot) : symbol);
		}
		base.ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
	}

	protected override void ReportUnassignedOutParameter(ParameterSymbol parameter, SyntaxNode node, Location location)
	{
		if (!_dataFlowsOut.Contains(parameter) && (node == null || node is ReturnStatementSyntax))
		{
			_dataFlowsOut.Add(parameter);
		}
		base.ReportUnassignedOutParameter(parameter, node, location);
	}
}
