namespace Microsoft.CodeAnalysis.CSharp;

internal struct SingleLookupResult
{
	internal readonly LookupResultKind Kind;

	internal readonly Symbol Symbol;

	internal readonly DiagnosticInfo Error;

	internal SingleLookupResult(LookupResultKind kind, Symbol symbol, DiagnosticInfo error)
	{
		Kind = kind;
		Symbol = symbol;
		Error = error;
	}
}
