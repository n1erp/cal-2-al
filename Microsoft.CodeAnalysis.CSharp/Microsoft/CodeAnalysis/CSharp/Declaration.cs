using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class Declaration
{
	protected readonly string name;

	public string Name => name;

	public ImmutableArray<Declaration> Children => GetDeclarationChildren();

	public abstract DeclarationKind Kind { get; }

	protected Declaration(string name)
	{
		this.name = name;
	}

	protected abstract ImmutableArray<Declaration> GetDeclarationChildren();
}
