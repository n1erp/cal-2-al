namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationStartedEvent : CompilationEvent
{
	public CompilationStartedEvent(Compilation compilation)
		: base(compilation)
	{
	}

	public override string ToString()
	{
		return "CompilationStartedEvent";
	}
}
