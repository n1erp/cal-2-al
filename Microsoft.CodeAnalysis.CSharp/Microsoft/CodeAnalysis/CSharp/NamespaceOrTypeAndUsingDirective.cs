using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct NamespaceOrTypeAndUsingDirective(NamespaceOrTypeSymbol namespaceOrType, UsingDirectiveSyntax usingDirective)
{
	public readonly NamespaceOrTypeSymbol NamespaceOrType = namespaceOrType;

	public readonly UsingDirectiveSyntax UsingDirective = usingDirective;
}
