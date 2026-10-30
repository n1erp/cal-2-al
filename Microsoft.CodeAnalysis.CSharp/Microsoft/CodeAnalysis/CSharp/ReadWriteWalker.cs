using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ReadWriteWalker : AbstractRegionDataFlowPass
{
	private readonly HashSet<Symbol> _readInside = new HashSet<Symbol>();

	private readonly HashSet<Symbol> _writtenInside = new HashSet<Symbol>();

	private readonly HashSet<Symbol> _readOutside = new HashSet<Symbol>();

	private readonly HashSet<Symbol> _writtenOutside = new HashSet<Symbol>();

	internal static void Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes, out IEnumerable<Symbol> readInside, out IEnumerable<Symbol> writtenInside, out IEnumerable<Symbol> readOutside, out IEnumerable<Symbol> writtenOutside, out IEnumerable<Symbol> captured, out IEnumerable<Symbol> unsafeAddressTaken, out IEnumerable<Symbol> capturedInside, out IEnumerable<Symbol> capturedOutside)
	{
		ReadWriteWalker readWriteWalker = new ReadWriteWalker(compilation, member, node, firstInRegion, lastInRegion, unassignedVariableAddressOfSyntaxes);
		try
		{
			bool badRegion = false;
			readWriteWalker.Analyze(ref badRegion);
			if (badRegion)
			{
				readInside = (writtenInside = (readOutside = (writtenOutside = (captured = (unsafeAddressTaken = (capturedInside = (capturedOutside = Enumerable.Empty<Symbol>())))))));
				return;
			}
			readInside = readWriteWalker._readInside;
			writtenInside = readWriteWalker._writtenInside;
			readOutside = readWriteWalker._readOutside;
			writtenOutside = readWriteWalker._writtenOutside;
			captured = readWriteWalker.GetCaptured();
			capturedInside = readWriteWalker.GetCapturedInside();
			capturedOutside = readWriteWalker.GetCapturedOutside();
			unsafeAddressTaken = readWriteWalker.GetUnsafeAddressTaken();
		}
		finally
		{
			readWriteWalker.Free();
		}
	}

	private ReadWriteWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes)
		: base(compilation, member, node, firstInRegion, lastInRegion, null, unassignedVariableAddressOfSyntaxes)
	{
	}

	protected override void EnterRegion()
	{
		MethodSymbol methodSymbol = currentSymbol as MethodSymbol;
		while ((object)methodSymbol != null)
		{
			foreach (ParameterSymbol parameter in methodSymbol.Parameters)
			{
				if (parameter.RefKind != RefKind.None)
				{
					_readOutside.Add(parameter);
				}
			}
			ParameterSymbol thisParameter = methodSymbol.ThisParameter;
			if ((object)thisParameter != null && thisParameter.RefKind != RefKind.None)
			{
				_readOutside.Add(thisParameter);
			}
			methodSymbol = methodSymbol.ContainingSymbol as MethodSymbol;
		}
		base.EnterRegion();
	}

	protected override void NoteRead(Symbol variable, ParameterSymbol rangeVariableUnderlyingParameter = null)
	{
		if ((object)variable != null)
		{
			if (variable.Kind != SymbolKind.Field)
			{
				(IsInside ? _readInside : _readOutside).Add(variable);
			}
			base.NoteRead(variable, rangeVariableUnderlyingParameter);
		}
	}

	protected override void NoteWrite(Symbol variable, BoundExpression value, bool read)
	{
		if ((object)variable != null)
		{
			(IsInside ? _writtenInside : _writtenOutside).Add(variable);
			base.NoteWrite(variable, value, read);
		}
	}

	protected override void CheckAssigned(BoundExpression expr, FieldSymbol fieldSymbol, SyntaxNode node)
	{
		base.CheckAssigned(expr, fieldSymbol, node);
		if (!IsInside && node.Span.Contains(RegionSpan) && expr.Kind == BoundKind.FieldAccess)
		{
			NoteReceiverRead((BoundFieldAccess)expr);
		}
	}

	private void NoteReceiverWritten(BoundFieldAccess expr)
	{
		NoteReceiverReadOrWritten(expr, _writtenInside);
	}

	private void NoteReceiverRead(BoundFieldAccess expr)
	{
		NoteReceiverReadOrWritten(expr, _readInside);
	}

	private void NoteReceiverReadOrWritten(BoundFieldAccess expr, HashSet<Symbol> readOrWritten)
	{
		if (expr.FieldSymbol.IsStatic || expr.FieldSymbol.ContainingType.IsReferenceType)
		{
			return;
		}
		BoundExpression receiverOpt = expr.ReceiverOpt;
		if (receiverOpt == null)
		{
			return;
		}
		SyntaxNode syntax = receiverOpt.Syntax;
		if (syntax == null)
		{
			return;
		}
		switch (receiverOpt.Kind)
		{
		case BoundKind.Local:
			if (RegionContains(syntax.Span))
			{
				readOrWritten.Add(((BoundLocal)receiverOpt).LocalSymbol);
			}
			break;
		case BoundKind.ThisReference:
			if (RegionContains(syntax.Span))
			{
				readOrWritten.Add(MethodThisParameter);
			}
			break;
		case BoundKind.BaseReference:
			if (RegionContains(syntax.Span))
			{
				readOrWritten.Add(MethodThisParameter);
			}
			break;
		case BoundKind.Parameter:
			if (RegionContains(syntax.Span))
			{
				readOrWritten.Add(((BoundParameter)receiverOpt).ParameterSymbol);
			}
			break;
		case BoundKind.RangeVariable:
			if (RegionContains(syntax.Span))
			{
				readOrWritten.Add(((BoundRangeVariable)receiverOpt).RangeVariableSymbol);
			}
			break;
		case BoundKind.FieldAccess:
			if (receiverOpt.Type.IsStructType() && syntax.Span.OverlapsWith(RegionSpan))
			{
				NoteReceiverReadOrWritten(receiverOpt as BoundFieldAccess, readOrWritten);
			}
			break;
		}
	}

	protected override void AssignImpl(BoundNode node, BoundExpression value, bool isRef, bool written, bool read)
	{
		switch (node.Kind)
		{
		case BoundKind.RangeVariable:
			if (written)
			{
				NoteWrite(((BoundRangeVariable)node).RangeVariableSymbol, value, read);
			}
			break;
		case BoundKind.QueryClause:
		{
			base.AssignImpl(node, value, isRef, written, read);
			RangeVariableSymbol definedSymbol = ((BoundQueryClause)node).DefinedSymbol;
			if ((object)definedSymbol != null && written)
			{
				NoteWrite(definedSymbol, value, read);
			}
			break;
		}
		case BoundKind.FieldAccess:
		{
			base.AssignImpl(node, value, isRef, written, read);
			BoundFieldAccess expr = node as BoundFieldAccess;
			if (!IsInside && node.Syntax != null && node.Syntax.Span.Contains(RegionSpan))
			{
				NoteReceiverWritten(expr);
			}
			break;
		}
		default:
			base.AssignImpl(node, value, isRef, written, read);
			break;
		}
	}

	public override BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		return VisitLambda(node.BindForErrorRecovery());
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		ParameterSymbol rangeVariableUnderlyingParameter = GetRangeVariableUnderlyingParameter(node.Value);
		NoteRead(node.RangeVariableSymbol, rangeVariableUnderlyingParameter);
		return null;
	}

	private static ParameterSymbol GetRangeVariableUnderlyingParameter(BoundNode underlying)
	{
		while (underlying != null)
		{
			switch (underlying.Kind)
			{
			case BoundKind.Parameter:
				return ((BoundParameter)underlying).ParameterSymbol;
			case BoundKind.PropertyAccess:
				break;
			default:
				return null;
			}
			underlying = ((BoundPropertyAccess)underlying).ReceiverOpt;
		}
		return null;
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		Assign(node, null);
		return base.VisitQueryClause(node);
	}
}
