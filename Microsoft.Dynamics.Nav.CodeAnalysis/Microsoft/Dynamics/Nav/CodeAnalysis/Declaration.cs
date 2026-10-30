using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class Declaration
{
	public string Name { get; }

	public ImmutableArray<Declaration> Children => GetDeclarationChildren();

	public abstract DeclarationKind Kind { get; }

	protected Declaration(string name)
	{
		Name = name;
	}

	protected abstract ImmutableArray<Declaration> GetDeclarationChildren();
}
