namespace Microsoft.CodeAnalysis;

internal abstract class ErrorLogger
{
	public abstract void LogDiagnostic(Diagnostic diagnostic);
}
