namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationCompletedEvent : CompilationEvent
{
	public CompilationCompletedEvent(Compilation compilation)
		: base(compilation)
	{
	}

	public override string ToString()
	{
		return "CompilationCompletedEvent";
	}
}
