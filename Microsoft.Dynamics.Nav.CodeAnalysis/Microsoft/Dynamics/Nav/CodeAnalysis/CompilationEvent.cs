namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class CompilationEvent
{
	public Compilation Compilation { get; private set; }

	internal CompilationEvent(Compilation compilation)
	{
		Compilation = compilation;
	}

	public virtual void FlushCache()
	{
	}
}
