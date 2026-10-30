using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class BuckStopsHereBinder : Binder
{
	internal override bool IsInMethodBody => false;

	internal override GeneratedLabelSymbol BreakLabel => null;

	internal override Symbol ContainingMember => null;

	internal override ImmutableArray<VariableSymbol> Locals
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal BuckStopsHereBinder(Compilation compilation)
		: base(compilation)
	{
	}

	internal override Binder GetBinder(SyntaxNode node)
	{
		return null;
	}

	internal override BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundForEachStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundRepeatStatement BindRepeatParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundCaseStatement BindCaseParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundWithStatement BindWithParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, LookupOptions options, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		failedThroughTypeCheck = false;
		return Binder.IsSymbolAccessibleConditional(symbol, Compilation.CompiledModule, options, ref useSiteDiagnostics);
	}
}
