using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public abstract class MetadataReferenceResolver
{
	public virtual bool ResolveMissingAssemblies => false;

	public abstract override bool Equals(object other);

	public abstract override int GetHashCode();

	public abstract ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties);

	public virtual PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
	{
		return null;
	}
}
