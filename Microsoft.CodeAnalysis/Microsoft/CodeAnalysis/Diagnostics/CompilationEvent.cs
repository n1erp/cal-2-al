namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class CompilationEvent
{
	public Compilation Compilation { get; }

	internal CompilationEvent(Compilation compilation)
	{
		Compilation = compilation;
	}

	public virtual void FlushCache()
	{
	}
}
