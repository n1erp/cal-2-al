namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal abstract class CompilerTextWriter
{
	public abstract void WriteLine();

	public abstract void WriteLine(string text);

	public abstract void WriteLine(string text, params object[] args);
}
