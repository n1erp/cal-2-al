using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SingleNamespaceDeclaration : SingleNamespaceOrTypeDeclaration
{
	private readonly ImmutableArray<SingleNamespaceOrTypeDeclaration> _children;

	public override DeclarationKind Kind => DeclarationKind.Namespace;

	public virtual bool HasUsings => false;

	public virtual bool HasExternAliases => false;

	protected SingleNamespaceDeclaration(string name, SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<SingleNamespaceOrTypeDeclaration> children, ImmutableArray<Diagnostic> diagnostics)
		: base(name, syntaxReference, nameLocation, diagnostics)
	{
		_children = children;
	}

	protected override ImmutableArray<SingleNamespaceOrTypeDeclaration> GetNamespaceOrTypeDeclarationChildren()
	{
		return _children;
	}

	public static SingleNamespaceDeclaration Create(string name, bool hasUsings, bool hasExternAliases, SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<SingleNamespaceOrTypeDeclaration> children, ImmutableArray<Diagnostic> diagnostics)
	{
		if (!hasUsings && !hasExternAliases)
		{
			return new SingleNamespaceDeclaration(name, syntaxReference, nameLocation, children, diagnostics);
		}
		return new SingleNamespaceDeclarationEx(name, hasUsings, hasExternAliases, syntaxReference, nameLocation, children, diagnostics);
	}
}
