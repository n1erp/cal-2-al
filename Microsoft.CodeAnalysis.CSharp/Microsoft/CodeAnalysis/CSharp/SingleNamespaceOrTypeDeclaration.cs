using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class SingleNamespaceOrTypeDeclaration : Declaration
{
	private readonly SyntaxReference _syntaxReference;

	private readonly SourceLocation _nameLocation;

	public readonly ImmutableArray<Diagnostic> Diagnostics;

	public SourceLocation Location => new SourceLocation(SyntaxReference);

	public SyntaxReference SyntaxReference => _syntaxReference;

	public SourceLocation NameLocation => _nameLocation;

	public new ImmutableArray<SingleNamespaceOrTypeDeclaration> Children => GetNamespaceOrTypeDeclarationChildren();

	protected SingleNamespaceOrTypeDeclaration(string name, SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<Diagnostic> diagnostics)
		: base(name)
	{
		_syntaxReference = syntaxReference;
		_nameLocation = nameLocation;
		Diagnostics = diagnostics;
	}

	protected override ImmutableArray<Declaration> GetDeclarationChildren()
	{
		return StaticCast<Declaration>.From(GetNamespaceOrTypeDeclarationChildren());
	}

	protected abstract ImmutableArray<SingleNamespaceOrTypeDeclaration> GetNamespaceOrTypeDeclarationChildren();
}
