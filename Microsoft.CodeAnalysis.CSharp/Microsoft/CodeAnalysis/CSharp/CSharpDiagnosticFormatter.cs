namespace Microsoft.CodeAnalysis.CSharp;

public class CSharpDiagnosticFormatter : DiagnosticFormatter
{
	public new static CSharpDiagnosticFormatter Instance { get; } = new CSharpDiagnosticFormatter();

	internal CSharpDiagnosticFormatter()
	{
	}
}
