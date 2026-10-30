using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct SingleLookupResult
{
	internal readonly LookupResultKind Kind;

	internal readonly Symbol Symbol;

	internal readonly BoundExpression Receiver;

	internal readonly DiagnosticInfo Error;

	internal SingleLookupResult(LookupResultKind kind, Symbol symbol, DiagnosticInfo error, BoundExpression receiver = null)
	{
		Kind = kind;
		Symbol = symbol;
		Error = error;
		Receiver = receiver;
	}
}
