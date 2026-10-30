namespace Microsoft.CodeAnalysis.Diagnostics;

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
