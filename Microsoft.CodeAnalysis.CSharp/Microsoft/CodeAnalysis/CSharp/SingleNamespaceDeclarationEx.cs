using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SingleNamespaceDeclarationEx : SingleNamespaceDeclaration
{
	private readonly bool _hasUsings;

	private readonly bool _hasExternAliases;

	public override bool HasUsings => _hasUsings;

	public override bool HasExternAliases => _hasExternAliases;

	public SingleNamespaceDeclarationEx(string name, bool hasUsings, bool hasExternAliases, SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<SingleNamespaceOrTypeDeclaration> children, ImmutableArray<Diagnostic> diagnostics)
		: base(name, syntaxReference, nameLocation, children, diagnostics)
	{
		_hasUsings = hasUsings;
		_hasExternAliases = hasExternAliases;
	}
}
