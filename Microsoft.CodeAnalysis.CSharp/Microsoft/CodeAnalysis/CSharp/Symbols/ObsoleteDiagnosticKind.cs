namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal enum ObsoleteDiagnosticKind
{
	NotObsolete,
	Suppressed,
	Diagnostic,
	Lazy,
	LazyPotentiallySuppressed
}
