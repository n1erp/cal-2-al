using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal class UnassignedVariablesWalker : DataFlowPass
{
	private readonly HashSet<Symbol> _result = new HashSet<Symbol>();

	private UnassignedVariablesWalker(CSharpCompilation compilation, Symbol member, BoundNode node)
		: base(compilation, member, node, new NeverEmptyStructTypeCache())
	{
	}

	internal static HashSet<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, bool convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = false)
	{
		UnassignedVariablesWalker unassignedVariablesWalker = new UnassignedVariablesWalker(compilation, member, node);
		if (convertInsufficientExecutionStackExceptionToCancelledByStackGuardException)
		{
			unassignedVariablesWalker._convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = true;
		}
		try
		{
			bool badRegion = false;
			HashSet<Symbol> hashSet = unassignedVariablesWalker.Analyze(ref badRegion);
			return badRegion ? new HashSet<Symbol>() : hashSet;
		}
		finally
		{
			unassignedVariablesWalker.Free();
		}
	}

	private new HashSet<Symbol> Analyze(ref bool badRegion)
	{
		Analyze(ref badRegion, null);
		return _result;
	}

	protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (symbol.Kind != SymbolKind.Field)
		{
			_result.Add(symbol);
			return;
		}
		_result.Add(GetNonFieldSymbol(slot));
		base.ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
	}

	protected override void ReportUnassignedOutParameter(ParameterSymbol parameter, SyntaxNode node, Location location)
	{
		_result.Add(parameter);
		base.ReportUnassignedOutParameter(parameter, node, location);
	}
}
